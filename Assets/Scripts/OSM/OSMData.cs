using System;
using System.Collections.Generic;

namespace OSM {
	public class OSMData : Singleton<OSMData> {
		private Dictionary<long, Node> nodes = new Dictionary<long, Node> ();
		private Dictionary<long, Way> ways = new Dictionary<long, Way>();
		private LatLonBounds bounds = new LatLonBounds ();

		public void AddNode(Node node) {
			nodes[node.id] = node;
		}

		public Dictionary<long, Node> GetNodes() {
			return nodes;
		}

		public Node GetNodeById(long id) {
			return nodes [id];
		}

		public void AddWay(Way way) {
			ways[way.id] = way;
		}

		public Dictionary<long, Way> GetWays() {
			return ways;
		}

		public Way GetWay(long id) {
			return ways [id];
		}

		public void SetBounds(double n, double e, double s, double w) {
			bounds.n = n;
			bounds.e = e;
			bounds.s = s;
			bounds.w = w;
		}

		public LatLonBounds GetBounds() {
			return bounds;
		}
	}
}
