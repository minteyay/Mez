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

    // End point prefab.
	public GameObject endPoint = null;
    // Distance to the end point from the start of the maze.
	private uint endPointDist = 0;
    // Index position of the end point.
	private Point endPointCoord = new Point(-1, -1);
    // Y rotation of the end point.
	private float endPointRotation = 0.0f;

    /// <summary>
    /// Generates a maze with the given ruleset.
    /// </summary>
    /// <returns>A brand-new maze to play with.</returns>
	public Maze GenerateMaze(MazeRuleset ruleset)
	{
		endPointDist = 0;
		endPointCoord = new Point(-1, -1);

		uint[,] grid = new uint[ruleset.size.y, ruleset.size.x];
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
		maze.Initialise((uint)ruleset.size.x, (uint)ruleset.size.y, roomDim);

		CreateRooms(grid, maze, ruleset.tileset);
		CreateRoomGeometry(maze);

		RunCrawlers(maze, ruleset.crawlers);

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
	/// Textures the room instances in a Maze by pulling tilesets from the supplied ThemeManager.
	/// If a Room requires a tileset that's not found in the ThemeManager, the default material in the MazeGenerator is used.
	/// </summary>
	/// <param name="maze"></param>
	/// <param name="themeManager"></param>
	public void TextureMaze(Maze maze, ThemeManager themeManager)
	{
		for (uint y = 0; y < maze.rooms.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.rooms.GetLength(1); x++)
			{
				MaterialSetter roomMaterialSetter = maze.rooms[y, x].instance.GetComponent<MaterialSetter>();
				// If the Room's tileset exists in the ThemeManager, apply it to the room instance.
				if (themeManager.Tilesets.ContainsKey(maze.rooms[y, x].theme))
				{
					roomMaterialSetter.SetMaterial(themeManager.Tilesets[maze.rooms[y, x].theme]);
				}
				// Try applying the default tileset if the Room's one isn't loaded.
				else if (themeManager.Tilesets.ContainsKey("default"))
				{
					roomMaterialSetter.SetMaterial(themeManager.Tilesets["default"]);
					Debug.LogWarning("Tileset named \"" + maze.rooms[y, x].theme + "\" not found in supplied ThemeManager, using default material.",
						maze.rooms[y, x].instance);
				}
				// Even the default tileset wasn't found, so leave the Room untextured.
				else
				{
					Debug.LogWarning("Tried using \"default\" tileset since a tileset named \"" + maze.rooms[y, x].theme + "\" wasn't found, but the default one wasn't found either.",
						maze.rooms[y, x].instance);
				}
			}
		}
	}

    /// <summary>
    /// Generates a maze into a 2D array.
    /// Calls itself recursively until the maze is complete.
    /// Uses bitwise integers to denote directions a room is connected in (these can be found in Room).
    /// </summary>
    /// <param name="x">Index x position to continue generating the maze from.</param>
    /// <param name="y">Index y position to continue generating the maze from.</param>
    /// <param name="grid">2D array to generate the maze into.</param>
    /// <param name="distance">Distance from the beginning of the maze (in rooms).</param>
	private void CarvePassagesFrom(int x, int y, uint[,] grid, uint distance)
	{
        // Try moving in directions in a random order.
		List<Dir> directions = new List<Dir> { Dir.N, Dir.S, Dir.E, Dir.W };
		Utils.Shuffle(Random.instance, directions);

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
					endPointRotation = Nav.FacingToAngle(dir);
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
	private void CreateRooms(uint[,] grid, Maze maze, string defaultTheme = "default")
	{
		for (uint y = 0; y < grid.GetLength(0); y++)
		{
			for (uint x = 0; x < grid.GetLength(1); x++)
			{
                // Create the room and add it to the Maze.
				Room room = new Room(grid[y, x], new Point((int)x, (int)y));
				room.theme = defaultTheme;
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
						Quaternion.Euler(0.0f, Nav.FacingToAngle(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-roomDim.y / 2.0f, 0.0f, 0.0f);
				wallInstance.name = Nav.bits[dir].ToString();
			}
		}
	}

	private void RunCrawlers(Maze maze, List<CrawlerRuleset> crawlers)
	{
		foreach (CrawlerRuleset crawlerRuleset in crawlers)
		{
			for (uint i = 0; i < crawlerRuleset.count; i++)
			{
				Room startRoom = null;
				switch (crawlerRuleset.start)
				{
					case CrawlerRuleset.CrawlerStart.Start:
						startRoom = maze.rooms[0, 0];
						break;
					case CrawlerRuleset.CrawlerStart.Random:
						Point randomPoint = new Point(Random.instance.Next(maze.size.x), Random.instance.Next(maze.size.y));
						startRoom = maze.rooms[randomPoint.y, randomPoint.x];
						break;
					case CrawlerRuleset.CrawlerStart.End:
						Point endPoint = Nav.WorldToIndexPos(maze.endPoint.transform.position, maze.roomDim);
						startRoom = maze.rooms[endPoint.y, endPoint.x];
						break;
				}

				List<Dir> possibleDirs = new List<Dir>();
				foreach (Dir dir in Enum.GetValues(typeof(Dir)))
				{
					if (Nav.IsConnected(startRoom.value, dir))
						possibleDirs.Add(dir);
				}

				Crawler.Crawl(maze, startRoom.position, possibleDirs[rnd.Next(possibleDirs.Count)], crawlerRuleset.size,
					(Room room) => { room.theme = crawlerRuleset.tileset; } );
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
