using UnityEngine;
using UnityEngine.Assertions;

using System;
using System.Collections.Generic;

public class Face : IEquatable<Face>, IComparable<Face>
{
	public readonly int index;

	readonly List<FaceEdge> 					_faceEdges	= new List<FaceEdge> ();
	readonly IDictionary<Pair<Point>, MeshEdge> _meshEdges 	= new Dictionary<Pair<Point>, MeshEdge> ();

	public IEnumerable<FaceEdge> faceEdges { get { return _faceEdges; } }
	public IEnumerable<MeshEdge> meshEdges { get { return _meshEdges.Values; } }
	
	public bool isWindingSet { get; private set; }

	public Face(int index, IList<Pair<Point>> edges, IDictionary<Pair<Point>, MeshEdge> meshEdges)
	{ 
		this.index = index;

		foreach (var edge in edges)
		{
			edge.p0.addFace (this);

			var meshEdge = meshEdges [edge.sorted];
			meshEdge.addFace (this);

			_faceEdges.Add (new FaceEdge (edge.p0, edge.p1, meshEdge, this));
			_meshEdges [edge.sorted] = meshEdge;
		}
	}

	public void setWinding(bool isReverse)
	{
		if (this.isWindingSet)
			return;

		this.isWindingSet = true;

		if (!isReverse)
			return;

		foreach (var edge in _faceEdges)
			edge.reverse ();
		
		_faceEdges.Reverse ();
	}

	public FaceEdge getLikeEdge(IPair<Point> edge)
	{
		foreach (var faceEdge in _faceEdges)
			if (faceEdge.isLike (edge))
				return faceEdge;

		Assert.IsTrue(false, "expected face edge not found");
		return null;
	}

	public Vertex getVertex(int i) 
	{ 
		Assert.IsTrue (i >= 0 && i < _faceEdges.Count, "invalid face vertex index");
		return _faceEdges [i].p0.tryGetVertexFor (this); 
	}

	public Vector3 normalWith(IList<Vector3> vertices)
	{
		return Ext.normal (
			vertices [getVertex (0).index], 
			vertices [getVertex (1).index], 
			vertices [getVertex (2).index]);
	}

	// Object
	public override int 	GetHashCode() 		{ return base.GetHashCode (); }
	public override bool 	Equals(object b) 	{ return b.NotNull () && b is Face && Equals ((Face)b); }

	// IEquatable
	public bool Equals(Face b) { return b.NotNull () && b.index == index; }
	
	// IComparable
	public int CompareTo(Face b) 
	{ 
		return b.IsNull () ? 
			1 : this.Equals (b) ? 
				0 : _faceEdges [0].p0.CompareTo (b._faceEdges [0].p0); 
	}

	public static bool operator == (Face a, Face b) { return a.IsNull () ? b.IsNull () : a.Equals (b); }
	public static bool operator != (Face a, Face b) { return a.IsNull () ? b.NotNull () : !a.Equals (b); } 

	public static bool operator < 	(Face a, Face b) { return a.IsNull () ? b.NotNull () : a.CompareTo (b) < 0; }
	public static bool operator > 	(Face a, Face b) { return a.IsNull () ? false : a.CompareTo (b) > 0; }
	public static bool operator <=	(Face a, Face b) { return a == b || a < b; }
	public static bool operator >=	(Face a, Face b) { return a == b || a > b; }
}
