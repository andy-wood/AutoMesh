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

public class Cube : MonoBehaviour 
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
		Vector3[] vertices =
		{
			new Vector3 (-0.5f, -0.5f, -0.5f),
			new Vector3 (-0.5f,  0.5f, -0.5f),
			new Vector3 ( 0.5f,  0.5f, -0.5f),
			new Vector3 ( 0.5f, -0.5f, -0.5f),
			new Vector3 (-0.5f, -0.5f,  0.5f),
			new Vector3 (-0.5f,  0.5f,  0.5f),
			new Vector3 ( 0.5f,  0.5f,  0.5f),
			new Vector3 ( 0.5f, -0.5f,  0.5f)
		};

		Edge[] edges =
		{
			Edge.hard (0, 1),
			Edge.hard (1, 2),
			Edge.hard (2, 3),
			Edge.hard (3, 0),

			Edge.hard (4, 5),
			Edge.hard (5, 6),
			Edge.hard (6, 7),
			Edge.hard (7, 4),

			Edge.hard (0, 4),
			Edge.hard (1, 5),
			Edge.hard (2, 6),
			Edge.hard (3, 7)
		};

		return new AutoMesh (vertices, edges);
	}
}
