using UnityEngine;
using System.Collections.Generic;

public enum Dir { N, S, E, W };

class Nav
{
	public static Dictionary<Dir, int> DX = new Dictionary<Dir, int>()
	{ { Dir.N, 0 }, { Dir.S, 0 }, { Dir.W, -1 }, { Dir.E, 1 } };
	public static Dictionary<Dir, int> DY = new Dictionary<Dir, int>()
	{ { Dir.N, -1 }, { Dir.S, 1 }, { Dir.W, 0 }, { Dir.E, 0 } };

	public static Dictionary<Dir, Dir> left = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.W }, { Dir.E, Dir.N }, { Dir.S, Dir.E }, { Dir.W, Dir.S } };
	public static Dictionary<Dir, Dir> right = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.E }, { Dir.E, Dir.S }, { Dir.S, Dir.W }, { Dir.W, Dir.N } };
	public static Dictionary<Dir, Dir> opposite = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.S }, { Dir.E, Dir.W }, { Dir.S, Dir.N }, { Dir.W, Dir.E } };

	public static Dir GetFacing(float rotation)
	{
		if (rotation < 0.0f)
			rotation += 360.0f;
		rotation /= 90.0f;

		int dir = Mathf.RoundToInt(rotation);
		switch (dir)
		{
			case 0:
			case 4:
				return Dir.W;
			case 1:
				return Dir.N;
			case 2:
				return Dir.E;
			case 3:
				return Dir.S;
			default:
				Debug.Log("What the heck is dir " + dir);
				break;
		}

		return Dir.N;
	}

	public static float GetRotation(Dir facing)
	{
		switch (facing)
		{
			case Dir.N:
				return 0.0f;
			case Dir.E:
				return 90.0f;
			case Dir.S:
				return 180.0f;
			case Dir.W:
				return -90.0f;
		}
		return 0.0f;
	}

	public static Point GetIndexAt(Vector3 position, Vector2 roomDim)
	{
		return new Point(Mathf.RoundToInt(position.z / roomDim.x), Mathf.RoundToInt(position.x / roomDim.y));
	}

	public static Vector3 GetPosAt(Point index, Vector2 roomDim)
	{
		return new Vector3(index.y * roomDim.y, 0f, index.x * roomDim.x);
	}

	public static bool IsConnected(int value, Dir facing)
	{
		if ((value & Room.bits[facing]) != 0)
			return true;
		return false;
	}
}