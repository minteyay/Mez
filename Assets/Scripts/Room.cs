using UnityEngine;
using System.Collections.Generic;

/// Object representing a room in a maze.
public class Room
{
	public Room(uint value, Point position)
	{
		this.value = value;
        this.position = position;
        instance = new GameObject(value.ToString());
	}

    /// Bitwise value. Represents which directions this room is connected to other rooms in.
	public uint value = 0;

    /// The tileset to use for this room.
    public string theme = "";

    /// Index position in the maze.
    public Point position = null;

    /// Scene instance.
	public GameObject instance = null;
}
