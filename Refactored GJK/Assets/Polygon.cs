using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Class that holds and handles a polygon								 *
 * @author Rodrigo Retuerto, Natalie Axelsson, Felix Broberg			 *
 * @version 2017-03-05													 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
public class Polygon : MonoBehaviour {

	public Vector3[] vertices;
	//public bool hit;
	private List<List<int>> partVertexIndices;
	public List<Polygon> parts;
	public HashSet<Polygon> touching;


	/*
	 * Constructor-like method that creates the polygon with the vertices in verticesList and 
	 * divides the polygon into convex shapes if it is originally concave
	 */
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
		Vector2[] v2 = new Vector2[vertices.Length];
		for (int i = 0; i < vertices.Length; i++) {
			v2 [i] = new Vector2(vertices[i].x, vertices[i].y);
		}

		safeDivide (v2);
		Update ();
	}

	/* 
	 * Divides the polygon with vertices v into convex shapes 
	 * If the division cannot be made within 1 second it moves on instead of hogging the program forever.
	 */
	void safeDivide(Vector2[] v) {
		ConcaveSplit a = new ConcaveSplit ();
		int time = 1000;
		ManualResetEvent wait = new ManualResetEvent (false);
		Thread work = new Thread (new ThreadStart (() => {
			partVertexIndices = a.divide (v);
			wait.Set();
		}));
		work.Start ();

		bool correct = wait.WaitOne (time);
		if (!correct) {
			work.Abort ();
			partVertexIndices = null;
			return;
		}
	}

	/*
	 * Update gets called every frame.
	 * It updates the list of vertices of this polygon and each
	 * of its parts to account for any movement that might have occured.
	 */
	void Update () {
		vertices = GetComponent <MeshFilter> ().mesh.vertices;
		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] = transform.TransformPoint (vertices [i]);
		}
		vertices = vertices.Distinct ().ToArray();

		if (parts != null) {
			foreach (Polygon poly in parts) {
				Destroy (poly);
			}
		}

		parts = new List<Polygon> ();
		if (partVertexIndices != null) {
			foreach (List<int> singPoly in partVertexIndices) {
				List<Vector3> partpart = new List<Vector3> ();
				foreach (int verti in singPoly) {
					partpart.Add (vertices [verti]);
				}

				Polygon part = gameObject.AddComponent<Polygon>() as Polygon;
				part.vertices = partpart.ToArray ();
				parts.Add (part);
			}
		}
	}

	/* 
	 * Returns the positions of the polygon's vertices 
	 */
	public Vector3[] getVertices() {
		return vertices;
	}

	/*
	 * Returns the vertex of the polygon that is the furthest away from 
	 * its center in the specified direction
	 */
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