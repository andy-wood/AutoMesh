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
