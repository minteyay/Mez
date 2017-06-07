using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object representing a room in a maze.
/// </summary>
public class Room
{
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
