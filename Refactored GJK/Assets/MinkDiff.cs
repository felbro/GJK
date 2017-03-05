using UnityEngine;
using Unitilities.Tuples;
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Class that holds a point of the Minkowski triangle and its ancestors  *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
public class MinkDiff : MonoBehaviour {
	public Vector2 s1Point;
	public Vector2 s2Point;
	public Vector2 diff;
	public SimulationManager sim;
	private Color[] colors = new Color[]{Color.red, Color.blue, Color.green, Color.yellow, Color.magenta};

	/* Draws the minkowski dot and the original dots with the same color */
	public void draw(int color,Tuple<Tuple<int,int>,Tuple<int,int>> t){
		drawDot (s1Point, color, t);
		drawDot (s2Point, color, t);
		drawDot (diff, color, t);
	}

	/* Draws the specified point with the specified color */
	private void drawDot(Vector2 point, int color, Tuple<Tuple<int,int>,Tuple<int,int>> t){
		foreach (GameObject g in sim.tutPts[t]) {
			if (g.transform.position.x == point.x && g.transform.position.y == point.y)
				return;
		}
		GameObject dot = GameObject.CreatePrimitive (PrimitiveType.Sphere);

		sim.tutPts [t].Add(dot);
		dot.transform.localScale = new Vector3 (0.2f, 0.2f, 0);
		dot.transform.position = new Vector3 (point.x, point.y, -0.0001f);
		dot.name = "TutorialDot";

		dot.GetComponent<Renderer> ().material.color = colors [color % 5];

	}
}