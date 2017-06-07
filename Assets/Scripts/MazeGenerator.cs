using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	public GameObject mazePrefab = null;

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

    // How much to move the walls inwards in rooms.
	public float wallInset = 0.0f;

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
		GameObject mazeInstance = Instantiate(mazePrefab);
		mazeInstance.name = "Maze";
		Maze maze = mazeInstance.GetComponent<Maze>();
		if (maze == null)
		{
			Debug.Log("Maze prefab has no Maze script attached!");
			return null;
		}
        // Create the visual parts for the maze.
		maze.Initialise(width, height, roomDim);
		CreateRooms(grid, maze);

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
				grid[y, x] |= Room.bits[dir];
				grid[ny, nx] |= Room.bits[Nav.opposite[dir]];

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
                // Base room GameObject.
				GameObject roomInstance = new GameObject(grid[y, x].ToString());
				roomInstance.AddComponent<MaterialSetter>();
				roomInstance.transform.position = new Vector3(y * roomDim.y, 0.0f, x * roomDim.x);
				roomInstance.name = grid[y, x].ToString();

                // Create a floor for the room.
				GameObject floor = CreateFloor(grid[y, x]);
				floor.transform.SetParent(roomInstance.transform, false);
				floor.GetComponent<MaterialSetter>().SetMaterial(defaultMaterial);

                // Create walls for the room.
				GameObject walls = CreateWalls(grid[y, x]);
				walls.transform.SetParent(roomInstance.transform, false);
				walls.GetComponent<MaterialSetter>().SetMaterial(defaultMaterial);

                // Create a ceiling for the room.
				GameObject ceiling = CreateCeiling(grid[y, x]);
				ceiling.transform.SetParent(roomInstance.transform, false);
				ceiling.GetComponent<MaterialSetter>().SetMaterial(defaultMaterial);

                // Create the actual room and add it to the Maze.
				Room room = new Room(grid[y, x], new Point((int)x, (int)y), roomInstance);
				maze.AddRoom(room);
			}
		}
	}

    /// <summary>
    /// Creates a floor with the shape and orientation corresponding to the given bitwise room value.
    /// </summary>
    /// <param name="value">Bitwise value of the room.</param>
    /// <returns>Floor with the correct shape and orientation.</returns>
	private GameObject CreateFloor(uint value)
	{
		GameObject floorInstance = (GameObject)Instantiate(floor,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[value], 0.0f));
		floorInstance.name = "Floor";
		floorInstance.GetComponent<UVRect>().start = Autotile.GetUVOffsetByIndex(Autotile.floorTileStartIndex + Autotile.fourBitTileIndices[value]);
		return floorInstance;
	}

    /// <summary>
    /// Creates walls in the directions required by the given bitwise room value.
    /// </summary>
    /// <param name="value">Bitwise value of the room.</param>
    /// <returns>GameObject with walls parented to it.</returns>
	private GameObject CreateWalls(uint value)
	{
        // Walls base GameObject.
		GameObject wallsInstance = new GameObject("Walls");
		wallsInstance.AddComponent<MaterialSetter>();

        // Check all directions to see if there should be a wall facing that way.
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
            // If the room value has this direction bit set, create a wall.
			if ((~value & Room.bits[dir]) != 0)
			{
				GameObject wallInstance = (GameObject)Instantiate(wall, new Vector3(),
						Quaternion.Euler(0.0f, Nav.GetRotation(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-roomDim.y / 2.0f + wallInset, 0.0f, 0.0f);
			}
		}

		return wallsInstance;
	}

	private GameObject CreateCeiling(uint value)
	{
		GameObject ceilingInstance = (GameObject)Instantiate(ceiling,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[value], 0.0f));
		ceilingInstance.name = "Ceiling";
		ceilingInstance.transform.Find("Mesh").GetComponent<UVRect>().start = Autotile.GetUVOffsetByIndex(Autotile.ceilingTileStartIndex + Autotile.fourBitTileIndices[value]);
		return ceilingInstance;
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
