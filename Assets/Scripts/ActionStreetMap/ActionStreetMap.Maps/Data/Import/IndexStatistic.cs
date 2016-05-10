﻿using System;
using ActionStreetMap.Infrastructure.Diagnostic;
using ActionStreetMap.Maps.Data.Helpers;

namespace ActionStreetMap.Maps.Data.Import
{
    internal class IndexStatistic
    {
        private const string LogTag = "index.stat";

        private readonly ITrace _trace;
        private int _processedNodesCount;
        private int _processedWaysCount;
        private int _processedRelationsCount;

        private int _addedNodesCount;
        private int _addedWaysCount;
        private int _addedRelationsCount;

        private int _skippedNodesCount;
        private int _skippedWaysCount;
        private int _skippedRelationsCount;

        public IndexStatistic(ITrace trace)
        {
            _trace = trace;
        }

        public void Increment(ElementType type)
        {
            switch (type)
            {
                case ElementType.Node:
                    _addedNodesCount++;
                    break;
                case ElementType.Way:
                    _addedWaysCount++;
                    break;
                case ElementType.Relation:
                    _addedRelationsCount++;
                    break;
            }
        }

        public void IncrementTotal(ElementType type)
        {
            switch (type)
            {
                case ElementType.Node:
                    PrintProgress(++_processedNodesCount, "node");
                    break;
                case ElementType.Way:
                    PrintProgress(++_processedWaysCount, "way");
                    break;
                case ElementType.Relation:
                    PrintProgress(++_processedRelationsCount, "relation");
                    break;
            }
        }

        private void PrintProgress(int value, string typeName)
        {
            if (value % 10000 == 0)
                _trace.Debug(LogTag, "processed {0}: {1}", typeName, value.ToString());
        }

        public void Skip(long id, ElementType type)
        {
            switch (type)
            {
                case ElementType.Node:
                    _skippedNodesCount++;
                    break;
                case ElementType.Way:
                    _skippedWaysCount++;
                    break;
                case ElementType.Relation:
                    _skippedRelationsCount++;
                    break;
            }
        }

        public void Summary()
        {
            PrintSummary("PROCESSED", _processedNodesCount, _processedWaysCount, _processedRelationsCount);
            PrintSummary("ADDED", _addedNodesCount, _addedWaysCount, _addedRelationsCount);
            PrintSummary("SKIPPED", _skippedNodesCount, _skippedWaysCount, _skippedRelationsCount);
        }

        private void PrintSummary(string totalText, int nodes, int ways, int relations)
        {
            _trace.Debug(LogTag, "Total {0} elements: {1}", totalText, (nodes + ways + relations).ToString());
            _trace.Debug(LogTag, "\tnodes: {0}", nodes.ToString());
            _trace.Debug(LogTag, "\tways: {0}", ways.ToString());
            _trace.Debug(LogTag, "\trelations: {0}", relations.ToString());
            _trace.Debug(LogTag, "");
        }
    }
}
