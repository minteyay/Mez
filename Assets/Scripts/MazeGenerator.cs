using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    /// Size of a room in world dimensions.
	public Vector2 roomDim;

    /// Floor model prefab.
	public GameObject floor = null;
	/// Wall model prefab.
	public GameObject wall = null;
	/// Ceiling model prefab.
	public GameObject ceiling = null;

	/// Default maze material.
	public Material defaultMaterial = null;

    // End point prefab.
	public GameObject endPoint = null;
    // Distance to the end point from the start of the maze.
	private uint endPointDist = 0;
    // Index position of the end point.
	private Point endPointCoord = new Point(-1, -1);
    // Y rotation of the end point.
	private float endPointRotation = 0.0f;

    // Random number generator to use in generating the maze.
	private System.Random rnd = null;

	void Awake()
	{
		rnd = new System.Random();
	}

    /// <summary>
    /// Generates a maze with the given dimensions.
    /// </summary>
    /// <returns>A brand-new maze to play with.</returns>
	public Maze GenerateMaze(uint width, uint height)
	{
		endPointDist = 0;
		endPointCoord = new Point(-1, -1);

		uint[,] grid = new uint[height, width];
        // Generate the maze.
		CarvePassagesFrom(0, 0, grid, 0);
		//PrintGrid(grid);

        // Base GameObject for the maze.
		GameObject mazeInstance = new GameObject();
		mazeInstance.name = "Maze";
		Maze maze = mazeInstance.AddComponent<Maze>();
		if (maze == null)
		{
			Debug.Log("Maze prefab has no Maze script attached!");
			return null;
		}
		maze.Initialise(width, height, roomDim);

		CreateRooms(grid, maze);
		CreateRoomGeometry(maze);

		UpdateMazeUVs(maze);

		maze.AddEndPoint(endPointCoord, endPoint, Quaternion.Euler(0.0f, endPointRotation, 0.0f));

        // Set the starting rotation based on the facing of the starting room.
		switch (grid[0, 0])
		{
			case 2:
				maze.startRotation = new Vector3(0.0f, 180.0f, 0.0f);
				break;
			case 4:
				maze.startRotation = new Vector3(0.0f, -90.0f, 0.0f);
				break;
			default:
				Debug.Log("Weird starting room " + grid[0, 0]);
				break;
		}

		return maze;
	}

    /// <summary>
    /// <para>Generates a maze into a 2D array.</para>
    /// <para>Calls itself recursively until the maze is complete.</para>
    /// <para>Uses bitwise integers to denote directions a room is connected in (these can be found in Room).</para>
    /// </summary>
    /// <param name="x">Index x position to continue generating the maze from.</param>
    /// <param name="y">Index y position to continue generating the maze from.</param>
    /// <param name="grid">2D array to generate the maze into.</param>
    /// <param name="distance">Distance from the beginning of the maze (in rooms).</param>
	private void CarvePassagesFrom(int x, int y, uint[,] grid, uint distance)
	{
        // Try moving in directions in a random order.
		List<Dir> directions = new List<Dir> { Dir.N, Dir.S, Dir.E, Dir.W };
		Utils.Shuffle(rnd, directions);

		foreach (Dir dir in directions)
		{
            // Calculate new index position in the direction to try to move in.
			int nx = x + Nav.DX[dir];
			int ny = y + Nav.DY[dir];

            // Check that the new position is within the maze dimensions.
			if ((ny >= 0 && ny < grid.GetLength(0)) && (nx >= 0 && nx < grid.GetLength(1)) && (grid[ny, nx] == 0))
			{
                // Set the connection bits in this new room and the room we came from.
				grid[y, x] |= Nav.bits[dir];
				grid[ny, nx] |= Nav.bits[Nav.opposite[dir]];

                // Continue generating the maze.
				CarvePassagesFrom(nx, ny, grid, distance + 1);
			}
		}

        // Set the end point here if its deeper in the maze than the previous one.
		if (distance > endPointDist)
		{
			endPointCoord.Set(x, y);
			endPointDist = distance;

            // Check which way to face the end point in.
			foreach (Dir dir in directions)
			{
				if (Nav.IsConnected(grid[y, x], dir))
				{
					endPointRotation = Nav.GetRotation(dir);
					break;
				}
			}
		}
	}

    /// <summary>
    /// Creates rooms into a Maze according to the bitwise data in a given 2D array.
    /// </summary>
    /// <param name="grid">2D grid of bitwise room values.</param>
    /// <param name="maze">Maze to add the rooms into.</param>
	private void CreateRooms(uint[,] grid, Maze maze)
	{
		for (uint y = 0; y < grid.GetLength(0); y++)
		{
			for (uint x = 0; x < grid.GetLength(1); x++)
			{
                // Create the room and add it to the Maze.
				Room room = new Room(grid[y, x], new Point((int)x, (int)y));
				if (y <= 1)
					room.theme = "blep";
				maze.AddRoom(room);
			}
		}
	}

	/// <summary>
	/// Creates the room geometries for all the rooms in a maze.
	/// </summary>
	/// <param name="maze">Maze to generate room geometries for.</param>
	private void CreateRoomGeometry(Maze maze)
	{
		for (uint y = 0; y < maze.rooms.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.rooms.GetLength(1); x++)
			{
				maze.rooms[y, x].instance.AddComponent<MaterialSetter>();
				maze.rooms[y, x].instance.transform.position = new Vector3(y * roomDim.y, 0.0f, x * roomDim.x);

				CreateFloor(maze.rooms[y, x]);
				CreateCeiling(maze.rooms[y, x]);
				CreateWalls(maze.rooms[y, x]);
			}
		}
	}

	/// <summary>
	/// Creates the floor instance for a Room and parents it.
	/// </summary>
	/// <param name="room"></param>
	private void CreateFloor(Room room)
	{
		GameObject floorInstance = (GameObject)Instantiate(floor,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[room.value], 0.0f));
		floorInstance.name = "Floor";
		floorInstance.transform.SetParent(room.instance.transform, false);
	}

	/// <summary>
	/// Creates the ceiling instance for a Room and parents it.
	/// </summary>
	/// <param name="room"></param>
	private void CreateCeiling(Room room)
	{
		GameObject ceilingInstance = (GameObject)Instantiate(ceiling,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[room.value], 0.0f));
		ceilingInstance.name = "Ceiling";
		ceilingInstance.transform.SetParent(room.instance.transform, false);
	}

	/// <summary>
	/// Creates wall instances for a Room and parents them.
	/// </summary>
	/// <param name="room"></param>
	private void CreateWalls(Room room)
	{
        // Walls base GameObject.
		GameObject wallsInstance = new GameObject("Walls");
		wallsInstance.AddComponent<MaterialSetter>();
		wallsInstance.transform.SetParent(room.instance.transform, false);

        // Check all directions to see if there should be a wall facing that way.
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
            // If the room isn't connected in this direction, create a wall.
			if ((room.value & Nav.bits[dir]) == 0)
			{
				GameObject wallInstance = (GameObject)Instantiate(wall, new Vector3(),
						Quaternion.Euler(0.0f, Nav.GetRotation(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-roomDim.y / 2.0f, 0.0f, 0.0f);
				wallInstance.name = Nav.bits[dir].ToString();
			}
		}
	}

	/// <summary>
	/// Updates the UV coordinates of all Room meshes in a Maze by autotiling them.
	/// </summary>
	/// <param name="maze"></param>
	private void UpdateMazeUVs(Maze maze)
	{
		for (uint y = 0; y < maze.rooms.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.rooms.GetLength(1); x++)
			{
				Room room = maze.rooms[y, x];

				// 
				uint fixedValue = room.value;
				foreach (Dir dir in Enum.GetValues(typeof(Dir)))
				{
					if ((room.value & Nav.bits[dir]) != 0)
					{
						Point neighbourPos = room.position + new Point(Nav.DX[dir], Nav.DY[dir]);
						Room neighbourRoom = maze.GetRoom(neighbourPos);
						if (neighbourRoom != null)
						{
							if (room.theme != neighbourRoom.theme)
								fixedValue &= ~Nav.bits[dir];
						}
					}
				}

				AutotileFloor(room, fixedValue);
				AutotileCeiling(room, fixedValue);
				AutotileWalls(maze, room, fixedValue);
			}
		}
	}

	private void AutotileFloor(Room room, uint fixedRoomValue)
	{
		Transform floorTransform = room.instance.transform.Find("Floor");

		floorTransform.Find("Mesh").GetComponent<UVRect>().start = Autotile.GetUVOffsetByIndex(Autotile.floorTileStartIndex + Autotile.fourBitTileIndices[fixedRoomValue]);
		floorTransform.rotation = Quaternion.Euler(0.0f, Autotile.tileRotations[fixedRoomValue], 0.0f);
	}

	private void AutotileCeiling(Room room, uint fixedRoomValue)
	{
		Transform ceilingTransform = room.instance.transform.Find("Ceiling");

		ceilingTransform.Find("Mesh").GetComponent<UVRect>().start = Autotile.GetUVOffsetByIndex(Autotile.ceilingTileStartIndex + Autotile.fourBitTileIndices[fixedRoomValue]);
		ceilingTransform.rotation = Quaternion.Euler(0.0f, Autotile.tileRotations[fixedRoomValue], 0.0f);
	}

	private void AutotileWalls(Maze maze, Room room, uint fixedRoomValue)
	{
		// Autotile the wall, using the other rooms around it.
		uint wallValue = 0;

		Transform wallsInstance = room.instance.transform.Find("Walls");

		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			wallValue = 0;

			if ((room.value & Nav.bits[dir]) == 0)
			{
				// Check to the left of the wall direction.
				if (Nav.IsConnected(fixedRoomValue, Nav.left[dir]))
				{
					Point leftPos = room.position + new Point(Nav.DX[Nav.left[dir]], Nav.DY[Nav.left[dir]]);
					if (leftPos.x >= 0 && leftPos.x < maze.rooms.GetLength(1) && leftPos.y >= 0 && leftPos.y < maze.rooms.GetLength(0))
					{
						if (Autotile.IsWallConnected(room.value, maze.rooms[leftPos.y, leftPos.x].value, dir))
							wallValue |= 1;
					}
				}
				// Check to the right of the wall direction.
				if (Nav.IsConnected(fixedRoomValue, Nav.right[dir]))
				{
					Point rightPos = room.position + new Point(Nav.DX[Nav.right[dir]], Nav.DY[Nav.right[dir]]);
					if (rightPos.x >= 0 && rightPos.x < maze.rooms.GetLength(1) && rightPos.y >= 0 && rightPos.y < maze.rooms.GetLength(0))
					{
						if (Autotile.IsWallConnected(room.value, maze.rooms[rightPos.y, rightPos.x].value, dir))
							wallValue |= 2;
					}
				}

				// Set the wall texture UV.
				Transform wallInstance = wallsInstance.Find(Nav.bits[dir].ToString());
				wallInstance.Find("Mesh").GetComponent<UVRect>().start = Autotile.GetUVOffsetByIndex(Autotile.wallTileStartIndex + Autotile.twoBitTileIndices[wallValue]);
			}
		}
	}

    /// <summary>
    /// Prints a 2D grid neatly.
    /// </summary>
    /// <param name="grid">2D grid to print.</param>
	private void PrintGrid(int[,] grid)
	{
		string output = "";
		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
			{
				output += grid[y, x];
				if (x < grid.GetLength(1) - 1)
					output += ", ";
			}
			output += "\n";
		}
		Debug.Log(output);
	}
}
