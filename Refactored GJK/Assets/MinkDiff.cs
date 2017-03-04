using UnityEngine;

public class MinkDiff : MonoBehaviour {
	public Vector2 s1Point;
	public Vector2 s2Point;
	public Vector2 diff;
	private Color[] colors = new Color[]{Color.red, Color.blue, Color.green, Color.yellow, Color.magenta};

	/*public MinkDiff(Vector2 s1Point, Vector2 s2Point){
		this.s1Point = s1Point;
		this.s2Point = s2Point;
		diff = s1Point - s2Point;
	}*/

	public void draw(int color){
		drawDot (s1Point, color);
		drawDot (s2Point, color);
		drawDot (diff, color);
	}

	private void drawDot(Vector2 point, int color){
		GameObject dot = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		dot.transform.localScale = new Vector3 (0.2f, 0.2f, 0);
		dot.transform.position = new Vector3(point.x, point.y, -0.0001f);
		dot.name = "TutorialDot";
		dot.GetComponent<Renderer> ().material.color = colors [color % 5];
	}
}

