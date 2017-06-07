using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object representing a room in a maze.
/// </summary>
public class Room
{
    /// <summary>
    /// Bits to set in the bitwise room value depending on the direction of other rooms this one is connected to.
    /// </summary>
	public static Dictionary<Dir, uint> bits = new Dictionary<Dir, uint>()
	{ { Dir.N, 1 }, { Dir.E, 2 }, { Dir.S, 4 }, { Dir.W, 8 } };

	public Room(uint value, Point position, GameObject instance)
	{
		this.value = value;
        this.position = position;
		this.instance = instance;
	}

    /// <summary>
    /// Bitwise value. Represents which directions this room is connected to other rooms in.
    /// </summary>
	public uint value = 0;
    /// <summary>
    /// Index position in the maze.
    /// </summary>
    public Point position = null;
    /// <summary>
    /// Scene instance.
    /// </summary>
	public GameObject instance = null;
}
