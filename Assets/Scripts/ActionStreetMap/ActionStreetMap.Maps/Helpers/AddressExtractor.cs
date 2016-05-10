﻿using System.Collections.Generic;
using ActionStreetMap.Core.Scene;
using ActionStreetMap.Core.Tiling.Models;
using ActionStreetMap.Core.Utils;

namespace ActionStreetMap.Maps.Helpers
{
    /// <summary> Extracts Address from OSM tag collection. </summary>
    public class AddressExtractor
    {
        #region Sorted lists of possible tags for Address's fields

        private static readonly List<string> NameKeyList = new List<string>
        {
            "addr:housenumber",
            "addr:housename",
            "name"
        };

        private static readonly List<string> StreetKeyList = new List<string>
        {
            "addr:street",
        };

        private static readonly List<string> CodeKeyList = new List<string>
        {
            "addr:postcode",
        };

        #endregion

        /// <summary> Extracts address from tags. </summary>
        /// <param name="tags">Tags collection.</param>
        /// <returns>Address.</returns>
        public static Address Extract(TagCollection tags)
        {
            return new Address
            {
                Name = GetValue(NameKeyList, tags),
                Street = GetValue(StreetKeyList, tags),
                Code = GetValue(CodeKeyList, tags)
            };
        }

        private static string GetValue(IEnumerable<string> keyList, TagCollection tags)
        {
            string value;
            foreach (var key in keyList)
            {
                if (tags.TryGetValue(key, out value))
                    return value;
            }
            return null;
        }
    }
}