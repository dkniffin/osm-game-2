using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using ActionStreetMap.Maps.Data.Import;
using ActionStreetMap.Maps.Entities;
using ActionStreetMap.Maps.Formats;
using ActionStreetMap.Maps.Formats.Xml;
using OSMGame;
using SimpleJSON;
using ActionStreetMap.Explorer.Infrastructure;
using System.Diagnostics;
using ActionStreetMap.Explorer.Scene.Terrain;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;
using ActionStreetMap.Core.Geometry.Clipping;
using ActionStreetMap.Core;
using ActionStreetMap.Core.Geometry;

public class BuildingImporter : MonoBehaviour {
	public GameObject prefab;

	public float width = 100f;
	public float height = 50f;

	private InMemoryIndexBuilder osm;

	private string _xmlContent;
	private ReaderContext _context;
	private XmlApiReader _reader;


//	private XmlReader _reader;
//	private Element _currentElement;


	// Use this for initialization
	void Start () {
		ImportOSMData ();
//		Vector2[] vertices = new Vector2[] {
//			new Vector2(0, 0),
//			new Vector2(0, height),
//			new Vector2(width, height),
//			new Vector2(width, 0)
//		};
//		GameObject buildingObject = (GameObject)Instantiate (prefab, new Vector3(0, 0, 0), Quaternion.identity);
//		Building b = buildingObject.GetComponent<Building> ();
//		b.vertices = vertices;
//		buildings.Add (b);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void ImportOSMData() {
		_xmlContent = File.ReadAllText("Assets/atc.osm");
		

		var settings = new IndexSettings();
		string _jsonContent = JSON.Parse (File.ReadAllText (@"Assets/config.json"));
		settings.ReadFromJson (_jsonContent);

		BoundingBox bbox = new BoundingBox(new GeoCoordinate(35.999148, -78.912091), new GeoCoordinate(35.988905, -78.897436));
		osm = new InMemoryIndexBuilder(bbox, settings, new ObjectPool(), new UnityConsoleTrace());
		_reader = new XmlApiReader ();

		_context = new ReaderContext {
			SourceStream = new MemoryStream(Encoding.UTF8.GetBytes(_xmlContent)),
			Builder = osm,
			ReuseEntities = false,
			SkipTags = false
		};

		_reader.Read(_context);

		print (osm.Tree);


//		XmlReader reader = XmlReader.Create (new StringReader (osmData));
//
//		while (reader.Read ()) {
//			if (reader.NodeType == XmlNodeType.Element) {
//				switch (reader.Name) {
//				case "node":
//					parseNode ();
//					break;
//				case "tag":
//					ParseTag ();
//					break;
//				case "nd":
//					ParseNd ();
//					break;
//				case "way":
//					ParseWay ();
//					break;
//				case "relation":
//					ParseRelation ();
//					break;
//				case "member":
//					ParseMember ();
//					break;
//				case "bounds":
//					ParseBounds ();
//					break;
//				}
//			}
//		}


	}

}
