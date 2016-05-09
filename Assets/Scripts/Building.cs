using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour {

	public Vector2[] vertices;

	// Use this for initialization
	void Start () {
		UpdateMesh (vertices);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void UpdateMesh(Vector2[] vertices) {
		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = new Mesh ();
		mf.mesh = mesh;

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
}
