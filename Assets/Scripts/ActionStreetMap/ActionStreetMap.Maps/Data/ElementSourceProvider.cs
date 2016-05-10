﻿using System;
using System.IO;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Config;
using ActionStreetMap.Infrastructure.Dependencies;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.Formats.Json;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Data
{
    /// <summary> Provides the way to get the corresponding element source by geocoordinate. </summary>
    public interface IElementSourceProvider: IDisposable
    {
        /// <summary> Adds element source. </summary>
        void Add(IElementSource elementSource);

        /// <summary> Returns element sources by query represented by bounding box. </summary>
        /// <returns>Element source.</returns>
        IObservable<IElementSource> Get(BoundingBox query);
    }

    /// <summary> Default implementation of <see cref="IElementSourceProvider"/>. </summary>
    internal sealed class ElementSourceProvider : IElementSourceProvider, IConfigurable
    {
        private const string Category = "mapdata.source";
        private const string CacheFileNameExtension = ".osm";

        // settings
        private string _mapDataServerUri;
        private string _mapDataServerQuery;
        private string _mapDataFormat;
        private string _indexSettingsPath;
        private string _cachePath;

        private readonly IPathResolver _pathResolver;
        private readonly IFileSystemService _fileSystemService;
        private readonly IObjectPool _objectPool;

        private IndexSettings _settings;
        private RTree<TreeNode> _tree;

        /// <summary> Trace. </summary>
        [global::System.Reflection.Obfuscation(Exclude=true, Feature="renaming")]
        [Dependency]
        public ITrace Trace { get; set; }

        /// <summary> Creates instance of <see cref="ElementSourceProvider"/>. </summary>
        /// <param name="pathResolver">Path resolver.</param>
        /// <param name="fileSystemService">File system service.</param>
        /// <param name="objectPool">Object pool.</param>
        [Dependency]
        public ElementSourceProvider(IPathResolver pathResolver, IFileSystemService fileSystemService, 
            IObjectPool objectPool)
        {
            _pathResolver = pathResolver;
            _fileSystemService = fileSystemService;
            _objectPool = objectPool;
        }

        #region IElementSourceProvider implementation

        /// <inheritdoc />
        public IObservable<IElementSource> Get(BoundingBox query)
        {
            // TODO ensure thread safety!
            return Observable.Create<IElementSource>(observer =>
            {
                Trace.Info(Category, "getting element sources for {0}", query.ToString());

                // Non read only element source should be first as it may override
                // some elements. Also search in tree should be fast
                var nodes = _tree.Search(query).ToList().Wait()
                    .OrderByDescending(e => e.ElementSource != null && !e.ElementSource.IsReadOnly);

                foreach (var node in nodes)
                {
                    EnsureElementSourceIntoNode(node);
                    observer.OnNext(node.ElementSource);
                }

                if (!nodes.Any())
                {
                    var cacheFileName = query.ToString().Replace(",", "_") + CacheFileNameExtension;
                    var cacheFullPath = Path.Combine(_cachePath, cacheFileName);
                    GetRemoteElementSource(cacheFullPath, query).Subscribe(e =>
                    {
                        observer.OnNext(e);
                        observer.OnCompleted();
                    });
                }
                else
                    observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        #endregion

        /// <inheritdoc />
        public void Add(IElementSource elementSource)
        {
            _tree.Insert(new TreeNode()
            {
                ElementSource = elementSource
            }, elementSource.BoundingBox);
        }

        #region Element source manipulation logic

        private void EnsureElementSourceIntoNode(TreeNode node)
        {
            if (node.ElementSource == null)
            {
                lock (node)
                {
                    if (node.ElementSource == null)
                    {
                        FreeUnusedElementSources();
                        Trace.Info(Category, "load index data from {0}", node.Path);
                        node.ElementSource = node.Path.EndsWith(CacheFileNameExtension)
                            ? BuildElementSourceInMemory(_fileSystemService.ReadBytes(node.Path))
                            : new ElementSource(node.Path, _fileSystemService, _objectPool);
                    }
                }
            }
        }

        /// <summary> Gets data from remote server. </summary>
        private IObservable<IElementSource> GetRemoteElementSource(string path, BoundingBox query)
        {
            // make online query
            var queryString = String.Format(_mapDataServerQuery,
                query.MinPoint.Latitude, query.MinPoint.Longitude,
                query.MaxPoint.Latitude, query.MaxPoint.Longitude);

            var uri = String.Format("{0}{1}", _mapDataServerUri, Uri.EscapeDataString(queryString));
            Trace.Warn(Category, Strings.NoPresistentElementSourceFound, query.ToString(), uri);
            return ObservableWWW.GetAndGetBytes(uri)
                .Take(1)
                .SelectMany(bytes =>
                {
                    Trace.Info(Category, "add to cache {0} and build index from {1} bytes received",
                        path, bytes.Length.ToString());
                    IElementSource elementSource = null;
                    lock (_tree)
                    {
                        // add to persistent cache
                        if (!_fileSystemService.Exists(path))
                        {
                            using (var stream = _fileSystemService.WriteStream(path))
                                stream.Write(bytes, 0, bytes.Length);
                        }
                        // build element source from bytes
                        elementSource = BuildElementSourceInMemory(bytes);
                        _tree.Insert(new TreeNode()
                        {
                            Path = path,
                            ElementSource = elementSource
                        }, query);
                    }
                    return Observable.Return(elementSource);
                });
        }

        /// <summary> Builds element source from raw data on fly. </summary>
        private IElementSource BuildElementSourceInMemory(byte[] bytes)
        {
            FreeUnusedElementSources();
            if (_settings == null) ReadIndexSettings();
            var indexBuilder = new InMemoryIndexBuilder(_mapDataFormat, new MemoryStream(bytes), 
                _settings, _objectPool, Trace);
            indexBuilder.Build();
            return new ElementSource(indexBuilder);
        }

        #endregion

        #region Persistent index processing

        private void SearchAndReadMapIndexHeaders(string folder)
        {
            _fileSystemService.GetFiles(folder, MapConsts.HeaderFileName).ToList()
                .ForEach(ReadMapIndexHeader);

            _fileSystemService.GetDirectories(folder, "*").ToList()
                .ForEach(SearchAndReadMapIndexHeaders);
        }

        private void ReadMapIndexHeader(string headerPath)
        {
            var boundingBox = PersistentIndexBuilder.ReadBoundingBox(_fileSystemService.ReadStream(headerPath));
            var node = new TreeNode()
            {
                Path = Path.GetDirectoryName(headerPath)
            };
            _tree.Insert(node, boundingBox);
        }

        #endregion

        private void FreeUnusedElementSources()
        {
            // TODO
        }

        private void ReadIndexSettings()
        {
            var jsonContent = _fileSystemService.ReadText(_indexSettingsPath);
            var node = JSON.Parse(jsonContent);
            _settings = new IndexSettings().ReadFromJson(node);
        }

        /// <inheritdoc />
        public void Configure(IConfigSection configSection)
        {
            _mapDataServerUri = configSection.GetString(@"remote.server", null);
            _mapDataServerQuery = configSection.GetString(@"remote.query", null);
            _mapDataFormat = configSection.GetString(@"remote.format", "xml");
            _indexSettingsPath = configSection.GetString(@"index.settings", null);

            _tree = new RTree<TreeNode>();
            var rootFolder = configSection.GetString("local", null);
            if (!String.IsNullOrEmpty(rootFolder))
            {
                SearchAndReadMapIndexHeaders(_pathResolver.Resolve(rootFolder));
                // create cache directory
                _cachePath = Path.Combine(rootFolder, ".cache");
                _fileSystemService.CreateDirectory(_cachePath);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tree.Traverse(node =>
            {
                if (node.Data != null && node.Data.ElementSource != null)
                    node.Data.ElementSource.Dispose();
            });
        }

        #region Nested classes

        private class TreeNode
        {
            public string Path;
            public IElementSource ElementSource;
        }

        #endregion
    }
}
