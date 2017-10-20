using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Cardinal compass directions.
/// </summary>
public enum Dir { N, S, E, W };

/// <summary>
/// Contains static helpers for navigating around the maze.
/// </summary>
class Nav
{
	/// <summary>
    /// Bits to set in the bitwise tile value depending on the direction of other tiles it's connected to.
    /// </summary>
	public static Dictionary<Dir, uint> bits = new Dictionary<Dir, uint>()
	{ { Dir.N, 1 }, { Dir.E, 2 }, { Dir.S, 4 }, { Dir.W, 8 } };

	/*
	 * How to move on the X and Y axes depending on the direction.
	 * For example: When moving in Dir.N, the x coordinate doesn't change (DX[Dir.N] == 0), and the Y coordinate changes by -1 (DY[Dir.N]).
	 */
	public static Dictionary<Dir, int> DX = new Dictionary<Dir, int>()
	{ { Dir.N, 0 }, { Dir.S, 0 }, { Dir.W, -1 }, { Dir.E, 1 } };
	public static Dictionary<Dir, int> DY = new Dictionary<Dir, int>()
	{ { Dir.N, -1 }, { Dir.S, 1 }, { Dir.W, 0 }, { Dir.E, 0 } };

	/*
	 * Relative directions used mostly when turning.
	 * For example, the direction opposite to Dir.S is Dir.N (opposite[Dir.S]).
	 */
	public static Dictionary<Dir, Dir> left = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.W }, { Dir.E, Dir.N }, { Dir.S, Dir.E }, { Dir.W, Dir.S } };
	public static Dictionary<Dir, Dir> right = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.E }, { Dir.E, Dir.S }, { Dir.S, Dir.W }, { Dir.W, Dir.N } };
	public static Dictionary<Dir, Dir> opposite = new Dictionary<Dir, Dir>()
	{ { Dir.N, Dir.S }, { Dir.E, Dir.W }, { Dir.S, Dir.N }, { Dir.W, Dir.E } };

	/// <summary>
	/// Changes a Y angle value to a cardinal compass direction.
	/// </summary>
	/// <param name="angle">Y angle in the range 0-360.</param>
	/// <returns>Cardinal compass direction.</returns>
	public static Dir AngleToFacing(float angle)
	{
		// Scale the angle from 360 degrees to a quadrant of a circle (0-4)
		while (angle < 0.0f)
			angle += 360.0f;
		angle /= 90.0f;

		// Round to the closest quadrant of angle
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
    /// Turns a cardinal compass direction into a Y angle.
    /// </summary>
    /// <param name="facing">Cardinal compass direction.</param>
    /// <returns>Y angle value.</returns>
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
    /// Turns a world position into an tile position in the maze, depending on the size of a tile in the maze.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="tileDim">Size of a tile in the maze.</param>
    /// <returns>Tile position in the maze.</returns>
	public static Point WorldToTilePos(Vector3 position, Vector2 tileDim)
	{
		return new Point(Mathf.RoundToInt(position.z / tileDim.x), Mathf.RoundToInt(position.x / tileDim.y));
	}

    /// <summary>
    /// Turns an tile position in the maze into a world position, depending on the size of a tile in the maze.
    /// </summary>
    /// <param name="tilePos">Tile position in the maze.</param>
    /// <param name="tileDim">Size of a tile in the maze.</param>
    /// <returns>World position.</returns>
	public static Vector3 TileToWorldPos(Point tilePos, Vector2 tileDim)
	{
		return new Vector3(tilePos.y * tileDim.y, 0f, tilePos.x * tileDim.x);
	}

    /// <summary>
    /// Checks if a tile is connected to a cardinal direction.
    /// This is done by checking if the correct bit is set in the input value.
    /// </summary>
    /// <param name="value">Bitwise connection value of a tile.</param>
    /// <param name="facing">Cardinal direction to check for a connection.</param>
    /// <returns>True if the tile is connected in the cardinal direction facing, false if not.</returns>
	public static bool IsConnected(uint value, Dir facing)
	{
		return (value & bits[facing]) != 0;
	}

	public static List<Dir> GetConnections(uint value)
	{
		List<Dir> connections = new List<Dir>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
			if (IsConnected(value, dir))
				connections.Add(dir);
		return connections;
	}
}