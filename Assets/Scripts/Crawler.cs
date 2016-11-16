using UnityEngine;
using System.Collections.Generic;

class Crawler
{
	public static void SetRoomShades(Maze maze, Vector3 start, Dir facing, List<Material> shadeMaterials)
	{
		int step = shadeMaterials.Count - 1;

		Dir prevFacing = facing;
		Vector3 prevPos = start;

		Point startPoint = Nav.GetIndexAt(prevPos, maze.roomDim);
		maze.rooms[startPoint.y, startPoint.x].instance.GetComponent<MaterialSetter>().SetMaterial(shadeMaterials[step]);

		while (step > 0)
		{
			step--;

			Vector3 newPos = maze.MoveLeftmost(prevPos, prevFacing);
			Point newPoint = Nav.GetIndexAt(newPos, maze.roomDim);

			maze.rooms[newPoint.y, newPoint.x].instance.GetComponent<MaterialSetter>().SetMaterial(shadeMaterials[step]);

			Vector3 posDelta = prevPos - newPos;
			float targetAngle = Quaternion.LookRotation(posDelta, Vector3.up).eulerAngles.y;
			prevFacing = Nav.GetFacing(targetAngle);

			prevPos = newPos;
		}
	}
}
