using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OSM {
	public class Way : Element
	{
		private List<Node> nodes;

		public Way () {
			nodes = new List<Node> ();
		}

		public void AddNode(Node node) {
			nodes.Add (node);
		}

		public List<Node> getNodes() {
			return nodes;
		}

		// This method takes in the map bounds, and scales the current way so that origin is in the southwest corner of the bounds
		// 
		public List<Vector2> BuildVertices(LatLonBounds bounds) {
			List<Vector2> vertices = new List<Vector2> ();
//			var earthRadius = 6371000;

			foreach(Node node in nodes) {
				var x = (float)DistanceBetween(bounds.minlat, bounds.minlon, bounds.minlat, node.longitude);
				var y = (float)DistanceBetween(bounds.minlat, bounds.minlon, node.latitude, bounds.minlon);

				vertices.Add(new Vector2(x, y));
			}
			// In OSM, the first node an the last node are the same. In Unity, we don't need that duplication
			vertices.RemoveAt (vertices.Count - 1);

			return vertices;
		}
			
		private double DistanceBetween(double lat1, double lon1, double lat2, double lon2) {
			var earthRadius = 6378137; // earth radius in meters
			var d2r = Math.PI / 180;
			var dLat = (lat2 - lat1) * d2r;
			var dLon = (lon2 - lon1) * d2r;
			var lat1rad = lat1 * d2r;
			var lat2rad = lat2 * d2r;
			var sin1 = Math.Sin (dLat / 2);
			var sin2 = Math.Sin (dLon / 2);

			var a = sin1 * sin1 + sin2 * sin2 * Math.Cos(lat1rad) * Math.Cos(lat2rad);

			return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
		}
	}
}