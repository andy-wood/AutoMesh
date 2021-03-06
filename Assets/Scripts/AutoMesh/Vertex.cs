﻿/*
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
