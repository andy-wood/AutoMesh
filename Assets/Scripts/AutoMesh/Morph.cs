using UnityEngine;

public interface IMorph
{
	Vector3 morph(Vector3 vertex, int index);
}

public class NullMorph : IMorph
{
	public Vector3 morph(Vector3 vertex, int index) { return vertex; }
}

public static class Morph
{
	public delegate Vector3 morph(Vector3 vertex, int index);
}
