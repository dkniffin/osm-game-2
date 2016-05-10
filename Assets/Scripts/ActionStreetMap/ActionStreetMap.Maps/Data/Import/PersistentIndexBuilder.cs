﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Infrastructure.IO;
using ActionStreetMap.Maps.Data.Spatial;
using ActionStreetMap.Maps.Data.Storage;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Infrastructure.Utilities;

namespace ActionStreetMap.Maps.Data.Import
{
    internal class PersistentIndexBuilder : IndexBuilder
    {
        private static readonly Regex GeoCoordinateRegex = new Regex(@"([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)");
        private static readonly string[] SplitString = { " " };

        private readonly string _filePath;
        private readonly string _outputDirectory;
        private readonly IFileSystemService _fileSystemService;

        public PersistentIndexBuilder(string filePath, string outputDirectory, IFileSystemService fileSystemService,
            IndexSettings settings, IObjectPool objectPool, ITrace trace)
            : base(settings, objectPool, trace)
        {
            _filePath = filePath;
            _outputDirectory = outputDirectory;
            _fileSystemService = fileSystemService;
        }

        public override void Build()
        {
            var sw = new Stopwatch();
            sw.Start();

            var sourceStream = _fileSystemService.ReadStream(_filePath);
            var format = _filePath.Split('.').Last();
            var reader = GetReader(format);

            var kvUsageMemoryStream = new MemoryStream();
            var kvUsage = new KeyValueUsage(kvUsageMemoryStream);

            var keyValueStoreFile = _fileSystemService.WriteStream(String.Format(MapConsts.KeyValueStorePathFormat, _outputDirectory));
            var index = new KeyValueIndex(Settings.Search.KvIndexCapacity, Settings.Search.PrefixLength);
            var keyValueStore = new KeyValueStore(index, kvUsage, keyValueStoreFile);

            var storeFile = _fileSystemService.WriteStream(String.Format(MapConsts.ElementStorePathFormat, _outputDirectory));
            Store = new ElementStore(keyValueStore, storeFile, ObjectPool);
            Tree = new RTree<uint>(65);

            reader.Read(new ReaderContext
            {
                SourceStream = sourceStream,
                Builder = this,
                ReuseEntities = false,
                SkipTags = false,
            });

            Clear();
            Complete();

            using (var kvFileStream = _fileSystemService.WriteStream(String.Format(MapConsts.KeyValueUsagePathFormat, _outputDirectory)))
            {
                var buffer = kvUsageMemoryStream.GetBuffer();
                kvFileStream.Write(buffer, 0, (int) kvUsageMemoryStream.Length);
            }

            KeyValueIndex.Save(index, _fileSystemService.WriteStream(String.Format(MapConsts.KeyValueIndexPathFormat, _outputDirectory)));
            SpatialIndex.Save(Tree, _fileSystemService.WriteStream(String.Format(MapConsts.SpatialIndexPathFormat, _outputDirectory)));
            Store.Dispose();
            
            sw.Stop();
            Trace.Debug(CategoryKey, Strings.IndexBuildInMs, sw.ElapsedMilliseconds.ToString());
        }

        public override void ProcessBoundingBox(BoundingBox bbox)
        {
            using (var writer = new StreamWriter(_fileSystemService.WriteStream(String.Format(MapConsts.HeaderPathFormat, _outputDirectory))))
            {
                writer.Write("{0} {1}", bbox.MinPoint, bbox.MaxPoint);
            }
        }


        #region Static functions

        /// <summary> Reads bounding box from header file stream. </summary>
        public static BoundingBox ReadBoundingBox(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var str = reader.ReadLine();
                var coordinateStrings = str.Split(SplitString, StringSplitOptions.None);
                var minPoint = GetCoordinateFromString(coordinateStrings[0]);
                var maxPoint = GetCoordinateFromString(coordinateStrings[1]);

                return new BoundingBox(minPoint, maxPoint);
            }
        }

        private static GeoCoordinate GetCoordinateFromString(string coordinateStr)
        {
            var coordinates = GeoCoordinateRegex.Match(coordinateStr).Value.Split(',');

            var latitude = double.Parse(coordinates[0]);
            var longitude = double.Parse(coordinates[1]);

            return new GeoCoordinate(latitude, longitude);
        }

        #endregion
    }
}