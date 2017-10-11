using UnityEngine;
using System;
using System.Collections.Generic;

public class Maze : MonoBehaviour
{
	public Point size;

    /// 2D array of rooms in the Maze.
	[HideInInspector]
	public Room[,] rooms;

    /// Size of a room in world dimensions.
	[HideInInspector]
	public Vector2 roomDim;

    /// Euler rotation that the player should start at.
	[HideInInspector]
	public Vector3 startRotation;

    /// <summary>
    /// Initialise the maze.
    /// </summary>
    /// <param name="width">Width of the maze in rooms.</param>
    /// <param name="height">Height of the maze in rooms.</param>
    /// <param name="roomDim">Size of a room in world dimensions.</param>
	public void Initialise(uint width, uint height, Vector2 roomDim)
	{
		size = new Point((int)width, (int)height);
		rooms = new Room[height, width];
		this.roomDim = roomDim;
	}

	public void AddRoom(Room room)
	{
		rooms[room.position.y, room.position.x] = room;
		room.instance.transform.parent = transform;
	}

	public Room GetRoom(Point pos)
	{
		if (pos.x < 0 || pos.y < 0 || pos.x >= rooms.GetLength(1) || pos.y >= rooms.GetLength(0))
			return null;
		return rooms[pos.y, pos.x];
	}

	public List<Room> GetNeighbours(Room room)
	{
		List<Room> neighbours = new List<Room>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(room.value, dir))
			{
				Room neighbour = GetRoom(room.position + new Point(Nav.DX[dir], Nav.DY[dir]));
				if (neighbour != null)
					neighbours.Add(neighbour);
			}
		}
		return neighbours;
	}

	public List<Dir> GetConnections(Room room)
	{
		List<Dir> connections = new List<Dir>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(room.value, dir))
			{
				if (GetRoom(room.position + new Point(Nav.DX[dir], Nav.DY[dir])) != null)
					connections.Add(dir);
			}
		}
		return connections;
	}

    /// <summary>
    /// Parent a GameObject to the Room in the given index.
    /// </summary>
    /// <param name="pos">Index position of the Room to parent the GameObject to.</param>
    /// <param name="item">Item to parent to a Room.</param>
	public void AddItem(Point pos, GameObject item)
	{
		item.transform.SetParent(rooms[pos.y, pos.x].instance.transform, false);
	}

    /// <summary>
    /// Get the world position of the leftmost room that you can move to from the given world position.
    /// </summary>
    /// <param name="position">World position to move from.</param>
    /// <param name="facing">Facing to find the leftmost room from.</param>
    /// <param name="newFacing">New facing towards the leftmost room.</param>
    /// <returns>Position of the leftmost room to move to.</returns>
	public Point MoveLeftmost(Point position, Dir facing, out Dir newFacing)
	{
		Point newPos = new Point(position);
		Dir chosenDir = facing;

		if (GetRoom(position) == null)
		{
			newFacing = facing;
			return newPos;
		}
		uint currentRoomValue = GetRoom(position).value;

        // Check if there's a connected room to the left.
		if (Nav.IsConnected(currentRoomValue, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
			chosenDir = Nav.left[facing];
		}
        // Check if there's a connected room straight ahead.
		else if (Nav.IsConnected(currentRoomValue, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
			chosenDir = facing;
		}
        // Check if there's a connected room to the right.
		else if (Nav.IsConnected(currentRoomValue, Nav.right[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.right[facing]], position.y + Nav.DY[Nav.right[facing]]);
			chosenDir = Nav.right[facing];
		}
        // Hit a dead end, move back in the opposite direction.
		else
		{
			newPos.Set(position.x + Nav.DX[Nav.opposite[facing]], position.y + Nav.DY[Nav.opposite[facing]]);
			chosenDir = Nav.opposite[facing];
		}

		newFacing = chosenDir;
		return newPos;
	}

    /// <summary>
    /// Get the world position of the room straight ahead from the given world position.
    /// </summary>
    /// <param name="position">World position to move from.</param>
    /// <param name="facing">Facing to find the room straight ahead from.</param>
    /// <param name="allowUTurns">If U turns are allowed.</param>
    /// <returns>World position of the room straight ahead to move to. Same as the input world position if U turns weren't allowed and one was hit.</returns>
	public Point MoveStraight(Point position, Dir facing, bool allowUTurns = true)
	{
		Point newPos = new Point(position);

		if (GetRoom(position) == null)
			return newPos;
		uint currentRoomValue = GetRoom(position).value;

        // Check if there's a connected room straight ahead.
		if (Nav.IsConnected(currentRoomValue, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
		}
        // Check if there's a connected room to the left.
		else if (Nav.IsConnected(currentRoomValue, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
		}
        // Check if there's a connected room to the right.
		else if (Nav.IsConnected(currentRoomValue, Nav.right[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.right[facing]], position.y + Nav.DY[Nav.right[facing]]);
		}
        // If U turns are allowed and one was hit, move back in the opposite direction.
		else if (allowUTurns)
		{
			newPos.Set(position.x + Nav.DX[Nav.opposite[facing]], position.y + Nav.DY[Nav.opposite[facing]]);
		}

		return newPos;
	}

	public Vector3 RoomToWorldPosition(Point roomPos)
	{
		return Nav.IndexToWorldPos(roomPos, roomDim);
	}

	public Point WorldToRoomPosition(Vector3 worldPos)
	{
		return Nav.WorldToIndexPos(worldPos, roomDim);
	}
}
