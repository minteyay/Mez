using UnityEngine;
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
	/// Changes a Y rotation value to a cardinal compass direction.
	/// </summary>
	/// <param name="rotation">Y rotation in the range 0-360.</param>
	/// <returns>Cardinal compass direction.</returns>
	public static Dir GetFacing(float rotation)
	{
		// Scale the rotation from 360 degrees to a quadrant of a circle (0-4)
		if (rotation < 0.0f)
			rotation += 360.0f;
		rotation /= 90.0f;

		// Round to the closest quadrant of rotation
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

    /// <summary>
    /// Turns a cardinal compass direction into a Y rotation.
    /// </summary>
    /// <param name="facing">Cardinal compass direction.</param>
    /// <returns>Y rotation value.</returns>
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

    /// <summary>
    /// Turns a world position into an index position in the maze, depending on the size of a room in the maze.
    /// </summary>
    /// <param name="position">World position.</param>
    /// <param name="roomDim">Size of a room in the maze.</param>
    /// <returns>Index position in the maze.</returns>
	public static Point GetIndexAt(Vector3 position, Vector2 roomDim)
	{
		return new Point(Mathf.RoundToInt(position.z / roomDim.x), Mathf.RoundToInt(position.x / roomDim.y));
	}

    /// <summary>
    /// Turns an index position in the maze into a world position, depending on the size of a room in the maze.
    /// </summary>
    /// <param name="index">Index position in the maze.</param>
    /// <param name="roomDim">Size of a room in the maze.</param>
    /// <returns>World position.</returns>
	public static Vector3 GetPosAt(Point index, Vector2 roomDim)
	{
		return new Vector3(index.y * roomDim.y, 0f, index.x * roomDim.x);
	}

    /// <summary>
    /// Checks if a room is connected to a cardinal direction.
    /// This is done by checking if the correct bit is set in the input value.
    /// </summary>
    /// <param name="value">Bitwise connection value of a room.</param>
    /// <param name="facing">Cardinal direction to check for a connection.</param>
    /// <returns>True if the room is connected in the cardinal direction facing, false if not.</returns>
	public static bool IsConnected(int value, Dir facing)
	{
		return (value & Room.bits[facing]) != 0;
	}
}