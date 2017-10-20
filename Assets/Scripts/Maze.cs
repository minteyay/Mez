using UnityEngine;
using System;
using System.Collections.Generic;

public class Maze : MonoBehaviour
{
	public Point size;

    /// 2D array of tiles in the Maze.
	[HideInInspector]
	public Tile[,] tiles;

    /// Size of a tile in world dimensions.
	[HideInInspector]
	public Vector2 tileDim;

	[HideInInspector]
	public Point startPosition;

	[HideInInspector]
	public uint entranceLength = 0;

	[HideInInspector]
	public string defaultTheme = "";

    /// <summary>
    /// Initialise the maze.
    /// </summary>
    /// <param name="width">Width of the maze in tiles.</param>
    /// <param name="height">Height of the maze in tiles.</param>
    /// <param name="tileDim">Size of a tile in world dimensions.</param>
	public void Initialise(uint width, uint height, Vector2 tileDim)
	{
		size = new Point((int)width, (int)height);
		tiles = new Tile[height, width];
		this.tileDim = tileDim;
	}

	public void AddTile(Tile tile)
	{
		tiles[tile.position.y, tile.position.x] = tile;
		tile.instance.transform.parent = transform;
	}

	public Tile GetTile(Point pos)
	{
		if (pos.x < 0 || pos.y < 0 || pos.x >= tiles.GetLength(1) || pos.y >= tiles.GetLength(0))
			return null;
		return tiles[pos.y, pos.x];
	}

	public List<Tile> GetNeighbours(Tile tile)
	{
		List<Tile> neighbours = new List<Tile>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(tile.value, dir))
			{
				Tile neighbour = GetTile(tile.position + new Point(Nav.DX[dir], Nav.DY[dir]));
				if (neighbour != null)
					neighbours.Add(neighbour);
			}
		}
		return neighbours;
	}

	public List<Dir> GetConnections(Tile tile)
	{
		List<Dir> connections = new List<Dir>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(tile.value, dir))
			{
				if (GetTile(tile.position + new Point(Nav.DX[dir], Nav.DY[dir])) != null)
					connections.Add(dir);
			}
		}
		return connections;
	}

    /// <summary>
    /// Parent a GameObject to the Tile in the given index.
    /// </summary>
    /// <param name="pos">Index position of the Tile to parent the GameObject to.</param>
    /// <param name="item">Item to parent to a Tile.</param>
	public void AddItem(Point pos, GameObject item)
	{
		item.transform.SetParent(tiles[pos.y, pos.x].instance.transform, false);
	}

    /// <summary>
    /// Get the world position of the leftmost tile that you can move to from the given world position.
    /// </summary>
    /// <param name="position">World position to move from.</param>
    /// <param name="facing">Facing to find the leftmost tile from.</param>
    /// <param name="newFacing">New facing towards the leftmost tile.</param>
    /// <returns>Position of the leftmost tile to move to.</returns>
	public Point MoveLeftmost(Point position, Dir facing, out Dir newFacing)
	{
		Point newPos = new Point(position);
		Dir chosenDir = facing;

		if (GetTile(position) == null)
		{
			newFacing = facing;
			return newPos;
		}
		uint currentTileValue = GetTile(position).value;

        // Check if there's a connected tile to the left.
		if (Nav.IsConnected(currentTileValue, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
			chosenDir = Nav.left[facing];
		}
        // Check if there's a connected tile straight ahead.
		else if (Nav.IsConnected(currentTileValue, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
			chosenDir = facing;
		}
        // Check if there's a connected tile to the right.
		else if (Nav.IsConnected(currentTileValue, Nav.right[facing]))
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
    /// Get the world position of the tile straight ahead from the given world position.
    /// </summary>
    /// <param name="position">World position to move from.</param>
    /// <param name="facing">Facing to find the tile straight ahead from.</param>
    /// <param name="allowUTurns">If U turns are allowed.</param>
    /// <returns>World position of the tile straight ahead to move to. Same as the input world position if U turns weren't allowed and one was hit.</returns>
	public Point MoveStraight(Point position, Dir facing, bool allowUTurns = true)
	{
		Point newPos = new Point(position);

		if (GetTile(position) == null)
			return newPos;
		uint currentTileValue = GetTile(position).value;

        // Check if there's a connected tile straight ahead.
		if (Nav.IsConnected(currentTileValue, facing))
		{
			newPos.Set(position.x + Nav.DX[facing], position.y + Nav.DY[facing]);
		}
        // Check if there's a connected tile to the left.
		else if (Nav.IsConnected(currentTileValue, Nav.left[facing]))
		{
			newPos.Set(position.x + Nav.DX[Nav.left[facing]], position.y + Nav.DY[Nav.left[facing]]);
		}
        // Check if there's a connected tile to the right.
		else if (Nav.IsConnected(currentTileValue, Nav.right[facing]))
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

	public Vector3 TileToWorldPosition(Point tilePos)
	{
		return Nav.TileToWorldPos(tilePos, tileDim);
	}

	public Point WorldToTilePosition(Vector3 worldPos)
	{
		return Nav.WorldToTilePos(worldPos, tileDim);
	}
}
