using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// An object representing a 2D maze.
/// Requires initialisation before use.
/// </summary>
public class Maze : MonoBehaviour
{
	/// Size of the maze in tiles.
	public Point size { get; private set; }
	/// Size of a tile in world dimensions.
	public Vector2 tileSize { get; private set; }

	[HideInInspector] public Point startPosition;
	[HideInInspector] public uint entranceLength = 0;
	[HideInInspector] public string defaultTheme = "";

	private Tile[,] _tiles;

    /// <param name="width">Width of the maze in tiles.</param>
    /// <param name="height">Height of the maze in tiles.</param>
    /// <param name="tileSize">Size of a tile in world dimensions.</param>
	public void Initialise(uint width, uint height, Vector2 tileSize)
	{
		size = new Point((int)width, (int)height);
		_tiles = new Tile[height, width];
		this.tileSize = tileSize;
	}

	public void AddTile(Tile tile)
	{
		_tiles[tile.position.y, tile.position.x] = tile;
		tile.instance.transform.parent = transform;
	}

	/// <returns>Tile in the given position, null if the position is out of bounds.</returns>
	public Tile GetTile(int x, int y)
	{
		if (x < 0 || y < 0 || x >= size.x || y >= size.y)
			return null;
		return _tiles[y, x];
	}

	/// <returns>Tile in the given position, null if the position is out of bounds.</returns>
	public Tile GetTile(Point position)
	{
		return GetTile(position.x, position.y);
	}

	public List<Tile> GetNeighbours(Tile tile)
	{
		// TODO: Use the functions in Utils.
		List<Tile> neighbours = new List<Tile>();
		foreach (Dir dir in System.Enum.GetValues(typeof(Dir)))
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
		// TODO: Use the functions in Utils.
		List<Dir> connections = new List<Dir>();
		foreach (Dir dir in System.Enum.GetValues(typeof(Dir)))
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
    /// Add an item to the tile in the given position.
    /// </summary>
	public void AddItem(Point position, GameObject item)
	{
		// TODO: Out of bounds checking.
		item.transform.SetParent(_tiles[position.y, position.x].instance.transform, false);
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
		// TODO: Combine this with MoveStraight.
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

	/// <summary>
    /// Converts a tile position in the maze into a world position.
    /// </summary>
	public Vector3 TileToWorldPosition(Point tilePos)
	{
		return Nav.TileToWorldPos(tilePos, tileSize);
	}

	/// <summary>
    /// Converts a world position into a tile position in the maze.
    /// </summary>
	public Point WorldToTilePosition(Vector3 worldPos)
	{
		return Nav.WorldToTilePos(worldPos, tileSize);
	}
}
