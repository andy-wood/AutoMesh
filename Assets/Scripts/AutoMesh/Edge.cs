public struct Edge
{
	readonly Pair<int> 	_points;
	readonly float 		_softness;

	public int 		p0 			{ get { return _points.p0; } }
	public int 		p1			{ get { return _points.p1; } }
	public float 	softness 	{ get { return _softness; } }

	public static Edge hard(int v0, int v1) 				{ return new Edge (v0, v1, 0.0f); }
	public static Edge soft(int v0, int v1) 				{ return new Edge (v0, v1, 1.0f); }
	public static Edge semi(int v0, int v1, float softness) { return new Edge (v0, v1, softness); }

	Edge(int p0, int p1, float softness)
	{ 
		_points = Pair<int>.newSorted (p0, p1); 
		_softness = softness;
	}
}

public class MeshEdge : IPair<Point>
{
	readonly Pair<Point> 	_points;
	readonly float 			_softness;

	Pair<Face> 	_faces;

	public float softness { get { return _softness; } }

	public bool 		isComplete 	{ get { return !_faces.contains (null); } }
	
	public Face			nextFace(Face face)	{ return _faces.next (face); }

	public Point		p0 			{ get { return _points.p0; } }
	public Point		p1 			{ get { return _points.p1; } }
	public Pair<Point> 	ordered 	{ get { return _points; } }
	public Pair<Point> 	sorted 		{ get { return _points.sorted; } }
	public Pair<Point> 	reversed	{ get { return _points.reversed; } }

	public bool 	contains(Point p)		{ return _points.contains (p); }
	public bool 	isExact(IPair<Point> b) { return _points.isExact (b); }
	public bool 	isLike(IPair<Point> b) 	{ return _points.isLike (b); }
	public Point 	next(Point p) 			{ return _points.next (p); }

	public MeshEdge(Point p0, Point p1, float softness)
	{
		_points = Pair<Point>.newSorted (p0, p1);
		_softness = softness;

		p0.addEdge (this);
		p1.addEdge (this);
	}

	public void addFace(Face face)
	{
		_faces = (_faces.p0 == null) ?
			new Pair<Face> (face, null) : new Pair<Face> (_faces.p0, face);
	}
}

public class FaceEdge : IPair<Point>
{
	readonly MeshEdge	_meshEdge;
	readonly Face 		_face;

	Pair<Point> 		_points;

	public Face 		nextFace	{ get { return _meshEdge.nextFace (_face); } }

	public Point		p0 			{ get { return _points.p0; } }
	public Point		p1 			{ get { return _points.p1; } }
	public Pair<Point> 	ordered 	{ get { return _points; } }
	public Pair<Point> 	sorted 		{ get { return _points.sorted; } }
	public Pair<Point> 	reversed	{ get { return _points.reversed; } }

	public bool 	contains(Point p)		{ return _points.contains (p); }
	public bool 	isExact(IPair<Point> b) { return _points.isExact (b); }
	public bool 	isLike(IPair<Point> b) 	{ return _points.isLike (b); }
	public Point	next(Point p) 			{ return _points.next (p); }

	public FaceEdge(Point p0, Point p1, MeshEdge meshEdge, Face face)
	{
		_points 	= new Pair<Point> (p0, p1);
		_meshEdge 	= meshEdge;
		_face 		= face;
	}

	public void reverse() { _points = _points.reversed; }
}
