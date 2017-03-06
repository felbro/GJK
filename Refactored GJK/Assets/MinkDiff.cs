using UnityEngine;
using Unitilities.Tuples;
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Class that holds a point of the Minkowski triangle and its ancestors  *
 * 																		 *
 * @author Rodrigo Retuerto, Natalie Axelsson, Felix Broberg			 *
 * @version 2017-03-05													 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
public class MinkDiff : MonoBehaviour {
	public Vector2 s1Point;
	public Vector2 s2Point;
	public Vector2 diff;
	private Color[] colors = new Color[]{Color.red, Color.blue, Color.green, Color.yellow, Color.magenta};

	/* Draws the minkowski dot and the original dots with the same color */
	public void draw(int color){
		drawDot (s1Point, color);
		drawDot (s2Point, color);
		drawDot (diff, color);
	}

	/* Draws the specified point with the specified color */
	private void drawDot(Vector2 point, int color){
		GameObject dot = GameObject.CreatePrimitive (PrimitiveType.Sphere);

		dot.transform.localScale = new Vector3 (0.2f, 0.2f, 0);
		dot.transform.position = new Vector3 (point.x, point.y, -0.0001f);
		dot.name = "TutorialDot";

		dot.GetComponent<Renderer> ().material.color = colors [color % 5];

	}
}