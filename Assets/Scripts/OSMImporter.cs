using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using OSM;

public class OSMImporter : MonoBehaviour {
	private GameObject buildingPrefab;

	private string _xmlContent;
	private XmlReader _reader;
	private Element _currentElement;

	void Start () {
		// Set up prefabs
		buildingPrefab = (GameObject)Resources.Load ("Prefabs/BuildingPrefab");

		// Import OSM Data
		ImportOSMData ();

		// Draw the scene
		DrawBuildings ();
	}

	private void ImportOSMData() {
		_xmlContent = File.ReadAllText ("Assets/atc.osm");
		_reader = XmlReader.Create (new StringReader (_xmlContent));

		while (_reader.Read ()) {
			if (_reader.NodeType == XmlNodeType.Element) {
				switch (_reader.Name) {
				case "node":
					parseNode ();
					break;
				case "tag":
					ParseTag ();
					break;
				case "nd":
					ParseNd ();
					break;
				case "way":
					ParseWay ();
					break;
			    /*
				case "relation":
					ParseRelation ();
					break;
				case "member":
					ParseMember ();
					break;
			    */
				case "bounds":
					ParseBounds ();
					break;
				}
			}
		}

	}

	private void parseNode() {
		Node node = new Node ();
		node.id = long.Parse(_reader.GetAttribute("id"));
		node.latitude = float.Parse(_reader.GetAttribute("lat"));
		node.longitude = float.Parse(_reader.GetAttribute("lon"));
		_currentElement = node;
		OSMData.Instance.AddNode (node);
	}

	private void ParseWay() {
		Way way = new Way ();
		way.id = long.Parse(_reader.GetAttribute("id"));
		_currentElement = way;
		OSMData.Instance.AddWay(way);
	}

	private void ParseNd() {
		long node_id = long.Parse(_reader.GetAttribute("ref"));
		Node node = OSMData.Instance.GetNodeById(node_id);
		(_currentElement as Way).AddNode(node);
	}

	private void ParseTag() {
		var key = _reader.GetAttribute("k");
		var value = _reader.GetAttribute("v");
		_currentElement.tags[key] = value;
	}

	private void ParseBounds() {
		var n = double.Parse(_reader.GetAttribute("maxlat"));
		var e = double.Parse(_reader.GetAttribute("maxlon"));
		var s = double.Parse(_reader.GetAttribute("minlat"));
		var w = double.Parse(_reader.GetAttribute("minlon"));

		OSMData.Instance.SetBounds (n, e, s, w);
	}

	private void DrawBuildings() {
		foreach (Way way in OSMData.Instance.GetWays().Values) {
			if (!way.HasTag("building")) {
				continue;
			}

			var position = way.BuildPosition(OSMData.Instance.GetBounds());
			GameObject buildingObject = (GameObject)Instantiate (buildingPrefab, position, Quaternion.identity);
			buildingObject.AddComponent<UIController> ();
			Building b = buildingObject.GetComponent<Building> ();
			b.way = way;
			b.vertices = way.BuildVertices().ToArray();
			GameObjectManager.Instance.AddBuilding(b);

		}
	}
}
