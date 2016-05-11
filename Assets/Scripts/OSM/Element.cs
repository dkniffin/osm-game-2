﻿using System;
using System.Collections.Generic;

namespace OSM
{
	public class Element
	{
		public Int64 id;
		public Dictionary<string, string> tags;

		public Element ()
		{
			tags = new Dictionary<string, string> ();
		}

		public Boolean HasTag(string k) {
			return tags.ContainsKey (k) && tags [k] != "no";
		}

		public Boolean HasTag(string k, string v) {
			return HasTag (k) && tags [k] == v;
		}
	}
}

