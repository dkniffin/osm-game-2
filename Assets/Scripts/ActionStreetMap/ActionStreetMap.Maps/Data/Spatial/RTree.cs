﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ActionStreetMap.Core;
using ActionStreetMap.Infrastructure.Reactive;
using ActionStreetMap.Maps.Data.Helpers;
using ActionStreetMap.Maps.Helpers;

namespace ActionStreetMap.Maps.Data.Spatial
{
    /// <summary> Implements R-Tree data structure which is used to index spatial data. </summary>
    /// <typeparam name="T">Data type which is associated with envelop.</typeparam>
    internal class RTree<T> : ISpatialIndex<T>
	{
		// per-bucket
		private readonly int _maxEntries;
		private readonly int _minEntries;

        public RTreeNode Root { get; private set; }

        public RTree(int maxEntries = 9)
		{
			_maxEntries = Math.Max(4, maxEntries);
			_minEntries = (int) Math.Max(2, Math.Ceiling(_maxEntries * 0.4));

            Root = new RTreeNode { IsLeaf = true, Height = 1 };
		}

	    public RTree(RTreeNode root)
	    {
	        Root = root;
	    }

        #region Traverse

        /// <summary> Performs tree traversal using BFS. </summary>
        public void Traverse(Action<RTreeNode> action)
        {
            Traverse(Root, action);
        }

        /// <summary> Breadth first search. </summary>
        private static void Traverse(RTreeNode node, Action<RTreeNode> action)
        {
            action(node);
            foreach (var rTreeNode in node.Children)
                action(rTreeNode);
        }

        #endregion

        #region ISpatialIndex implementation. The same as optimized version.

        /// <inheritdoc />
        public IObservable<T> Search(BoundingBox query)
        {
            return Search(query, MapConsts.MaxZoomLevel);
        }

        /// <inheritdoc />
        public IObservable<T> Search(BoundingBox query, int zoomLevel)
        {
            return Search(new Envelop(query.MinPoint, query.MaxPoint), zoomLevel);
        }

        /// <inheritdoc />
        public void Insert(T data, BoundingBox boundingBox)
        {
            Insert(data, new Envelop(boundingBox));
        }

        /// <inheritdoc />
        public void Remove(T data, BoundingBox boundingBox)
        {
            Remove(data, new Envelop(boundingBox));
        }

        #endregion

        private IObservable<T> Search(IEnvelop envelope, int zoomLevel)
        {
            var minMargin = ZoomHelper.GetMinMargin(zoomLevel);
            return Observable.Create<T>(observer =>
            {
                var node = Root;
                if (!envelope.Intersects(node.Envelope))
                {
                    observer.OnCompleted();
                    return Disposable.Empty;
                }

                var nodesToSearch = new Stack<RTreeNode>();

                while (node != null && node.Envelope != null)
                {
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children)
                        {
                            var childEnvelope = child.Envelope;
                            if (envelope.Intersects(childEnvelope))
                            {
                                if (node.IsLeaf && childEnvelope.Margin >= minMargin)
                                    observer.OnNext(child.Data);
                                else if (envelope.Contains(childEnvelope))
                                    Collect(child, minMargin, observer);
                                else
                                    nodesToSearch.Push(child);
                            }
                        }
                    }
                    node = nodesToSearch.TryPop();
                }
                observer.OnCompleted();
                return Disposable.Empty;
            });
        }

        private void Remove(T item, Envelop envelope)
        {
            var node = Root;
            var itemEnvelope = envelope;

            var path = new Stack<RTreeNode>();
            var indexes = new Stack<int>();

            var i = 0;
            var goingUp = false;
            RTreeNode parent = null;

            // depth-first iterative tree traversal
            while (node != null || path.Count > 0)
            {
                if (node == null)
                {
                    // go up
                    node = path.TryPop();
                    parent = path.TryPeek();
                    i = indexes.TryPop();

                    goingUp = true;
                }

                if (node != null && node.IsLeaf)
                {
                    // check current node
                    var index = node.Children.FindIndex(n => Comparer.Equals(item, n.Data));

                    if (index != -1)
                    {
                        // item found, remove the item and condense tree upwards
                        node.Children.RemoveAt(index);
                        path.Push(node);
                        CondenseNodes(path.ToArray());

                        return;
                    }
                }

                if (!goingUp && !node.IsLeaf && node.Envelope.Contains(itemEnvelope))
                {
                    // go down
                    path.Push(node);
                    indexes.Push(i);
                    i = 0;
                    parent = node;
                    node = node.Children[0];

                }
                else if (parent != null)
                {
                    i++;
                    if (i == parent.Children.Count)
                    {
                        // end of list; will go up
                        node = null;
                    }
                    else
                    {
                        // go right
                        node = parent.Children[i];
                        goingUp = false;
                    }

                }
                else node = null; // nothing found
            }
        }

        private static void Collect(RTreeNode node, long minMargin, IObserver<T> observer)
        {
            var nodesToSearch = new Stack<RTreeNode>();
            while (node != null && node.Envelope != null)
            {
                if (node.Children != null)
                {
                    if (node.IsLeaf)
                        foreach (var child in node.Children)
                            if (child.Envelope.Margin >= minMargin)
                            observer.OnNext(child.Data);
                    else
                        foreach (var n in node.Children)
                            nodesToSearch.Push(n);
                }
                node = nodesToSearch.TryPop();
            }
        }

        private void Insert(RTreeNode item)
		{
			Insert(item, Root.Height - 1);
		}

		internal void Insert(T data, IEnvelop bounds)
		{
            Insert(new RTreeNode(data, bounds));
		}

        private void Insert(RTreeNode item, int level)
		{
			var envelope = item.Envelope;
            var insertPath = new List<RTreeNode>();

			// find the best node for accommodating the item, saving all nodes along the path too
			var node = ChooseSubtree(envelope, Root, level, insertPath);

			// put the item into the node
			node.Children.Add(item);
			node.Envelope.Extend(envelope);

			// split on node overflow; propagate upwards if necessary
			while (level >= 0)
			{
				if (!insertPath[level].HasChildren || insertPath[level].Children.Count <= _maxEntries) 
                    break;

				Split(insertPath, level);
				level--;
			}

			// adjust bboxes along the insertion path
			AdjutsParentBounds(envelope, insertPath, level);
		}

        private static double CombinedArea(IEnvelop what, IEnvelop with)
        {
            var minX1 = Math.Max(what.MinPointLongitude, with.MinPointLongitude);
            var minY1 = Math.Max(what.MinPointLatitude, with.MinPointLatitude);
            var maxX2 = Math.Min(what.MaxPointLongitude, with.MaxPointLongitude);
            var maxY2 = Math.Min(what.MaxPointLatitude, with.MaxPointLatitude);

			return (maxX2 - minX1) * (maxY2 - minY1);
		}

        private static double IntersectionArea(IEnvelop what, IEnvelop with)
		{
            var minX = Math.Max(what.MinPointLongitude, with.MinPointLongitude);
            var minY = Math.Max(what.MinPointLatitude, with.MinPointLatitude);
            var maxX = Math.Min(what.MaxPointLongitude, with.MaxPointLongitude);
            var maxY = Math.Min(what.MaxPointLatitude, with.MaxPointLatitude);

			return Math.Max(0, maxX - minX) * Math.Max(0, maxY - minY);
		}

        private RTreeNode ChooseSubtree(IEnvelop bbox, RTreeNode node, int level, List<RTreeNode> path)
		{
			while (true)
			{
				path.Add(node);

				if (node.IsLeaf || path.Count - 1 == level) 
                    break;

                var minArea = double.MaxValue;
                var minEnlargement = double.MaxValue;

                RTreeNode targetNode = null;

			    if (node.HasChildren)
			    {
			        for (var i = 0; i < node.Children.Count; i++)
			        {
			            var child = node.Children[i];
			            var area = child.Envelope.Area;
			            var enlargement = CombinedArea(bbox, child.Envelope) - area;

			            // choose entry with the least area enlargement
			            if (enlargement < minEnlargement)
			            {
			                minEnlargement = enlargement;
			                minArea = area < minArea ? area : minArea;
			                targetNode = child;

			            }
			            else if (Math.Abs(enlargement - minEnlargement) < double.Epsilon)
			            {
			                // otherwise choose one with the smallest area
			                if (area < minArea)
			                {
			                    minArea = area;
			                    targetNode = child;
			                }
			            }
			        }
			    }

			    Debug.Assert(targetNode != null);
				node = targetNode;
			}

			return node;
		}

		// split overflowed node into two
        private void Split(List<RTreeNode> insertPath, int level)
		{
			var node = insertPath[level];
			var totalCount = node.HasChildren ? node.Children.Count: 0;

			ChooseSplitAxis(node, _minEntries, totalCount);

			var newNode = new RTreeNode { Height = node.Height };
			var splitIndex = ChooseSplitIndex(node, _minEntries, totalCount);

			newNode.Children.AddRange(node.Children.GetRange(splitIndex, node.Children.Count - splitIndex));
			node.Children.RemoveRange(splitIndex, node.Children.Count - splitIndex);

			if (node.IsLeaf) 
                newNode.IsLeaf = true;

			RefreshEnvelope(node);
			RefreshEnvelope(newNode);

			if (level > 0) 
                insertPath[level - 1].Children.Add(newNode);
			else 
                SplitRoot(node, newNode);
		}

        private void SplitRoot(RTreeNode node, RTreeNode newNode)
		{
			// split root node
			Root = new RTreeNode
			{
				Children = { node, newNode },
				Height = (ushort) (node.Height + 1)
			};

			RefreshEnvelope(Root);
		}

        private int ChooseSplitIndex(RTreeNode node, int minEntries, int totalCount)
		{
            var minOverlap = double.MaxValue;
            var minArea = double.MaxValue;
			int index = 0;

			for (var i = minEntries; i <= totalCount - minEntries; i++)
			{
				var bbox1 = SumChildBounds(node, 0, i);
				var bbox2 = SumChildBounds(node, i, totalCount);

				var overlap = IntersectionArea(bbox1, bbox2);
				var area = bbox1.Area + bbox2.Area;

				// choose distribution with minimum overlap
				if (overlap < minOverlap)
				{
					minOverlap = overlap;
					index = i;

					minArea = area < minArea ? area : minArea;
				}
				else if (Math.Abs(overlap - minOverlap) < double.Epsilon)
				{
					// otherwise choose distribution with minimum area
					if (area < minArea)
					{
						minArea = area;
						index = i;
					}
				}
			}

			return index;
		}

        private void CondenseNodes(IList<RTreeNode> path)
        {
            // go through the path, removing empty nodes and updating bboxes
            for (var i = path.Count - 1; i >= 0; i--)
            {
                if (path[i].Children.Count == 0)
                {
                    if (i == 0)
                    {
                        Clear();
                    }
                    else
                    {
                        var siblings = path[i - 1].Children;
                        siblings.Remove(path[i]);
                    }
                }
                else
                {
                    RefreshEnvelope(path[i]);
                }
            }
        }

		// calculate node's bbox from bboxes of its children
        private static void RefreshEnvelope(RTreeNode node)
		{
		    var count = node.HasChildren ? node.Children.Count : 0;
            node.Envelope = SumChildBounds(node, 0, count);
		}

        private static IEnvelop SumChildBounds(RTreeNode node, int startIndex, int endIndex)
		{
            var retval = new Envelop();

			for (var i = startIndex; i < endIndex; i++)
				retval.Extend(node.Children[i].Envelope);

			return retval;
		}

        private static void AdjutsParentBounds(IEnvelop bbox, List<RTreeNode> path, int level)
		{
			// adjust bboxes along the given tree path
			for (var i = level; i >= 0; i--)
			{
				path[i].Envelope.Extend(bbox);
			}
		}

		// sorts node children by the best axis for split
        private static void ChooseSplitAxis(RTreeNode node, int m, int M)
		{
			var xMargin = AllDistMargin(node, m, M, CompareNodesByMinX);
			var yMargin = AllDistMargin(node, m, M, CompareNodesByMinY);

			// if total distributions margin value is minimal for x, sort by minX,
			// otherwise it's already sorted by minY
			if (node.HasChildren && xMargin < yMargin) 
                node.Children.Sort(CompareNodesByMinX);
		}

        private static int CompareNodesByMinX(RTreeNode a, RTreeNode b)
        {
            return a.Envelope.MinPointLongitude.CompareTo(b.Envelope.MinPointLongitude);
        }

        private static int CompareNodesByMinY(RTreeNode a, RTreeNode b)
        {
            return a.Envelope.MinPointLatitude.CompareTo(b.Envelope.MinPointLatitude);
        }

        private static double AllDistMargin(RTreeNode node, int m, int M, Comparison<RTreeNode> compare)
		{
            if (node.HasChildren)
			    node.Children.Sort(compare);

			var leftBBox = SumChildBounds(node, 0, m);
			var rightBBox = SumChildBounds(node, M - m, M);
			var margin = leftBBox.Margin + rightBBox.Margin;

			for (var i = m; i < M - m; i++)
			{
				var child = node.Children[i];
				leftBBox.Extend(child.Envelope);
				margin += leftBBox.Margin;
			}

			for (var i = M - m - 1; i >= m; i--)
			{
				var child = node.Children[i];
				rightBBox.Extend(child.Envelope);
				margin += rightBBox.Margin;
			}

			return margin;
		}

        public void Clear()
        {
            Root = new RTreeNode { IsLeaf = true, Height = 1 };
        }

        internal class RTreeNode
        {
            public T Data { get; private set; }
            public IEnvelop Envelope { get; set; }

            public bool IsLeaf { get; set; }
            public ushort Height { get; set; }

            private List<RTreeNode> _children;
            public List<RTreeNode> Children { get { return _children ?? (_children = new List<RTreeNode>()); } }

            public bool HasChildren { get { return _children == null || _children.Any(); } }

            public RTreeNode() : this(default(T), new Envelop()) { }

            public RTreeNode(T data, IEnvelop envelope)
            {
                Data = data;
                Envelope = envelope;
            }
        }
	}
}

