using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unitilities.Tuples;

public class InputManager : MonoBehaviour{
	public SimulationManager sm;


	public void inputs() {
		movementKey ();
		reactionKey ();
	}


	/**
	* Look for input from the keys WASD or QE and move the currently selected
	* polygon accordingly.
	*/
	void movementKey() {
		bool didMove = false;
	
		//Movement keys
		if (Input.GetKey (KeyCode.A)) {
			tShape (new Vector3(-0.05f,0),ref didMove, false);
		}

		if (Input.GetKey (KeyCode.W)) {
			tShape (new Vector3(0,0.05f),ref didMove, false);
		}

		if (Input.GetKey (KeyCode.S)) {
			tShape (new Vector3(0,-0.05f),ref didMove,false);
		}

		if (Input.GetKey (KeyCode.D)) {
			tShape (new Vector3(0.05f,0),ref didMove,false);
		}

		//Rotation keys
		if (Input.GetKey (KeyCode.Q)) {
			tShape (new Vector3(0,0,0.5f),ref didMove,true);
		}

		if (Input.GetKey (KeyCode.E)) {
			tShape (new Vector3(0,0,-0.5f),ref didMove,true);
		}

		if (didMove && sm.gjk.tutorialMode) {
			sm.iterationSteps = 0;
			sm.stepLabel.text = "Step : 0";
			foreach (GameObject gameobj in GameObject.FindObjectsOfType<GameObject>()) {
				if (gameobj.name == "TutorialDot") {
					Destroy (gameobj);
				}
			}
		}
	}

	/**
	* Transforms the selected polygon by the input vector3 t.
	* If rot is set to true, the polygon is rotated.
	* If false, the polygon's position is altered.
	**/
	void tShape(Vector3 t,ref bool didMove, bool rot) {
		if (sm.selectedPoly == null)
			return;
		if (rot) { 
			sm.selectedPoly.gameObject.transform.Rotate(t);
		} else {
			sm.selectedPoly.gameObject.transform.position += t;
		}
		sm.moved.Enqueue (sm.selectedPoly);
		didMove = true;
	}



	/**
	* Look for input connected to creating polygons and tutorial mode and
	* perform connected actions.
	*/
	void reactionKey() {
		if (Input.GetMouseButtonDown(0)) {
			mouseDown ();
		}

		if (Input.GetKeyDown(KeyCode.X) && !sm.gjk.tutorialMode) {
			removeS ();
		}

		if (Input.GetKeyDown(KeyCode.Return)){
			newS ();
		}

		if (Input.GetKeyDown(KeyCode.N)) {
			if (sm.selectedPoly == null)
				return;
			sm.selectedPoly = sm.polys[(sm.polys.IndexOf(sm.selectedPoly) + 1) % sm.polys.Count];
			sm.moved.Enqueue (sm.selectedPoly);
		}

		if (Input.GetKey(KeyCode.R)) {
			sm.gjk.drawOuterMinkiDiffi(sm.polys[0],sm.polys[1]);
		}

		if (Input.GetKeyDown(KeyCode.U)) {
			invertTut ();
		}


		if (Input.GetKeyDown(KeyCode.K) && sm.gjk.tutorialMode) {
				if (!sm.gjk.done) {
					sm.iterationSteps++;
					sm.stepLabel.text = "Step : " + sm.iterationSteps;
				}
		}

		if (Input.GetKeyDown(KeyCode.J) && sm.gjk.tutorialMode) {
				sm.iterationSteps = Mathf.Max (sm.iterationSteps - 1, 0);
				sm.stepLabel.text = "Step : " + sm.iterationSteps;
		}



	}

	// If mousebutton 0 has been pressed
	void mouseDown() {
		sm.mousePoint = Input.mousePosition;
		sm.mousePoint.z = -1 * Camera.main.gameObject.transform.position.z;
		sm.mousePoint= Camera.main.ScreenToWorldPoint (sm.mousePoint);
		Vector2 point = new Vector2 (sm.mousePoint.x, sm.mousePoint.y); 
		if(!sm.newPolypoints.Contains(point) ){
			sm.newPolypoints.Add(point);
		}

		sm.newPolyLine.numPositions = sm.newPolypoints.Count;
		sm.newPolyLine.SetPosition (sm.newPolypoints.Count-1, point);

		if (sm.newPolypoints.Count == 1) {
			sm.newPolyDot = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			sm.newPolyDot.transform.localScale = new Vector3(0.05f,0.05f,0);
			sm.newPolyDot.transform.position = point;
		} else if (sm.newPolyDot != null) {
			Destroy(sm.newPolyDot);
		}
	}

	// Removes the current shape
	void removeS() {
		if (sm.selectedPoly == null)
			return;
		int i = sm.polys.IndexOf (sm.selectedPoly);


		foreach(Tuple<int,int> entry in new List<Tuple<int,int>>(sm.closestPts.Keys)) {
			if (entry.first == i || entry.second == i) {
				Destroy (sm.closestPts[entry]);
				sm.closestPts.Remove (entry);
			}
		}

		foreach (Polygon p in sm.polys) {
			p.touching.Remove (sm.selectedPoly);
		}

		sm.polys.Remove (sm.selectedPoly);
		foreach (GameObject g in GameObject.FindObjectsOfType<GameObject>()) {
			if (g.name == "Poly" && g.GetComponent (typeof(Polygon)) == sm.selectedPoly)
				Destroy (g);
		}
		Destroy (sm.selectedPoly);

		int pSize = sm.polys.Count;
		if (sm.polys.Count > 0) {
			i = ((i-1) % pSize + pSize) % pSize;
			sm.selectedPoly = sm.polys [i];
		} else {
			sm.selectedPoly = null;
		}

		foreach (Polygon p in sm.polys) {
			sm.moved.Enqueue (p);
		}
	}

	// Creates a new shape based on the input points
	void newS() {
		if (sm.newPolypoints.Count > 0) {
			sm.addPoly (sm.newPolypoints);
			sm.newPolypoints = new List<Vector2> ();
			sm.selectedPoly = sm.polys [0];
		}

		sm.newPolyLine.numPositions = 0;
	}

	// Turns on or turns off tutorial mode.
	void invertTut() {
		sm.gjk.tutorialMode = ! sm.gjk.tutorialMode;
		if (sm.gjk.tutorialMode) {
			sm.stepLabel.text = "Step : " + sm.iterationSteps;
		} else {
			sm.iterationSteps = 0;
			sm.stepLabel.text = "";

			foreach (GameObject g in GameObject.FindObjectsOfType<GameObject>()) {
				if (g.name == "TutorialDot" || g.name == "MinkDiff" || g.name == "LineBruv" || g.name == "MinkLine") {
					Destroy (g);
				}
			}
			if (sm.selectedPoly != null) {
				sm.moved.Enqueue (sm.selectedPoly);
			}
		}
	}




}