using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Diagnostics;
using OSM;

public class OSMImporter : MonoBehaviour {
	private GameObject buildingPrefab;

	private string _xmlContent;

	private Dictionary<long, Node> nodes = new Dictionary<long, Node> ();
	private Dictionary<long, Way> ways = new Dictionary<long, Way>();
	private List<Building> buildings = new List<Building>();
	private LatLonBounds bounds = new LatLonBounds();

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
	
	void Update () {
	
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
		nodes[node.id] = node;
	}

	private void ParseWay() {
		Way way = new Way ();
		way.id = long.Parse(_reader.GetAttribute("id"));
		_currentElement = way;
		ways[way.id] = way;
	}

	private void ParseNd() {
		long node_id = long.Parse(_reader.GetAttribute("ref"));
		Node node = nodes [node_id];
		(_currentElement as Way).AddNode(node);
	}

	private void ParseTag() {
		var key = _reader.GetAttribute("k");
		var value = _reader.GetAttribute("v");
		_currentElement.tags[key] = value;
	}

	private void ParseBounds() {
		bounds.minlat = double.Parse(_reader.GetAttribute("minlat"));
		bounds.maxlat = double.Parse(_reader.GetAttribute("maxlat"));
		bounds.minlon = double.Parse(_reader.GetAttribute("minlon"));
		bounds.maxlon = double.Parse(_reader.GetAttribute("maxlon"));
	}

	private void DrawBuildings() {
		foreach (Way way in ways.Values) {
			if (!way.HasTag("building")) { 
				continue;
			}
			GameObject buildingObject = (GameObject)Instantiate (buildingPrefab, new Vector3(0, 0, 0), Quaternion.identity);
			Building b = buildingObject.GetComponent<Building> ();
			b.way = way;
			b.vertices = way.BuildVertices(bounds).ToArray();
			buildings.Add (b);
		}
	}
}
