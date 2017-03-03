using UnityEngine;

public class MinkDiff {
	public Vector2 s1Point;
	public Vector2 s2Point;
	public Vector2 diff;

	public MinkDiff(Vector2 s1Point, Vector2 s2Point){
		this.s1Point = s1Point;
		this.s2Point = s2Point;
		diff = s1Point - s2Point;
	}
}

