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

using System.Collections.Generic;

public class Rock : MonoBehaviour 
{
	[SerializeField] bool  _rotate;
	[SerializeField] float _jitterAmount;

	static AutoMesh _mesh;

	void Start ()
	{
		if (_mesh == null)
			_mesh = getAutoMesh ();
		
		_jitterAmount = Mathf.Clamp01 (_jitterAmount);

		Vector3[] vertices;
		Vector3[] normals;

		if (_jitterAmount < float.Epsilon)
		{
			vertices = _mesh.getVertices ();
			normals = _mesh.getNormals ();
		}
		else
		{
			_mesh.getMorphed 
			(
				(vertex, index) =>
				{
					return vertex + Random.insideUnitSphere * _jitterAmount * 0.5f;
				}, 
			
			out vertices, out normals);
		}
		
		int[] indices = _mesh.getIndices ();

		var mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.normals  = normals;
		mesh.triangles = indices;

		this.GetComponent<MeshFilter> ().mesh = mesh;
	}
	
	void Update () 
	{
		if (_rotate)
			transform.Rotate (Vector3.up, Time.deltaTime * 90.0f);
	}

	static AutoMesh getAutoMesh()
	{
		const float rootY	= 0.0f;
		const float baseY	= 0.2f;
		const float midY	= 0.4f;
		const float topY	= 0.5f;
		const float maxY	= 0.55f;

		const float rootR 	= 0.4f;
		const float baseR	= 0.5f;
		const float midR	= 0.4f;
		const float topR	= 0.3f;
		
		const float arc5	= 360 / 5.0f;
		const float arc8 	= 360 / 8.0f;
		const float arc16 	= 360 / 16.0f;

		// vertices
		var vertices = new List<Vector3> ();

		// root (underground)
		for (int i = 0; i < 8; ++i)
			vertices.Add (Quaternion.AngleAxis (i * arc8, Vector3.up) * new Vector3 (0, rootY, rootR));

		// base (wide part)
		for (int i = 0; i < 16; ++i)
			vertices.Add (Quaternion.AngleAxis ((i + 0.5f) * arc16, Vector3.up) * new Vector3 (0, baseY, baseR));

		// middle
		for (int i = 0; i < 8; ++i)
			vertices.Add (Quaternion.AngleAxis ((i + 0.5f) * arc8, Vector3.up) * new Vector3 (0, midY, midR));

		// top
		for (int i = 0; i < 5; ++i)
			vertices.Add (Quaternion.AngleAxis (i * arc5, Vector3.up) * new Vector3 (0, topY, topR));

		// max
		vertices.Add (new Vector3 (0, maxY, 0));

		// perturb
		// top - vary the height more here

		//edges
		var edges = new List<Edge> ();

		// root
		for (int i = 0; i < 8; ++i)
			edges.Add (Edge.hard (i, (i + 1) % 8));

		// base
		for (int i = 0; i < 16; ++i)
		{
			edges.Add (Edge.hard (8 + i, 8 + (i + 1) % 16));

			// base -> root
			edges.Add (Edge.hard (8 + i, ((i + 1) / 2) % 8));

			// base -> mid
			edges.Add (Edge.hard (8 + i, 24 + i / 2));
		}

		// mid
		int[] toTop = { 32, 33, 34, 34, 35, 35, 36, 32 };

		for (int i = 0; i < 8; ++i)
		{
			edges.Add (Edge.hard (24 + i, 24 + (i + 1) % 8));

			// mid -> top
			edges.Add (Edge.hard (24 + i, toTop [i]));
		}

		// top
		for (int i = 0; i < 5; ++i)
		{
			edges.Add (Edge.hard (32 + i, 32 + (i + 1) % 5));
			
			// top -> max
			edges.Add (Edge.hard (32 + i, 37));
		}

		return new AutoMesh (vertices, edges, true);
	}
}
