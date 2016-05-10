﻿using System.Collections.Generic;
using System.Linq;
using ActionStreetMap.Core.Tiling.Models;

namespace ActionStreetMap.Core.MapCss.Domain
{
    /// <summary> Contains some performance optimizations for rule processing. </summary>
    internal class StyleCollection
    {
        private readonly List<Style> _canvasStyles = new List<Style>(1);
        private readonly List<Style> _areaStyles = new List<Style>(16);
        private readonly List<Style> _wayStyles = new List<Style>(16);
        private readonly List<Style> _nodeStyles = new List<Style>(64);

        private readonly List<Style> _combinedStyles = new List<Style>(16);

        private int _count;
        public int Count { get { return _count; } }

        public void Add(Style style)
        {
            // NOTE store different styles in different collections to increase
            // lookup performance. However, there are two limitations:
            // 1. style order is resorted by type
            // 2. Combined styles (logical AND variations) in new collection now and will be processed last
            if (IsZoomTypeStyle<NodeSelector>(style))
                _nodeStyles.Add(style);
            else if (IsZoomTypeStyle<AreaSelector>(style))
                _areaStyles.Add(style);
            else if (IsZoomTypeStyle<WaySelector>(style))
                _wayStyles.Add(style);
            else if (style.Selectors.All(s => s is CanvasSelector))
                _canvasStyles.Add(style);
            else
                _combinedStyles.Add(style);

            _count++;
        }

        private bool IsZoomTypeStyle<T>(Style style)
        {
            var andSelector = style.Selectors[0] as AndSelector;
            if (andSelector == null)
                return style.Selectors.All(s => s is T);

            if (andSelector.Selectors[0].Operation != MapCssStrings.OperationZoom)
                return false;

            return andSelector.Selectors.Skip(1).All(s => s is T);
        }

        public Rule GetMergedRule(Model model, int zoomLevel)
        {
            var styles = GetModelStyles(model);
            var rule = new Rule(model);
            for (int i = 0; i < styles.Count; i++)
                MergeDeclarations(styles[i], rule, model, zoomLevel);

            for (int i = 0; i < _combinedStyles.Count; i++)
                MergeDeclarations(_combinedStyles[i], rule, model, zoomLevel);

            return rule;
        }

        public Rule GetCollectedRule(Model model, int zoomLevel)
        {
            var styles = GetModelStyles(model);
            var rule = new Rule(model);
            for (int i = 0; i < styles.Count; i++)
                CollectDeclarations(styles[i], rule, model, zoomLevel);

            for (int i = 0; i < _combinedStyles.Count; i++)
                CollectDeclarations(_combinedStyles[i], rule, model, zoomLevel);

            return rule;
        }

        private List<Style> GetModelStyles(Model model)
        {
            if (model is Node)
                return _nodeStyles;

            if (model is Area)
                return _areaStyles;

            if (model is Way)
                return _wayStyles;

            return _canvasStyles;
        }

        #region Declaration processing

        private void MergeDeclarations(Style style, Rule rule, Model model, int zoomLevel)
        {
            if (!style.IsApplicable(model, zoomLevel))
                return;

            // NOTE This can be nicely done by LINQ intesection extension method
            // but this peace of code is performance critical
            foreach (var key in style.Declarations.Keys)
            {
                var styleDeclaration = style.Declarations[key];
                if (rule.Declarations.ContainsKey(key))
                    rule.Declarations[key] = styleDeclaration;
                else
                    rule.Declarations.Add(key, styleDeclaration);
            }
        }

        private void CollectDeclarations(Style style, Rule rule, Model model, int zoomLevel)
        {
            if (!style.IsApplicable(model, zoomLevel))
                return;

            foreach (var keyValue in style.Declarations)
                rule.Declarations.Add(keyValue.Key, keyValue.Value);
        }

        #endregion
    }
}
