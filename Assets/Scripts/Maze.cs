﻿using UnityEngine;
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

    /// Object indicating the end of the maze.
	[HideInInspector]
	public GameObject endPoint = null;

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
    /// Add an endpoint to the Maze.
    /// </summary>
    /// <param name="pos">Index position of the endpoint.</param>
    /// <param name="endPoint">GameObject for the endpoint.</param>
    /// <param name="rotation">Rotation of the endpoint.</param>
	public void AddEndPoint(Point pos, GameObject endPoint, Quaternion rotation)
	{
		this.endPoint = (GameObject)Instantiate(endPoint,
			new Vector3(pos.y * roomDim.y, 0.0f, pos.x * roomDim.x),
			rotation);
		this.endPoint.name = "End Point";
		this.endPoint.transform.parent = transform;
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

        // Check if there's a connected room to the left.
		if (IsConnected(position, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
			chosenDir = Nav.left[facing];
		}
        // Check if there's a connected room straight ahead.
		else if (IsConnected(position, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
			chosenDir = facing;
		}
        // Check if there's a connected room to the right.
		else if (IsConnected(position, Nav.right[facing]))
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
        // Check if there's a connected room straight ahead.
		if (IsConnected(position, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
		}
        // Check if there's a connected room to the left.
		else if (IsConnected(position, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
		}
        // Check if there's a connected room to the right.
		else if (IsConnected(position, Nav.right[facing]))
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

    /// <summary>
    /// Checks if the Room in the given index position is connected to another Room in the given direction.
    /// </summary>
    /// <param name="pos">Index position to check room connection from.</param>
    /// <param name="dir">Direction to check for room connection.</param>
    /// <returns>True if the room in the given index position was connected in the given direction, false if not.</returns>
	private bool IsConnected(Point pos, Dir dir)
	{
		Point newPos = new Point(pos.x + Nav.DX[dir], pos.y + Nav.DY[dir]);
		if (newPos.x >= 0 && newPos.x < rooms.GetLength(1) && newPos.y >= 0 && newPos.y < rooms.GetLength(0))
		{
			if (Nav.IsConnected(rooms[newPos.y, newPos.x].value, Nav.opposite[dir]))
				return true;
		}
		return false;
	}
}
