using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OSM;

public class Building : MonoBehaviour {
	public Vector2[] vertices;
	public Way way;
	private MeshFilter meshFilter;
	private PolygonCollider2D polyCollider;
	private MeshRenderer meshRenderer;
	public static readonly string[] TogglableTags = { "building_fire_department", "building_police" };
	private UIController ui;


	// Use this for initialization
	void Start () {
		ui = GetComponent<UIController> ();
		meshFilter = GetComponent<MeshFilter> ();
		polyCollider = GetComponent<PolygonCollider2D> ();
		meshRenderer = GetComponent<MeshRenderer> ();
		UpdateMesh ();
		UpdateMaterial ();
		UpdateTag ();
	}

	public static bool ContainsGUIElement (GameObject gameObject) {
		if (System.Array.IndexOf (TogglableTags, gameObject.tag) != -1) {
			return true;
		} else {
			return false;
		}
	}

	void OnGUI () {
		if(ContainsGUIElement(gameObject)) {
		 	ui.CreatePopUpMenu (gameObject);
		}
	}

	public void UpdateMesh() {
		Mesh mesh = new Mesh ();
		meshFilter.mesh = mesh;
		polyCollider.points = vertices;

		Vector3[] mesh_vertices = new Vector3[vertices.Length];
		Vector3[] normals = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) {
			Vector2 vertex = vertices [i];
			// Set up 3d vertex from 2d vertex
			mesh_vertices [i] = new Vector3 (vertex.x, vertex.y, 0);

			// The normal is always the same, since we're in 2d
			normals [i] = Vector3.back;
		}

		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices);
		int[] indices = tr.Triangulate();

		mesh.vertices = mesh_vertices;
		mesh.triangles = indices;
		mesh.normals = normals;
	}

	private void UpdateMaterial() {
		Dictionary<string, Material> materialMap = new Dictionary<string, Material>{
			{ "police", (Material)  Resources.Load ("Materials/Building/PoliceStation") },
			{ "fire_department", (Material) Resources.Load ("Materials/Building/FireStation") },
			{ "medical", (Material) Resources.Load ("Materials/Building/medical") },
			{ "unknown", (Material) Resources.Load ("Materials/Building/Default") }
		};
		meshRenderer.material = materialMap [way.BuildingType ()];
	}

	private void UpdateTag() {
		 var type = way.BuildingType ();
		 gameObject.tag = (type == "unknown") ? "building" :  "building_" + type;
	}
}
