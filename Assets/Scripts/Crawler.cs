using UnityEngine;
using System;
using System.Collections.Generic;

class Crawler
{
	public static void SetRoomShades(Maze maze, Vector3 start, Dir facing, List<Material> shadeMaterials, int step = -1)
	{
		if (step == -1)
			step = shadeMaterials.Count - 1;

		Dir prevFacing = facing;
		Vector3 prevPos = start;

		Point startPoint = Nav.GetIndexAt(prevPos, maze.roomDim);
		maze.rooms[startPoint.y, startPoint.x].instance.GetComponent<MaterialSetter>().SetMaterial(shadeMaterials[step]);

		while (step > 0)
		{
			step--;

			Vector3 newPos = maze.MoveStraight(prevPos, prevFacing, false);
			if (newPos == prevPos)
				return;
			Point newPoint = Nav.GetIndexAt(newPos, maze.roomDim);

			maze.rooms[newPoint.y, newPoint.x].instance.GetComponent<MaterialSetter>().SetMaterial(shadeMaterials[step]);
			Transform lamp = maze.rooms[newPoint.y, newPoint.x].instance.transform.Find("Lamp");
			if (lamp != null)
				MonoBehaviour.Destroy(lamp.gameObject);

			Vector3 posDelta = prevPos - newPos;
			float targetAngle = Quaternion.LookRotation(posDelta, Vector3.up).eulerAngles.y;
			prevFacing = Nav.GetFacing(targetAngle);

			List<Dir> connections = new List<Dir>();
			foreach (Dir dir in Enum.GetValues(typeof(Dir)))
			{
				if (dir != Nav.opposite[prevFacing])
					if (Nav.IsConnected(maze.rooms[newPoint.y, newPoint.x].value, dir))
						connections.Add(dir);
			}
			if (connections.Count > 1)
			{
				foreach (Dir dir in connections)
				{
					if (step > 0)
						SetRoomShades(maze, newPos, dir, shadeMaterials, step);
				}
				return;
			}

			prevPos = newPos;
		}
	}
}
