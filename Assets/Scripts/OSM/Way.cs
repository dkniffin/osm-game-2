using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OSM {
	public class Way : Element
	{
		private List<Node> nodes;

		private double boundingBoxN = -90.0;
		private double boundingBoxE = -180.0;
		private double boundingBoxS = 90.0;
		private double boundingBoxW = 180.0;

		private double centerLat = 1000.0;
		private double centerLon = 1000.0;

		public Way () {
			nodes = new List<Node> ();
		}

		public void AddNode(Node node) {
			nodes.Add (node);

			if (node.latitude > boundingBoxN) {
				boundingBoxN = node.latitude;
			}
			if (node.longitude > boundingBoxE) {
				boundingBoxE = node.longitude;
			}
			if (node.latitude < boundingBoxS) {
				boundingBoxS = node.latitude;
			}
			if (node.longitude < boundingBoxW) {
				boundingBoxW = node.longitude;
			}
		}

		public List<Node> getNodes() {
			return nodes;
		}

		// This method takes in the map bounds, and scales the current way so that origin is in the southwest corner of the bounds
		// 
		public List<Vector2> BuildVertices() {
			List<Vector2> vertices = new List<Vector2> ();

			foreach(Node node in nodes) {
				var x = (float)DistanceBetween(CenterLat(), CenterLon(), CenterLat(), node.longitude);
				if (CenterLon () > node.longitude) {
					x = 0 - x;
				}
					
				var y = (float)DistanceBetween(CenterLat(), CenterLon(), node.latitude, CenterLon());
				if (CenterLat () > node.latitude) {
					y = 0 - y;
				}

				vertices.Add(new Vector2(x, y));
			}
			// In OSM, the first node an the last node are the same. In Unity, we don't need that duplication
			vertices.RemoveAt (vertices.Count - 1);
			return vertices;
		}

		public Vector3 BuildPosition(LatLonBounds bounds) {
			var x = (float)DistanceBetween(bounds.s, bounds.w, bounds.s, CenterLon());
			var y = (float)DistanceBetween(bounds.s, bounds.w, CenterLat(), bounds.w);

			return new Vector3(x, y, 0);
		}

		public string BuildingType() {
			if (HasTag ("amenity", "police")) {
				return "police";
			} else if (HasTag ("amenity", "fire_station")) {
				return "fire_department";
			} else if (HasTag ("amenity", new[]{"hospital", "doctors", "dentist", "clinic", "pharmacy", "veterinary"})) {
				return "medical";
			} else {
				return "unknown";
			}
		}

		public Dictionary<string, double> GetBoundingBox() {
			return new Dictionary<string, double>{
				{ "n", boundingBoxN },
				{ "e", boundingBoxE },
				{ "s", boundingBoxS },
				{ "w", boundingBoxW }
			};
		}

		private double CenterLat() {
			if (centerLat == 1000.0) {
				centerLat = (boundingBoxN + boundingBoxS) / 2;
			}
			return centerLat;
		}

		private double CenterLon() {
			if (centerLon == 1000.0) {
				centerLon = (boundingBoxW + boundingBoxE) / 2;
			}
			return centerLon;
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