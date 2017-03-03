using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour {

	public List<Polygon> polys;
	public Polygon selectedPoly;
	public double dist = 999;
	public Vector3 mousePoint;
	public List<Vector2> newPolypoints;
	public LineRenderer newPolyLine;
	public GameObject newPolyDot;
	public GJK gjk;
	public List<GameObject> lines;
	public List<Vector2> stupidAssMinkiTriangle;
	public List<Vector3> outerPoints;
	public LineRenderer minkiDinkiLine;



	// Use this for initialization
	void Start () {
		polys = new List<Polygon> ();
		gjk = new GJK ();

		List<Vector2> vertices2D = new List<Vector2>() {
			/*new Vector2( -1.39f, 2.28f),
			new Vector2( -2.91f, 2.16f),
			new Vector2( -1.68f, 2.63f),
			new Vector2( -1.32f , 4.82f),
			new Vector2( -1.29f , 2.94f),
			new Vector2( 0.17f , 2.93f),
			new Vector2( -0.62f , 2.85f),
			new Vector2( 0.29f , 1.53f),
			new Vector2( -0.62f , 1.54f),
			new Vector2( -0.94f , 0.07f),
			new Vector2( -1.29f , 1.00f),*/

			new Vector2( 0.54f, -0.39f),
			new Vector2( 0.62f, -1.9f),
			new Vector2( -0.21f , -0.63f),
			new Vector2( -1.62f , -1.18f),
			new Vector2( -0.67f , 0.0f),
			new Vector2( -1.62f, 1.18f),
			new Vector2( -0.21f, 0.63f),
			new Vector2( 0.62f , 1.9f),
			new Vector2( 0.54f, 0.39f),
			new Vector2( 2.0f , 0.0f),

			//new Vector2( -2f ,0f),
			//new Vector2( -2f, 4f),
			//new Vector2( 2, 4f),
			//new Vector2( 2f , 0f),

			//new Vector2( -4.08f, -0.27f),
			//new Vector2( -4.43f, 2.71f),
			//new Vector2( -4.10f, 2.22f),
			//new Vector2( -3.71f , 1.96f),
			//new Vector2( -3.02f , 1.99f),
			//new Vector2( -2.2f , 1.96f),
			//new Vector2( -1.78f , 2.13f),
			//new Vector2( -1.43f , 2.67f),
			//new Vector2( -1.13f , -0.75f),
			//new Vector2( -1.72f , -0.88f),
			//new Vector2( -1.86f , -0.37f),
			//new Vector2( -2.05f , 0.14f),
			//new Vector2( -3.01f , 0.567f),
			//new Vector2( -3.69f , 0.47f),
			//new Vector2( -3.86f , -0.76f),
		};

		addPoly (vertices2D);
		selectedPoly = polys [0];
		newPolypoints = new List<Vector2>();
		GameObject newPoly = new GameObject ();
		newPolyLine = newPoly.AddComponent<LineRenderer> ();
		newPolyLine.material = new Material (Shader.Find ("Particles/Additive"));
		newPolyLine.widthMultiplier = 0.03f;
		newPolyLine.numPositions = 0;

		GameObject minkiDinki = new GameObject ();
		minkiDinkiLine = minkiDinki.AddComponent<LineRenderer> ();
		minkiDinkiLine.material = new Material (Shader.Find ("Particles/Additive"));
		minkiDinkiLine.widthMultiplier = 0.03f;
		minkiDinkiLine.numPositions = 0;

	}

	void addPoly(List<Vector2> vertices){
		GameObject a = new GameObject ("Poly");
		Polygon aa = a.AddComponent<Polygon>() as Polygon;
		aa.addVertices (vertices);
		polys.Add (aa);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			mousePoint = Input.mousePosition;
			mousePoint.z = -1 * Camera.main.gameObject.transform.position.z;
			mousePoint= Camera.main.ScreenToWorldPoint (mousePoint);
			Vector2 point = new Vector2 (mousePoint.x, mousePoint.y); 
			if(!newPolypoints.Contains(point) ){
				newPolypoints.Add(point);
			}

			newPolyLine.numPositions = newPolypoints.Count;
			newPolyLine.SetPosition (newPolypoints.Count-1, point);

			if (newPolypoints.Count == 1) {
				newPolyDot = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				newPolyDot.transform.localScale = new Vector3(0.05f,0.05f,0);
				newPolyDot.transform.position = point;
			} else if (newPolyDot != null) {
				Destroy(newPolyDot);
			}

		}
		
		if (Input.GetKey(KeyCode.Return)){
			if (newPolypoints.Count > 0) {
				addPoly (newPolypoints);
				newPolypoints = new List<Vector2> ();
			}

			//Destroy (line);
			newPolyLine.numPositions = 0;
		}

		if (Input.GetMouseButtonDown(1)) {
			Vector3 mousePoint2 = Input.mousePosition;
			mousePoint2.z = -1 * Camera.main.gameObject.transform.position.z;
			mousePoint2= Camera.main.ScreenToWorldPoint (mousePoint2);
			Vector2 point = new Vector2 (mousePoint2.x, mousePoint2.y);

			List<Vector2> vertices = new List<Vector2> (3);
			vertices.Add (point);
			vertices.Add (point - new Vector2 (0, 0.05f));
			vertices.Add (point - new Vector2 (0.05f, 0));

			addPoly (vertices);
			Polygon temp = polys [polys.Count - 1];

			for (int i = 0; i < polys.Count - 1; i++) {
				if (gjk.gjk (temp, polys [i])) {
					selectedPoly = polys [i];
				}
			}
			polys.RemoveAt (polys.Count - 1);
			Destroy(temp.gameObject);

		}

		if (Input.GetKey (KeyCode.A)) {
			selectedPoly.gameObject.transform.position += new Vector3 (-0.05f, 0);
		}

		if (Input.GetKey (KeyCode.W)) {
			selectedPoly.gameObject.transform.position += new Vector3 (0, 0.05f);
		}

		if (Input.GetKey (KeyCode.S)) {
			selectedPoly.gameObject.transform.position += new Vector3 (0, -0.05f);
		}

		if (Input.GetKey (KeyCode.D)) {
			selectedPoly.gameObject.transform.position += new Vector3 (0.05f, 0);

		}

		if (Input.GetKey (KeyCode.Q)) {
			selectedPoly.gameObject.transform.Rotate(new Vector3(0, 0, 0.5f));

		}

		if (Input.GetKey (KeyCode.E)) {
			selectedPoly.gameObject.transform.Rotate(new Vector3(0, 0, -0.5f));
						
		}

		if (Input.GetKey (KeyCode.R)) {
			makeShitAssDiarrhea ();
		}
		if (polys.Count == 2) {
			//makeShitAssDiarrhea ();
		}

		for (int s = 0; s < polys.Count; s++) {
			polys [s].hit = false;
		}

		//Epic hack to not get endless lines
		for(int i = 0; i < lines.Count ; i++){
			Destroy (lines [i]);
		}

		lines = new List<GameObject>();

		if (Input.GetKeyDown(KeyCode.T)) {
			ConcaveSplit a = new ConcaveSplit ();
			Vector3[] v3 = selectedPoly .getVertices();
			Vector2[] v2 = new Vector2[v3.Length];
			for (int i = 0; i < v3.Length; i++) {
				v2 [i] = new Vector2(v3[i].x, v3[i].y);
			}
			List<List<int>> temp = a.divide(v2);
			//splitter = a.divide(v2);
			//Debug.Log (splitter[0]);


			foreach (List<int> singPoly in temp) {
				List<Vector2> tempList = new List<Vector2>();
				foreach(int verti in singPoly){
					tempList.Add(selectedPoly.getVertices()[verti]);
				}

				addPoly(tempList);
			}
		}




		for (int i = 0; i < polys.Count; i++) {
			for (int j = i+1; j < polys.Count; j++) {
				bool hit =  gjk.gjk(polys [i], polys [j]);

				if (hit){
					polys [i].hit = true;
					polys [j].hit = true;
				} else {
					Vector2[] closestPoints = gjk.closestPoints (polys [i], polys [j]);
					drawLine (closestPoints [0], closestPoints [1]);
					dist = Vector2.Distance (closestPoints [0], closestPoints [1]);
				}

			}

			if (polys[i].hit) {
				polys [i].GetComponent<Renderer> ().material.color = Color.red;
			} else {
				polys [i].GetComponent<Renderer> ().material.color = Color.green;
			}
		}

	}
		
	void drawLine(Vector3 l1, Vector3 l2 ){
		lines.Add (new GameObject ());
		LineRenderer lineRenderer = lines[lines.Count-1].AddComponent<LineRenderer>();
		lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
		lineRenderer.widthMultiplier = 0.05f;
		lineRenderer.numPositions = 2;
		lineRenderer.SetPosition(0, l1);
		lineRenderer.SetPosition(1, l2);
	}


	void makeShitAssDiarrhea(){
		foreach(GameObject sph in GameObject.FindGameObjectsWithTag("Player")){
			Destroy (sph);
		}

		stupidAssMinkiTriangle = new List<Vector2>();
		foreach (Vector2 v in polys[0].getVertices()) {
			foreach (Vector2 w in polys[1].getVertices()) {
				stupidAssMinkiTriangle.Add (v - w);
				GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
				sphere.transform.position = v - w;
				sphere.tag = "Player";
			}
		}

		outerPoints = jarvis (stupidAssMinkiTriangle);
		outerPoints.Add (outerPoints [0]);

		minkiDinkiLine.numPositions = outerPoints.Count;
		minkiDinkiLine.SetPositions (outerPoints.ToArray());
	}

	/*bool GJK(Polygon p1, Polygon p2){
		for (int i = 0; i < p1.parts.Count; i++) {
			for (int j = 0; j < p2.parts.Count; j++) {
				if (gjk.GARB (p1.parts [i], p2.parts [j])) {
					return true;
				}
			}
		}
		return false;
	}

	void drawShortestLine(Polygon p1, Polygon p2){
		Vector2[] closestPoints = new Vector2[]{Vector2.zero,Vector2.zero};
		float dist = Mathf.Infinity;

		for (int i = 0; i < p1.parts.Count; i++) {
			for (int j = 0; j < p2.parts.Count; j++) {
				Vector2[] newPoints = gjk.BARG(p1.parts[i], p2.parts[j]);
				float newDist = Vector2.Distance(newPoints [0], newPoints [1]);
				if (newDist < dist){
					closestPoints = newPoints;
					dist = newDist;
				}
			}
		}

		drawLine (closestPoints [0], closestPoints [1]);
		this.dist = dist;
	}*/

	List<Vector3> jarvis(List<Vector2> points){
		Vector3 currentPoint = points [0];
		foreach (Vector2 point in points){
			if (point.x < currentPoint.x) {
				currentPoint = point;
			}
		}

		List<Vector3> P = new List<Vector3> ();

		for(int i = 0; i < points.Count; i++){
			P.Add(currentPoint);
			Vector3 endpoint = points [0];
			foreach (Vector2 point in points){
				if (endpoint == currentPoint || toTheRight(P[i], point, endpoint)){
					endpoint = point;
				}
			}
			currentPoint = endpoint;
			if (endpoint == P[0]){
				return P;
			}
		}

		return new List<Vector3> (){Vector3.zero};
	}

	bool toTheRight(Vector3 prev, Vector3 curr, Vector3 next) {
		return (next.y - prev.y) * (curr.x-prev.x) < (next.x - prev.x) * (curr.y-prev.y);
	}

}