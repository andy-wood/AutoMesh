using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.Collections.Generic;

public class Point : IEquatable<Point>, IComparable<Point>
{
	public readonly int		index;
	public readonly Vector3 vector;

	readonly HashSet<MeshEdge>	_edges		= new HashSet<MeshEdge> ();
	readonly HashSet<Face>		_faces		= new HashSet<Face> ();
	readonly HashSet<Vertex> 	_vertices	= new HashSet<Vertex> ();

	public IEnumerable<MeshEdge> 	edges 		{ get { return _edges; } }
	public IEnumerable<Face> 		faces 		{ get { return _faces; } }
	public IEnumerable<Vertex>		vertices 	{ get { return _vertices; } }

	public Point(int index, Vector3 vertex)
	{
		this.index = index;
		this.vector = vertex;
	}

	public bool isIn(MeshEdge edge) { return _edges.Contains (edge); }

	public void addEdge(MeshEdge edge)
	{
		Assert.IsNotNull (edge);
		_edges.Add (edge);
	}

	public void addFace(Face face)
	{
		Assert.IsNotNull (face);
		_faces.Add (face);
	}

	public void addVertex(Vertex v)
	{
		Assert.IsNotNull (v);
		_vertices.Add (v);
	}

	public Vertex tryGetVertexFor(Face face)
	{
		// null face typically means 'no adjacent face'
		// i.e. mesh is not closed, which is ok
		if (face == null)
			return null;

		foreach (var vertex in _vertices)
			if (vertex.isSharedBy (face))
				return vertex;

		return null;
	}

	public Vector3 normalWith(IList<Vector3> vertices)
	{
		var sum = Vector3.zero;

		foreach (var face in _faces)
			sum += face.normalWith (vertices);

		return sum / _faces.Count ();
	}

	public static implicit operator Vector3(Point p) { return p.vector; }

	// Object
	public override int 	GetHashCode() 		{ return index.GetHashCode (); }
	public override bool 	Equals(object b) 	{ return b.NotNull () && b is Point && Equals ((Point) b); }

	// IEquatable
	public bool Equals(Point b) { return b.NotNull () && b.index == index; }
	
	// IComparable
	public int CompareTo(Point b) { return b.IsNull () ? 1 : this.index.CompareTo (b.index); }

	public static bool operator == (Point a, Point b) { return a.IsNull () ? b.IsNull () : a.Equals (b); } 
	public static bool operator != (Point a, Point b) { return a.IsNull () ? b.NotNull () : !a.Equals (b); } 

	public static bool operator < 	(Point a, Point b) { return a.IsNull () ? b.NotNull () : a.CompareTo (b) < 0; }
	public static bool operator > 	(Point a, Point b) { return a.IsNull () ? false : a.CompareTo (b) > 0; }
	public static bool operator <=	(Point a, Point b) { return a == b || a < b; }
	public static bool operator >=	(Point a, Point b) { return a == b || a > b; }
}
