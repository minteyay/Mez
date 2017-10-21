using UnityEngine;
using System.Collections.Generic;

/// Cardinal directions.
public enum Dir { N, S, E, W };

/// <summary>
/// Class containing utilities for navigating around the maze.
/// </summary>
class Nav
{
	/// Bit values for the cardinal directions in a tile's bitwise value.
	public static Dictionary<Dir, uint> bits = new Dictionary<Dir, uint>()
	{ { Dir.N, 1 }, { Dir.E, 2 }, { Dir.S, 4 }, { Dir.W, 8 } };

	/// Movement along the X-axis in a cardinal direction.
	public static Dictionary<Dir, int> DX = new Dictionary<Dir, int>()
	{ { Dir.N, 0 }, { Dir.S, 0 }, { Dir.W, -1 }, { Dir.E, 1 } };
	/// Movement along the Y-axis in a cardinal direction.
	public static Dictionary<Dir, int> DY = new Dictionary<Dir, int>()
	{ { Dir.N, -1 }, { Dir.S, 1 }, { Dir.W, 0 }, { Dir.E, 0 } };

	public static Dictionary<Dir, Dir> left = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.W }, { Dir.E, Dir.N }, { Dir.S, Dir.E }, { Dir.W, Dir.S } };
	public static Dictionary<Dir, Dir> right = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.E }, { Dir.E, Dir.S }, { Dir.S, Dir.W }, { Dir.W, Dir.N } };
	public static Dictionary<Dir, Dir> opposite = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.S }, { Dir.E, Dir.W }, { Dir.S, Dir.N }, { Dir.W, Dir.E } };

	/// <summary>
	/// Converts an angle (in degrees) to a cardinal direction.
	/// </summary>
	public static Dir AngleToFacing(float angle)
	{
		// Scale the angle from 360 degrees down to a quadrant of a circle.
		while (angle < 0.0f)
			angle += 360.0f;
		angle /= 90.0f;

		// Round to the closest quadrant.
		int dir = Mathf.RoundToInt(angle) % 4;
		switch (dir)
		{
			case 0:
				return Dir.E;
			case 1:
				return Dir.S;
			case 2:
				return Dir.W;
			case 3:
				return Dir.N;
			default:
				Debug.Log("What the heck is dir " + dir);
				break;
		}

		return Dir.N;
	}

    /// <summary>
	/// Converts a cardinal direction to an angle (in degrees).
	/// </summary>
	public static float FacingToAngle(Dir facing)
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

	/// <summary>
	/// Converts a delta to a cardinal direction.
	/// </summary>
	public static Dir DeltaToFacing(Point delta)
	{
		float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
		return AngleToFacing(angle);
	}

    /// <summary>
    /// Converts a world position into a tile position in the maze, depending on tile size.
    /// </summary>
	public static Point WorldToTilePos(Vector3 position, Vector2 tileDim)
	{
		return new Point(Mathf.RoundToInt(position.z / tileDim.x), Mathf.RoundToInt(position.x / tileDim.y));
	}

    /// <summary>
    /// Converts a tile position in the maze into a world position, depending on tile size.
    /// </summary>
	public static Vector3 TileToWorldPos(Point tilePos, Vector2 tileDim)
	{
		return new Vector3(tilePos.y * tileDim.y, 0f, tilePos.x * tileDim.x);
	}

    /// <summary>
    /// Checks if a tile is connected in a cardinal direction.
    /// </summary>
	public static bool IsConnected(uint value, Dir facing)
	{
		return (value & bits[facing]) != 0;
	}

	/// <summary>
	/// Lists all the cardinal directions a tile is connected in.
	/// </summary>
	public static List<Dir> GetConnections(uint value)
	{
		List<Dir> connections = new List<Dir>();
		foreach (Dir dir in System.Enum.GetValues(typeof(Dir)))
			if (IsConnected(value, dir))
				connections.Add(dir);
		return connections;
	}
}