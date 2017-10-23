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

	/// <summary>
	/// Lists the neighbouring tiles for a specified tile.
	/// </summary>
	public List<Tile> GetNeighbours(Tile tile)
	{
		List<Tile> neighbours = new List<Tile>();
		List<Dir> connections = Nav.GetConnections(tile.value);
		foreach (Dir dir in connections)
		{
			Tile neighbour = GetTile(tile.position + new Point(Nav.DX[dir], Nav.DY[dir]));
			if (neighbour != null)
				neighbours.Add(neighbour);
		}
		return neighbours;
	}

	/// <summary>
	/// Lists the cardinal directions a tile is connected to other tiles in.
	/// Directions where there aren't neighbouring tiles aren't included, use GetConnections in Nav if they should be.
	/// </summary>
	public List<Dir> GetConnections(Tile tile)
	{
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

	public enum MovementPreference { Leftmost, Straight	}

    /// <summary>
    /// Move forwards by one tile in the maze from a position towards a direction.
    /// </summary>
    /// <param name="preference">The direction to prioritise.</param>
    /// <returns>Position of the next tile to move to. Same as the input if we couldn't move.</returns>
	public Point MoveForwards(Point position, Dir facing, MovementPreference preference, bool allowUTurns = false)
	{
		if (GetTile(position) == null)
			return position;
		uint currentTileValue = GetTile(position).value;

		switch (preference)
		{
			case MovementPreference.Leftmost:
				if (Nav.IsConnected(currentTileValue, Nav.left[facing]))
					return position + new Point(Nav.DX[Nav.left[facing]], Nav.DY[Nav.left[facing]]);
				else if (Nav.IsConnected(currentTileValue, facing))
					return position + new Point(Nav.DX[facing], Nav.DY[facing]);
				break;
			case MovementPreference.Straight:
				if (Nav.IsConnected(currentTileValue, facing))
					return position + new Point(Nav.DX[facing], Nav.DY[facing]);
				else if (Nav.IsConnected(currentTileValue, Nav.left[facing]))
					return position + new Point(Nav.DX[Nav.left[facing]], Nav.DY[Nav.left[facing]]);
				break;
		}

		if (Nav.IsConnected(currentTileValue, Nav.right[facing]))
			return position + new Point(Nav.DX[Nav.right[facing]], Nav.DY[Nav.right[facing]]);
		
		// Hit a dead end.
		if (!allowUTurns)
			return position;
		return position + new Point(Nav.DX[Nav.opposite[facing]], Nav.DY[Nav.opposite[facing]]);
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
