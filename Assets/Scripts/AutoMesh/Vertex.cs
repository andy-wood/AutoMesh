using UnityEngine.Assertions;

using System.Collections.Generic;

public class Vertex
{
	readonly int	_index;
	readonly Point 	_point;
	
	readonly HashSet<Face> _faces = new HashSet<Face> ();

	public int 					index { get { return _index; } }
	public Point 				point { get { return _point; } }
	public IEnumerable<Face> 	faces { get { return _faces; } }

	public Vertex(int index, Point point, Face face)
	{
		_index = index;
		_point = point;

		_faces.Add (face);
		point.addVertex (this);
	}

	public void addFace(Face face)
	{
		Assert.IsNotNull (face);
		_faces.Add (face);
	}

	public bool isSharedBy(Face face) { return _faces.Contains (face); }
}
