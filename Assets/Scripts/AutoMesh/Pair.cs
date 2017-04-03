using System;

public interface IPair<T> where T : IEquatable<T>, IComparable<T>
{
	T			p0			{ get; }
	T 			p1			{ get; }
	Pair<T> 	ordered		{ get; }
	Pair<T>		sorted		{ get; }
	Pair<T>		reversed	{ get; }

	bool 	contains(T p);
	bool 	isExact(IPair<T> b);
	bool 	isLike(IPair<T> b);
	T		next(T p);
}

public struct Pair<T> : IPair<T>, IEquatable<IPair<T>>, IComparable<IPair<T>> 
	where T : IComparable<T>, IEquatable<T>
{
	readonly T _p0, _p1;
	
	public Pair(T p0, T p1) { _p0 = p0; _p1 = p1; }
	
	public static Pair<T> newSorted(T p0, T p1) 
	{ 
		bool isSorted = p0.CompareTo (p1) <= 0;
		return new Pair<T> (isSorted ? p0 : p1, isSorted ? p1 : p0);
	}

	public T p0 { get { return _p0; } }
	public T p1 { get { return _p1; } }

	public Pair<T> ordered	{ get { return this; } }
	public Pair<T> sorted 	{ get { return newSorted (_p0, _p1); } }
	public Pair<T> reversed	{ get { return new Pair<T> (_p1, _p0); } }

	public bool contains(T p) 
	{ 
		return 	(_p0.NotNull () && _p0.Equals (p)) || 
				(_p1.NotNull () && _p1.Equals (p)) ||
				(p.IsNull () && (_p0.IsNull () || _p1.IsNull ()));
	}

	public bool isExact(IPair<T> b)	{ return b.NotNull () && _p0.Equals (b.p0) && _p1.Equals (b.p1); }

	public bool isLike(IPair<T> b)
	{ 
		return b.NotNull () && 
			(_p0.Equals (b.p0) && _p1.Equals (b.p1)) || (_p0.Equals (b.p1) && _p1.Equals (b.p0));
	}
	
	public T next(T p) { return _p0.Equals (p) ? _p1 : _p0; }

	// Object
	public override int GetHashCode() 		{ return Ext.hash (_p0.GetHashCode (), _p1.GetHashCode ()); }
	public override bool Equals(object b) 	{ return b.NotNull () && b is IPair<T> && Equals ((IPair<T>) b); }

	// IEquatable<IPair>
	public bool Equals(IPair<T> b) 			{ return isExact (b); }

	// IComparable<IPair>
	public int CompareTo(IPair<T> b)
	{ 
		return b.IsNull () ? 
			1 : this.Equals (b) ? 
				0 : _p0.CompareTo (b.p0); 
	}
	
	public static bool operator == (Pair<T> a, IPair<T> b) { return a.IsNull () ? b.IsNull () : a.Equals (b); } 
	public static bool operator != (Pair<T> a, IPair<T> b) { return a.IsNull () ? b.NotNull () : !a.Equals (b); } 

	public static bool operator < 	(Pair<T> a, IPair<T> b) { return a.IsNull () ? b.NotNull () : a.CompareTo (b) < 0; }
	public static bool operator > 	(Pair<T> a, IPair<T> b) { return a.IsNull () ? false : a.CompareTo (b) > 0; }
	public static bool operator <=	(Pair<T> a, IPair<T> b) { return a == b || a < b; }
	public static bool operator >=	(Pair<T> a, IPair<T> b) { return a == b || a > b; }
}
