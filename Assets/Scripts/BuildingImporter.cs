using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BuildingImporter : MonoBehaviour {

	public GameObject prefab;
	public List<Building> buildings;
	public TextAsset osmFile;

	public float width = 100f;
	public float height = 50f;

	private Dictionary<int, OSMNode> nodes;
	private Dictionary<int, OSMWay> ways;

	// Use this for initialization
	void Start () {
		Vector2[] vertices = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, height),
			new Vector2(width, height),
			new Vector2(width, 0)
		};
		GameObject buildingObject = (GameObject)Instantiate (prefab, new Vector3(0, 0, 0), Quaternion.identity);
		Building b = buildingObject.GetComponent<Building> ();
		b.vertices = vertices;
		buildings.Add (b);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
