using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;

/**
* Class for dividing concave objects into
* several convex.
*
* @author	Felix Broberg 2017-02-27
*/
public class ConcaveSplit{
	private Vector2[] points; // Points of the object
	private List< List<int> > convexShapes; // List of convex objects
	private HashSet<int> visited; // Points that have previously been traversed
	private Queue<int> notDone;	// Points left to do
	private Dictionary<int, List<int>> edges; //internal "edges"
	private HashSet<int> currVisited;
	private List<int> curr = new List<int>();


	/**
	* Divides the list of points into several convex objects
	*
	* @param pts	Vertices of object
	*/
	public List<List<int>> divide(Vector2[] pts) {

		convexShapes = new List<List<int>> ();
		visited = new HashSet<int>();
		notDone = new Queue<int> ();
		edges = new Dictionary<int,List<int>> ();


		points = pts;
		notDone.Enqueue(0); // Add first point to "divide"

		while (notDone.Count > 0) {
			divideit(notDone.Dequeue());
		}

		ctrlCheck ();
		return convexShapes;
	}

	void ctrlCheck() {
		foreach (KeyValuePair<int, List<int>> pair in edges) {
			foreach (int next in pair.Value) {
				ctrlHelp (pair.Key,next, pair.Key, new List<int> ());
			}
		}
	}

	void ctrlHelp(int prev, int current, int goal, List<int> path){
		if (current == goal) {
			path.Add (goal);
			addShape (path);
			return;
		}

		if (path.Contains (current)) {
			return;
		}

		foreach (int next in edges[current]) {
			if (next != prev) {
				List<int> pathCopy = new List<int> (path);
				pathCopy.Add (current);
				ctrlHelp (current,next, goal, pathCopy);
			}
		}
	}

	void addShape(List<int> toAdd){
		toAdd.Sort();
		bool add = true;
		foreach (List<int> v in convexShapes) {
			if (v.SequenceEqual (toAdd)) {
				add = false;
				break;
			}
		}
		if (add)
			convexShapes.Add (toAdd);
	}



	/**
	* Divides the object based on starting point
	*
	* @param start	Starting point
	*/
	void divideit(int start) {

		curr = new List<int>();
		currVisited = new HashSet<int>();
		addToCurrVisited(start);

		visited.Add(start);
		curr.Add(start);


		while (curr.Count > 0) {
			if (curr.Count < 2) {



				addToCurrVisited((curr[0]+1)% points.Length);

				visited.Add((curr[0]+1)% points.Length);
				curr.Add((curr[0]+1) % points.Length);

			}
			int prev = curr[curr.Count-2];
			int current = curr[curr.Count-1];
			int next = -1;

			if (curr.Count == 2) {

				if (visited.Contains(current) && edges.ContainsKey(current)) {
					next = rightVertex(prev,current);

				}
			}

			if (next == -1) {
				next = (current + 1) % points.Length;
			}

			threePoints(prev,current,next,false);

		}
	}



	void addToCurrVisited(int i) {
		if (!visited.Contains(i))
			currVisited.Add(i);
	}

	/**
	* Checks if three following points are convex or concave,
	* Continues if convex, backtracks if concave.
	*
	* @param prev	previous vertex
	* @param current current vertex
	* @curr	curr 	list of vertices so far that are convex
	*/
	void threePoints(int prev, int current, int next,bool last) {


		if (next == prev) {
			curr.Clear();
			currVisited.Clear();
			return;
		}

		if (toTheRight(prev,current,next)) {
			right(prev,current,next,last);
		}

		else {
			left(prev,current);
		}

	}

	/**
	* If next vertex forms a convex shapes together with previous two
	* vertices, this function is called. Checks if done splitting, or
	* Continues.
	*
	* @param current	the current vertex
	* @param next			the next vertex
	* @param curr			List of vertices currently forming a convex shape
	* @param last 		True if this could be the last vertex. False elsewise
	*/
	void right(int prev,int current, int next, bool last) {

		if (last) {
			//convexShapes.Add(new List<int>(curr));
			addToShapes(new List<int>(curr));
			curr.Clear();
			currVisited.Clear();
		}

		else if (visited.Contains(next) ) {
			itsvisited (prev, current, next);

		}
		else {
			curr.Add(next);
			addToCurrVisited(next);
			visited.Add(next);

		}
	}

	void itsvisited(int prev,int current,int next) {

		if (!curr.Contains (next)) {

			// must be edge in already done piece
			if (!edges.ContainsKey(next)) {
				if (edges.ContainsKey(current)) {
					int right = rightVertex(prev,current);
					if (toTheRight(prev,current,right)) {
						curr.Add(right);

						addToCurrVisited(right);
					}

					else {
						left(prev,current);
					}
				}
				else {
					curr.Add(next);
				}
			}

			// Must have another edge
			else {
				curr.Add (next);

				if (edges[next].Contains(curr[0])){
					//MAYBE TRUE
					threePoints(current,next,curr[0],false);
				}
				else {
					int right = rightVertex(curr[0],next);
					if (right >= 0) {

						threePoints (current, next, right, false);
					}
				}



			}
		} else {
			threePoints(current,next,curr[(curr.IndexOf(next)+1)%curr.Count],true);
		}
	}


	int rightVertex(int prev, int curr) {
		int temp = -1;
		int dist = 2140000000;
		foreach (int v in edges[curr]) {
			if (prev > curr) {
				if ((v > curr && v < prev) && ((prev - v)%points.Length < dist)) {
					temp = v;
					dist = ((prev - v)%points.Length);
				}
			}
			else {
				if ((v < prev || v > curr) && ((prev - v)%points.Length < dist)) {
					temp = v;
					dist = ((prev - v)%points.Length);
				}
			}

		}
		return temp;
	}
	/**
	* If next vertex forms a convex shapes together with previous two
	* vertices, this function is called. Checks if done splitting, or
	* Continues.
	*
	* @param prev			the previous vertex
	* @param current	the current vertex
	* @param curr			List of vertices currently forming a convex shape
	*/
	void left(int prev, int current) {
		int cont = findClosestRightVertex(prev,current);
		if (cont == -1) return;

		if (visited.Contains(cont)) {
			if (curr.Contains (cont)) {
				List <int> temp = new List<int>();

				int i = curr.Count-1;
				while (i >= 0 && curr[i] != cont) {
					temp.Add(curr[i]);
					curr.RemoveAt(i);

					i--;
				}
				currVisited.Clear ();
				temp.Add (cont);
				if (i > 0 && !notDone.Contains(curr[0])){
					notDone.Enqueue(curr[0]);
				}
				curr.Clear ();

				if (!temp.Contains (current)) {
					temp.Add (current);
				}

				temp.Reverse ();
				curr = new List<int> (temp);
				foreach(int j in curr) {
					//currVisited.Add(j);
					addToCurrVisited (j);
				}
				threePoints(current,cont,curr[1],true);


			}
			else {

				itsvisited(prev,current,cont);

			}
			if (!notDone.Contains(current)) {
				notDone.Enqueue(current);
			}


		}



		else {
			curr.Add(cont);
			addToCurrVisited(cont);
			visited.Add(cont);
			if (!notDone.Contains(current)) {
				notDone.Enqueue(current);
			}

		}

	}

	// Instead of % that is the remainder
	int mod(int x, int y) {
		return (x % y + y) % y;
	}


	void addToShapes(List<int> toAdd) {
		toAdd.Sort();
		for (int i = 0; i < toAdd.Count; i++) {
			// Previous vertex
			int negMod = toAdd [mod ((i - 1), toAdd.Count)];
			if (negMod != mod((toAdd[i] - 1),points.Length)) {
				if (!edges.ContainsKey(toAdd[i])) {
					edges[toAdd[i]] = new List<int>();
				}
				if (!edges[toAdd[i]].Contains(negMod)) {
					edges[toAdd[i]].Add(negMod);
				}
			}
			//next vertex
			int posMod = toAdd[mod((i + 1),toAdd.Count)];

			if (posMod != (toAdd [i] + 1) % points.Length) {
				if (!edges.ContainsKey (toAdd [i])) {
					edges [toAdd [i]] = new List<int> ();
				}
				if (!edges [toAdd [i]].Contains (posMod)) {
					edges [toAdd [i]].Add (posMod);
				}
			} 

		}
		addShape(toAdd);


	}

	/**
  * Returns true if the next vertex is to the right or straight ahead.
	*
	* @param prev	the previous vertex
	*	@param curr	the current vertex
	* @param next the next vertex
	* @return true if the next vertex is straight ahead or to the right
  */
	bool toTheRight(int prev, int curr, int next) {
		if (prev == -1 || curr == -1 || next == -1)
			Thread.Sleep (2000);
		return (points[next].y - points[prev].y) * (points[curr].x-points[prev].x)
			<= (points[next].x - points[prev].x) * (points[curr].y-points[prev].y);
	}



	/**
	* Finds the closest vertex ahead to the right from a line AB
	*
	* @param a first vertex
	* @param b second vertex
	* @return the closest vertex. -1 if no such vertex
	*/
	int findClosestRightVertex(int a, int b) {
		float distance = 2141000000;
		int vertex = -1;

		//Iterate over outer edges
		for (int i = 0; i < points.Length; i++) {
			float retval = intersecting(points[a],points[b],points[i],points[(i+1)%points.Length],true);
			if (retval >= 0 && retval < distance) {
				assignVertexDist(a,b,i,(i+1)%points.Length,ref vertex,ref distance,retval);
			}
		}


		foreach (KeyValuePair<int,List<int>> i in edges) {
			for (int j = 0; j < i.Value.Count; j++) {
				float retval = intersecting(points[a],points[b],points[i.Key],points[i.Value[j]],true);
				if (retval >= 0 && retval < distance) {
					assignVertexDist(a,b,i.Key,i.Value[j],ref vertex, ref distance,retval);

				}
			}
		}

		//CUTS ITSELF
		for (int i = 0; i < curr.Count; i++) {

			if (intersecting(points[b],points[vertex],points[curr[i]],points[curr[(i+1)%curr.Count]],false) == 1) {

				backitup(vertex,a,b);
				return -1;
			}
		}
		return vertex;
	}

	void assignVertexDist(int a, int b, int i, int j,ref int vertex, ref float distance,float retval) {
		int tempvertex;

		if (toTheRight(a,b,i)) {
			tempvertex = i;
		}
		else {
			//tempvertex = (i+1)%points.Length;
			tempvertex = j;
		}
		if(!cutsother(b,tempvertex)) {
			distance = retval;
			vertex = tempvertex;
		}
		else {

			int k = (tempvertex+1)%points.Length;
			while (cutsother(b,k) ) {
				k = (k+1)%points.Length;
			}

			float newDist;
			if ((newDist = (points[b]-points[k]).magnitude) < distance) {
				distance = newDist;
				vertex = k;
			}
		}
	}



	// True if a,b cuts other edges
	bool cutsother(int a, int b) {
		for (int i = 0; i < points.Length; i++) {
			if (a != i && a != (i + 1) % points.Length && b != i && b != (i + 1) % points.Length) {
				if (intersecting (points [a], points [b], points [i], points [(i + 1) % points.Length], false) == 1)
					return true;
			}
		}
		return false;
	}


	void backitup(int next,int prev,int current) {
		if (!notDone.Contains(curr[0]))
			notDone.Enqueue(curr[0]);

		for (int i = curr.Count - 1; i >= 0; i--) {
			if(currVisited.Contains(curr[i])){
				visited.Remove(curr[i]);
			}
			//curr.RemoveAt(i);
		}

		currVisited.Clear();
		curr.Clear ();
		curr.Add(prev);
		curr.Add(current);
		curr.Add(next);

		addToCurrVisited(prev);
		addToCurrVisited(current);
		addToCurrVisited(next);

		visited.Add (prev);
		visited.Add (current);
		visited.Add (next);


	}

	/**
	* Finds the distance from point base2 to intersection between line base1-base2
	* to line ctrl1-ctrl2. Requires intersection point to be in front of base1-base2
	* line and on the ctrl1-ctrl2 line.
	*/
	float intersecting(Vector2 base1, Vector2 base2, Vector2 ctrl1, Vector2 ctrl2, bool getDist) {
		if (base1 == ctrl1 || base1 == ctrl2 || base2 == ctrl1 || base2 == ctrl2)
			return -1;

		float denom = (base1.x-base2.x)*(ctrl1.y-ctrl2.y) - (base1.y-base2.y)*(ctrl1.x-ctrl2.x);
		if (Mathf.Abs(denom) <= 1e-8)
			return -1;

		float x = ((base1.x * base2.y - base1.y * base2.x) * (ctrl1.x - ctrl2.x) - (base1.x - base2.x) * (ctrl1.x * ctrl2.y - ctrl1.y * ctrl2.x)) /
			denom;
		float y = ((base1.x * base2.y - base1.y * base2.x) * (ctrl1.y - ctrl2.y) - (base1.y - base2.y) * (ctrl1.x * ctrl2.y - ctrl1.y * ctrl2.x)) /
			denom;

		Vector2 resVec = new Vector2 (x, y);
		float scalar1 = Vector2.Dot((ctrl2-ctrl1),(resVec-ctrl1))/Vector2.Dot((ctrl2-ctrl1),(ctrl2-ctrl1));
		float scalar2 = Vector2.Dot((base2-base1),(resVec-base1))/Vector2.Dot((base2-base1),(base2-base1));
		if (getDist && scalar1 >= 0 && scalar1 <= 1 && scalar2 > 1)
			return (resVec - base2).magnitude;
		else if(scalar1 >= 0 && scalar1 <= 1 && scalar2 > 1e-7 && scalar2 < (1-1e-7))
			return 1;

		return -1;
	}

}
