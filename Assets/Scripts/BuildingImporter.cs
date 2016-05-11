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

public class BuildingImporter : MonoBehaviour {
	public GameObject prefab;

	public float width = 100f;
	public float height = 50f;

//	private InMemoryIndexBuilder osm;

	private string _xmlContent;

	private Dictionary<long, Node> nodes;
	private Dictionary<long, Way> ways;
	private List<Building> buildings;
	private LatLonBounds bounds;

	private XmlReader _reader;
	private Element _currentElement;


	// Use this for initialization
	void Start () {
		nodes = new Dictionary<long, Node> ();
		ways = new Dictionary<long, Way>();
		buildings = new List<Building>();
		bounds = new LatLonBounds();

		ImportOSMData ();
		foreach (Way way in ways.Values) {
			if (!way.HasTag("building")) { 
				continue;
			}
			GameObject buildingObject = (GameObject)Instantiate (prefab, new Vector3(0, 0, 0), Quaternion.identity);
			Building b = buildingObject.GetComponent<Building> ();
			b.vertices = way.BuildVertices(bounds).ToArray();
			buildings.Add (b);
		}

//		var vertices = ways[27473106].BuildVertices(bounds);
//		GameObject buildingObject = (GameObject)Instantiate (prefab, new Vector3(0, 0, 0), Quaternion.identity);
//		Building b = buildingObject.GetComponent<Building> ();
//		b.vertices = vertices.ToArray();
//		buildings.Add (b);

	}
	
	// Update is called once per frame
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
//				case "relation":
//					ParseRelation ();
//					break;
//				case "member":
//					ParseMember ();
//					break;
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

	//		var settings = new IndexSettings();
	//		JSONNode _jsonContent = JSON.Parse (File.ReadAllText (@"Assets/config.json"));
	//		settings.ReadFromJson (_jsonContent);
	//
	//		BoundingBox bbox = new BoundingBox(new GeoCoordinate(35.999148, -78.912091), new GeoCoordinate(35.988905, -78.897436));
	//		osm = new InMemoryIndexBuilder(bbox, settings, new ObjectPool(), new UnityConsoleTrace());
	//		_reader = new XmlApiReader ();
	//
	//		_context = new ReaderContext {
	//			SourceStream = new MemoryStream(Encoding.UTF8.GetBytes(_xmlContent)),
	//			Builder = osm,
	//			ReuseEntities = false,
	//			SkipTags = false
	//		};
	//
	//		_reader.Read(_context);
	//
	//		print (osm.Tree);

}
