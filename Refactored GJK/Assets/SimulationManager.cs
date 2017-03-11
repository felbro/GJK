using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unitilities.Tuples;
using UnityEngine.UI;
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Class that handles key input and the simulation						 *
 * @author Rodrigo Retuerto, Natalie Axelsson, Felix Broberg			 *
 * @version 2017-03-05													 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
public class SimulationManager : MonoBehaviour {

	public List<Polygon> polys;
	public Polygon selectedPoly;
	public Vector3 mousePoint;
	public List<Vector2> newPolypoints;
	public LineRenderer newPolyLine;
	public GameObject newPolyDot;
	public GJK gjk;
	public InputManager im;
	public List<Vector2> stupidAssMinkiTriangle;
	public List<Vector3> outerPoints;
	public LineRenderer minkiDinkiLine;
	public Queue<Polygon> moved;
	private Polygon prevMoved;
	private HashSet<Polygon> done;
	public Dictionary<Tuple<int,int>,GameObject> closestPts;
	public int iterationSteps = 0;
	public Text stepLabel;


	/**
	* Initialize the graphics and objects needed to run the program
	*/
	void Start () {
		stepLabel = GameObject.Find ("StepLabel").GetComponent<Text> ();
		moved = new Queue<Polygon> ();
		done = new HashSet<Polygon> ();
		closestPts = new Dictionary<Tuple<int,int>,GameObject> ();

		polys = new List<Polygon> ();
		GameObject a = new GameObject("GJK");
		gjk = a.AddComponent<GJK> () as GJK;

		GameObject b = new GameObject ("Inputs");
		im = b.AddComponent<InputManager> () as InputManager;
		im.sm = this;

		newPolypoints = new List<Vector2>();
		GameObject newPoly = new GameObject ();
		newPolyLine = newPoly.AddComponent<LineRenderer> ();
		newPolyLine.material = new Material (Shader.Find ("Particles/Additive"));
		newPolyLine.widthMultiplier = 0.03f;
		newPolyLine.numPositions = 0;

		GameObject minkiDinki = new GameObject ("Minkow");
		minkiDinkiLine = minkiDinki.AddComponent<LineRenderer> ();
		minkiDinkiLine.material = new Material (Shader.Find ("Particles/Additive"));
		minkiDinkiLine.widthMultiplier = 0.03f;
		minkiDinkiLine.numPositions = 0;
	}

	/**
	* Look for updates in each frame
	*/
	void Update () {
		gjk.maxIterations = iterationSteps;
		// Check for input
		im.inputs();

		// Destory partial lines
		destroyLines ();

		// Create tutorial lines if tutorial mode on
		tutLines ();

		// Perform GJK calculations
		doGJK ();
	}

	void destroyLines() {
		foreach (GameObject gameobj in GameObject.FindObjectsOfType<GameObject>()) {
			if (gameobj.name == "PartialLine") {
				Destroy (gameobj);
			}
		}
	}

	void tutLines() {
		if (gjk.tutorialMode){
			moved.Enqueue (prevMoved);
			foreach (Polygon p in polys) {
				foreach (Polygon q in p.parts) {
					GameObject lineobject = new GameObject ("PartialLine");

					LineRenderer line = lineobject.AddComponent<LineRenderer> ();
					line.GetComponent<Renderer> ().material.color = Color.red;
					line.widthMultiplier = 0.02f;
					line.numPositions = q.vertices.Length + 1;
					for (int r = 0; r < q.vertices.Length; r++) {
						line.SetPosition (r, new Vector3 (q.vertices[r].x, q.vertices[r].y, -0.01f));
					}
					line.SetPosition (line.numPositions-1, new Vector3 (q.vertices [0].x, q.vertices [0].y, -0.01f));

				}
			}
		}
	}

	/**
	* Add a polygon to the scene
	*/
	public void addPoly(List<Vector2> vertices){
		GameObject a = new GameObject ("Poly");
		Polygon aa = a.AddComponent<Polygon>() as Polygon;
		aa.touching = new HashSet<Polygon> ();
		aa.addVertices (vertices);
		polys.Add (aa);
		moved.Enqueue (aa);
	}

	/**
	* Decide which polygons to perform GJK on and then do it
	*/
	void doGJK() {
		while (moved.Count > 0){
			Polygon fst = moved.Dequeue ();
			prevMoved = fst;
			done.Add (fst);
			foreach (Polygon p in polys) {
				if (done.Contains (p))
					continue;

				int a = polys.IndexOf (fst);
				int b = polys.IndexOf (p);
				if (b < a) {
					int tmp = a;
					a = b;
					b = tmp;
				}

				bool hit = gjk.gjk (polys[a], polys[b]);

				Tuple<int,int> t = new Tuple<int, int> (a, b);
				if (closestPts.ContainsKey(t))
					Destroy (closestPts [t]);
				if (hit) {
					fst.touching.Add(p);
					p.touching.Add (fst);
				} else {
					Vector2[] closestPoints = gjk.closestPoints (polys[a], polys[b]);

					fst.touching.Remove (p);
					p.touching.Remove (fst);

					closestPts [t] = new GameObject();
					drawLine(closestPoints [0], closestPoints [1],closestPts[t]);
				}
			}
		}

		if (gjk.tutorialMode && !gjk.done) {
			for (int i = 0; i < polys.Count; i++) {
				polys [i].GetComponent<Renderer> ().material.color = Color.yellow;
			}
		} else if (done.Count > 0) {
			for (int i = 0; i < polys.Count; i++) {
				if (polys [i].touching.Count > 0) {
					polys [i].GetComponent<Renderer> ().material.color = Color.red;
				} else {
					polys [i].GetComponent<Renderer> ().material.color = Color.green;
				}
			}
		}

		done.Clear ();
	}


	/**
	* Draw a line between points l1 and l2. The LineRenderer will be 
	* added as a component to the GameObject g.
	*/
	void drawLine(Vector3 l1, Vector3 l2,GameObject g ){
		LineRenderer lineRenderer = g.AddComponent<LineRenderer>();
		lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
		lineRenderer.widthMultiplier = 0.05f;
		lineRenderer.numPositions = 2;
		lineRenderer.SetPosition(0, l1);
		lineRenderer.SetPosition(1, l2);
	}
}