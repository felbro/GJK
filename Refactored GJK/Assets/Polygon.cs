using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Polygon : MonoBehaviour {

	public Vector3[] vertices;
	public bool hit;
	public List<List<int>> partVertexIndices;
	public List<Polygon> parts;


	public void addVertices(List<Vector2> verticesList) {
		Vector2[] vertices2D = verticesList.ToArray();

		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate();

		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		for (int i=0; i<vertices.Length; i++) {
			vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
		}

		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();

		// Set up game object with mesh;
		gameObject.AddComponent(typeof(MeshRenderer));
		MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
		filter.mesh = msh;

		//Concave Split TM
		ConcaveSplit a = new ConcaveSplit ();
		Vector2[] v2 = new Vector2[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) {
			v2 [i] = new Vector2(vertices[i].x, vertices[i].y);
		}
		partVertexIndices = a.divide(v2);

		Update ();
	
	}

	void Start () {
		
	}

	void Update () {
		vertices = GetComponent <MeshFilter> ().mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] = transform.TransformPoint (vertices [i]);
		}
		vertices = vertices.Distinct ().ToArray();

		parts = new List<Polygon> ();

		foreach (List<int> singPoly in partVertexIndices) {
			List<Vector3> partpart = new List<Vector3>();
			foreach(int verti in singPoly){
				partpart.Add(vertices[verti]);
			}

			Polygon part = new Polygon ();
			part.vertices = partpart.ToArray();
			parts.Add (part);
		}

	}

	public Vector3[] getVertices() {
		return vertices;
	}

	public Vector2 getFurthest(Vector2 dir){

		double maxDist = Vector3.Dot (dir, vertices[0]);
		int furthest = 0;

		for(int i = 1; i < vertices.Length; i++ ){
			double dottis = Vector3.Dot (dir, vertices [i]);
			if (dottis > maxDist) {
				furthest = i;
				maxDist = dottis;
			}

		}
		return vertices[furthest];

	}
}