using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GJK {
	public Vector2 dir;
	public double TOL = 1e-8;
	public int loopcounter = 0;

	public bool gjk(Polygon p1, Polygon p2){
		for (int i = 0; i < p1.parts.Count; i++) {
			for (int j = 0; j < p2.parts.Count; j++) {
				if (GARB (p1.parts [i], p2.parts [j])) {
					return true;
				}
			}
		}
		return false;
	}

	public Vector2[] closestPoints(Polygon p1, Polygon p2){
		Vector2[] closestPoints = new Vector2[]{Vector2.zero,Vector2.zero};
		float dist = Mathf.Infinity;

		for (int i = 0; i < p1.parts.Count; i++) {
			for (int j = 0; j < p2.parts.Count; j++) {
				Vector2[] newPoints = BARG(p1.parts[i], p2.parts[j]);
				float newDist = Vector2.Distance(newPoints [0], newPoints [1]);
				if (newDist < dist){
					closestPoints = newPoints;
					dist = newDist;
				}
			}
		}

		return closestPoints;
	}


	/** Are the GA intersecting the RB */
	public	bool GARB (Polygon p1, Polygon p2){
		//if (p1.transform.position == p2.transform.position)
		//	return true;

		dir = p1.vertices [0] - p2.vertices [0];

		List<MinkDiff> simplex = new List<MinkDiff> ();
		simplex.Add (support (p1, p2, dir));
		dir = -1 * dir;

		loopcounter = 0;
		while (true && loopcounter < 1000 && true && !false || false) {
			MinkDiff a = support (p1, p2, dir);
			simplex.Add (a);
			if (!canContainOrigin (a.diff, dir)) {
				return false;
			} else if (containsOrigin (simplex)) {
				return true;
			}
			loopcounter++;
		}
			
		p1.GetComponent<Renderer> ().material.color = Color.magenta;
		p2.GetComponent<Renderer> ().material.color = Color.magenta;
		return false;
	}
		
	/** How are BA antipenetrating RG deep */
	public Vector2[] BARG (Polygon p1, Polygon p2){
		//dir = p1.transform.position - p2.transform.position;
		dir = p1.vertices [0] - p2.vertices [0];
		List<MinkDiff> simplex = new List<MinkDiff> (2);
		simplex.Add (support (p1, p2, dir));
		simplex.Add (support (p1, p2, dir));
		dir = closestPointToOrigin (simplex [0].diff, simplex [1].diff);

		for (int i = 0; i < 30; i++) {
			dir = -1 * dir;
			if (dir.magnitude == 0) {
				return new Vector2[2]{ Vector2.zero, Vector2.zero };
			}
	
			MinkDiff c = support (p1, p2, dir);
			if (Vector2.Dot (c.diff, dir) - Vector2.Dot (simplex [0].diff, dir) < TOL) {
				Vector2[] closestPoints = findClosestPoints (simplex);
				return closestPoints;
			}

			Vector2 v1 = closestPointToOrigin (simplex [0].diff, c.diff);
			Vector2 v2 = closestPointToOrigin (simplex [1].diff, c.diff);

			if (v1.magnitude < v2.magnitude) {
				simplex [1] = c;
				dir = v1;
			} else {
				simplex [0] = c;
				dir = v2;
			}
		}

		Vector2[] closestPointss = findClosestPoints (simplex);
		return closestPointss;
	}


	Vector2 closestPointToOrigin (Vector2 a, Vector2 b){
		Vector2 ab = b - a;
		if (ab.magnitude <= TOL)
			return a;

		float proj = Vector2.Dot (-1 * a, ab) / Vector2.Dot (ab, ab);
		proj = Mathf.Min (1.0f, Mathf.Max (0, proj));
		return a + proj * ab;
	}

	Vector2[] findClosestPoints (List<MinkDiff> simplex){
		Vector2 l = simplex [1].diff - simplex [0].diff;
		Vector2 a = Vector2.zero;
		Vector2 b = Vector2.zero;

		if (l.magnitude == 0) {
			a = simplex [0].s1Point;
			b = simplex [0].s2Point;
		} else {
			float lambda2 = Vector2.Dot (-1 * l, simplex [0].diff) / Vector2.Dot (l, l);
			float lambda1 = 1 - lambda2;

			if (lambda1 < 0) {
				a = simplex [1].s1Point;
				b = simplex [1].s2Point;
			} else if (lambda2 < 0) {
				a = simplex [0].s1Point;
				b = simplex [0].s2Point;
			} else {
				a = lambda1 * simplex [0].s1Point + lambda2 * simplex [1].s1Point;
				b = lambda1 * simplex [0].s2Point + lambda2 * simplex [1].s2Point;
			}
		}

		return new Vector2[]{ a, b };
	}



	MinkDiff support (Polygon p1, Polygon p2, Vector2 dir){
		return new MinkDiff (p1.getFurthest (dir), p2.getFurthest (-1 * dir));
	}

	Vector2 tripleProd (Vector2 a, Vector2 b, Vector2 c){
		return b * (Vector2.Dot (c, a)) - a * (Vector2.Dot (c, b));
	}

	bool canContainOrigin (Vector2 point, Vector2 dir){
		// = 0 betyder nuddis
		return Vector2.Dot (point, dir) >= 0;
	}

	bool containsOrigin (List<MinkDiff> simplex){
		if (simplex.Count == 3) {
			return tripleSimplexHF (simplex);
		} else {	// simplex.Count == 2
			Vector2 a = simplex [1].diff;
			Vector2 b = simplex [0].diff;
			Vector2 newdir = tripleProd (b - a, -1 * a, b - a);

			if (newdir.magnitude != 0) {
				dir = newdir; 
			} else {
				dir = Vector2.left;
			}
			return false;
		}
	}

	bool tripleSimplexHF (List<MinkDiff> simplex){
		Vector2 a = simplex [2].diff;
		Vector2 ao = -1 * a;
		Vector2 ab = simplex [1].diff - a;
		Vector2 ac = simplex [0].diff - a;
		Vector2 abPerp = tripleProd (ac, ab, ab);
		Vector2 acPerp = tripleProd (ab, ac, ac);

		if (canContainOrigin (abPerp, ao)) {
			simplex.RemoveAt (0);
			dir = abPerp;
		} else if (canContainOrigin (acPerp, ao)) {
			simplex.RemoveAt (1);
			dir = acPerp;
		} else {
			return true;
		}

		return false;
	}	
}