using UnityEngine;

using System;
using System.Collections.Generic;

public static class Ext
{
	public static bool IsNull(this object o) 				{ return object.ReferenceEquals (o, null); }
	public static bool NotNull(this object o)				{ return !object.ReferenceEquals (o, null); }
	public static bool RefEquals(this object o, object o_) 	{ return object.ReferenceEquals (o, o_); }

	public static int Count<T>(this IEnumerable<T> o)
	{
		if (o is ICollection<T>)
			return (o as ICollection<T>).Count;
		
		int count = 0;

		foreach (var e in o)
			++count;
		
		return count;
	}

	public static bool Contains<T>(this IEnumerable<T> o, T element) where T : IEquatable<T>
	{
		if (o is ICollection<T>)
			return (o as ICollection<T>).Contains (element);
		
		foreach (var e in o)
			if (e.Equals (element))
				return true;

		return false;
	}

	public static T First<T>(this IEnumerable<T> o)
	{
		var e = o.GetEnumerator ();
		
		if (e.MoveNext ())
			return e.Current;
		else
			return default (T);
	}

	public static bool isCWOf(this Vector3 a, Vector3 b, Vector3 normal)
	{
		// this is the opposite of the above, because here the 'corner' is the 
		// center of the face, and not another vertex between a and b
		return normal.isSameSign (Ext.normal (a, Vector3.zero, b));
	}

	public static bool isSameSign(this Vector3 a, Vector3 b)
	{
		return 	Mathf.Sign (a.x) == Mathf.Sign (b.x) &&
				Mathf.Sign (a.y) == Mathf.Sign (b.y) &&
				Mathf.Sign (a.z) == Mathf.Sign (b.z);
	}

	// Left-handed
	// e.g. +X cross +Y gives +Z
	// e.g. specify in clockwise order, as viewed from outside
	public static Vector3 normal(Vector3 v0, Vector3 v1, Vector3 v2)
	{
		return Vector3.Cross (v2 - v1, v0 - v1).normalized;
	}

	const uint prime = 4294967291;

	public static int hash(int a, int b) { return (int)(a + ((uint)b + prime) * prime); }
}
