/*
	Copyright 2017 Andrew A. Wood
	
	This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.	
 */

using UnityEngine;
using UnityEngine.Assertions;

using System.Collections.Generic;

public class AutoMesh
{
	// connected points / thin Vector3 wrapper
	readonly List<Point> _points = new List<Point> ();

	// faces with directed edges and winding order
	readonly List<Face> _faces = new List<Face> ();

	// vertices split by shared faces
	readonly List<Vertex> _vertices = new List<Vertex> ();

	// canonical edge to mesh edge
	readonly Dictionary<Pair<Point>, MeshEdge> _edges = new Dictionary<Pair<Point>, MeshEdge> ();

	// unique edge loops - use the set of points in edges, for generality
	readonly List<HashSet<Point>> _loops = new List<HashSet<Point>> ();
	
	// final mesh-ready arrays
	readonly List<Vector3> 	_originalVertices 	= new List<Vector3> ();
	readonly List<Vector3> 	_originalNormals	= new List<Vector3> ();
	readonly List<int> 		_indices 			= new List<int> ();

	public Vector3[] 	getVertices ()	{ return _originalVertices.ToArray (); }
	public Vector3[] 	getNormals () 	{ return _originalNormals.ToArray (); }
	public int[]		getIndices () 	{ return _indices.ToArray (); }

	public AutoMesh(IEnumerable<Vector3> vertices, IEnumerable<Edge> edges)
		: this (vertices, edges, false) {}

	public AutoMesh(IEnumerable<Vector3> vertices, IEnumerable<Edge> edges, bool flipWinding)
	{
		create (vertices, edges, flipWinding);
	}

	public void getMorphed(Morph.morph morphFunc, out Vector3[] vertices, out Vector3[] normals)
	{
		vertices = new Vector3 [_vertices.Count];

		foreach (var point in _points)
		{
			var newVertex = morphFunc (point, point.index);
			
			foreach (var vertex in point.vertices)
				vertices [vertex.index] = newVertex;
		}
		
		var normalsList = new List<Vector3> (_vertices.Count);
		computeNormals (vertices, normalsList);
		normals = normalsList.ToArray ();
	}

	public void getMorphed(IMorph morph, out Vector3[] vertices, out Vector3[] normals)
	{
		getMorphed (morph.morph, out vertices, out normals);
	}

	public void getNormals(IList<Vector3> vertices, out Vector3[] normals)
	{
		var normalsList = new List<Vector3> (_vertices.Count);
		computeNormals (vertices, normalsList);
		normals = normalsList.ToArray ();
	}

	void create(IEnumerable<Vector3> vertices, IEnumerable<Edge> edges, bool flipWinding)
	{
		foreach (var vertex in vertices)
			_points.Add (new Point (_points.Count, vertex));

		// map normalized edge to mesh edge
		foreach (var edge in edges)
		{
			var meshEdge = new MeshEdge (_points [edge.p0], _points [edge.p1], edge.softness);
			_edges [meshEdge.sorted] = meshEdge;
		}

		findFaces ();
		fixWinding (flipWinding);
		triangulate ();

		foreach (var vertex in _vertices)
			_originalVertices.Add (vertex.point);

		// compute default normals. can get new normals later
		// for morphed vertices with same mesh topology
		computeNormals (_originalVertices, _originalNormals);
	}

	void findFaces()
	{
		var edges = new List<Pair<Point>> ();

		foreach (var p in _points)
		{
			edges.Clear ();

			// find all 3loops before 4loops, then reject 4loops that are supersets
			findFaces (p, p, 3, edges);
			findFaces (p, p, 4, edges);
		}
	}

	bool findFaces(Point start, Point p0, int max, List<Pair<Point>> edges)
	{
		foreach (var edge in p0.edges)
		{
			if (edge.isComplete || edges.Contains (edge.ordered) || edges.Contains (edge.reversed))
				continue;

			bool unwind = false;

			Point p1 = edge.next (p0);

			// use this like a stack
			edges.Add (new Pair<Point> (p0, p1));

			if (p1 == start)
			{
				if (edges.Count == max)
					tryAddFace (edges);
				
				unwind = true;
			}
			else if (edges.Count < max)
			{
				// graph-wise, when a loop is found, can unwind back to one edge
				unwind = (findFaces (start, p1, max, edges) && edges.Count > 1);
			}

			// pop
			edges.RemoveAt (edges.Count - 1);

			if (unwind)
				return true;
		}

		return false;
	}

	void tryAddFace(List<Pair<Point>> edges)
	{
		// check for redundant loop sets
		// use set of points in edges for generality, not edges themselves
		var newLoop = new HashSet<Point> ();

		foreach (var edge in edges)
			newLoop.Add (edge.p0);
		
		newLoop.Add (edges [edges.Count - 1].p1);

		foreach (var loop in _loops)
		{
			// non-proper set covers subsets and identities
			if (loop.IsSubsetOf (newLoop))
				return;

			Assert.IsFalse (loop.IsSupersetOf (newLoop), "4loop snuck in ahead of subset 3loop?");
		}
		
		_loops.Add (newLoop);
		_faces.Add (new Face (_faces.Count, edges, _edges));
	}

	void fixWinding(bool flip)
	{
		var  firstFace = chooseFirstFace ();
		bool isReverse = !hasExpectedWinding (firstFace);

		if (flip)
			isReverse = !isReverse;

		firstFace.setWinding (isReverse);
		fixAdjacent (firstFace);
	}

	Face chooseFirstFace()
	{
		// choose a face at an extremity, 
		// to improve the chance to best-guess its winding
		int p = 0;
		float max = _points [p].vector.sqrMagnitude;

		for (int i = 1; i < _points.Count; ++i)
		{
			float mag = _points [i].vector.sqrMagnitude;

			if (mag > max)
			{
				p = i;
				max = mag;
			}
		}

		return _points [p].faces.First ();
	}

	bool hasExpectedWinding(Face face)
	{
		var points = new List<Point> ();

		foreach (var edge in face.faceEdges)
			points.Add (edge.p0);

		var faceCenter = getCenter (points);
		var meshCenter = getCenter (_points);
		var pseudoNormal = (faceCenter - meshCenter).normalized;

		// assume pseudoNormal is roughly accurate
		// then winding is ok if v1 is clockwise of v0 for pseudoNormal
		// if this is a complicated concave mesh, then just manually flip the winding in constructor

		var p0 = points [0] - faceCenter;
		var p1 = points [1] - faceCenter;

		return p1.isCWOf (p0, pseudoNormal);
	}

	Vector3 getCenter(IEnumerable<Point> points)
	{
		var total = Vector3.zero;

		foreach (var p in points)
			total += p;
		
		return total / points.Count ();
	}

	void fixAdjacent(Face face)
	{
		foreach (var edge in face.faceEdges)
		{
			var adjFace = edge.nextFace;

			if (adjFace == null || adjFace.isWindingSet)
				continue;

			// corresponding edge of adjacent face should be opposite. if not, flip it
			bool isReverse = (adjFace.getLikeEdge (edge.ordered).isExact (edge));
			adjFace.setWinding (isReverse);

			// "floodfill" the winding
			fixAdjacent (adjFace);
		}
	}

	void triangulate()
	{
		splitVertices ();

		foreach (var face in _faces)
		{
			createTriangle(face, 0, 1, 2);

			if (face.faceEdges.Count () == 4)
				createTriangle(face, 2, 3, 0);
		}
	}

	void splitVertices()
	{
		var addedFaces = new HashSet<Face> ();

		foreach (var p in _points)
		{
			// search greedily, add faces conservatively, to maximize grouping
			// of faces with shared (completely soft) edges
			
			var pendingFaces = new HashSet<Face> (p.faces);

			while (pendingFaces.Count > 0)
			{
				addedFaces.Clear ();

				foreach (var face in pendingFaces)
					if (tryAddFaceToVertex (p, face))
						addedFaces.Add (face);

				if (addedFaces.Count > 0)
					pendingFaces.ExceptWith (addedFaces);
				else
					addOneFaceToNewVertex (p, pendingFaces);
			}
		}
	}

	bool tryAddFaceToVertex(Point point, Face face)
	{
		foreach (var edge in face.meshEdges)
		{
			// for each edge common to the point and the face,
			// add if it's a shared (completely soft) edge
			// and the adjacent face was already added
			// our service achieves 5 9's!

			if (edge.softness < 0.99999f || !point.isIn (edge))
				continue;
			
			var vertex = point.tryGetVertexFor (edge.nextFace (face));

			if (vertex != null)
			{
				vertex.addFace (face);
				return true;
			}
		}

		return false;
	}

	void addOneFaceToNewVertex(Point point, ICollection<Face> faces)
	{
		var firstFace = faces.First ();
		_vertices.Add(new Vertex (_vertices.Count, point, firstFace));
		faces.Remove (firstFace);
	}

	void createTriangle(Face face, int p0, int p1, int p2)
	{
		_indices.Add(face.getVertex (p0).index);
		_indices.Add(face.getVertex (p1).index);
		_indices.Add(face.getVertex (p2).index);
	}

	void computeNormals(IList<Vector3> vertices, List<Vector3> normals)
	{
		// compute all normals late, not coupled to the mesh structure
		// to enable morphing the vertices per instantiated final mesh

		normals.Clear ();

		foreach (var vertex in _vertices)
		{
			var vertexNormal = vertex.point.normalWith (vertices);

			var sum = Vector3.zero;
			int edgeCount = 0;

			foreach (var face in vertex.faces)
			{
				var faceNormal = face.normalWith (vertices);

				foreach (var edge in face.meshEdges)
				{
					if (!vertex.point.isIn (edge))
						continue;

					// for edges common to the vertex and face
					// edge is softened by blending face normal with vertex normal

					var edgeNormal = (faceNormal * (1.0f - edge.softness) + vertexNormal * edge.softness);

					sum += edgeNormal;
					++edgeCount;
				}
			}

			normals.Add (sum / edgeCount);
		}
	}
}
