/*
	Copyright 2017 Andrew A. Wood

    email: projecteterna@gmail.com
    twitter: @projecteterna
	
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

This is a reference implementation of an automatic procedural mesh generator.
It is provided primarily for educational purposes. It takes a simple list of
vertices and un-directed edges, and automatically generates a triangulated 
mesh, with consistent winding order, and normals. You can specify 'hardness'
per edge, and normals will be generated to simulate that look.

See the included Unity sample for usage.

The AutoMesh library itself is in /Assets/Scripts/AutoMesh
What's in that folder should not have any dependecies outside the folder.