using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	public GameObject mazePrefab = null;

    /// Size of a room in world dimensions.
	public Vector2 roomDim;

    /*
     * Prefabs to use for different shape floors.
     */
	public GameObject uFloor = null;
	public GameObject cornerFloor = null;
	public GameObject straightFloor = null;
	public GameObject tFloor = null;
	public GameObject xFloor = null;

	/// Wall model prefab.
	public GameObject wall = null;
	/// Ceiling model prefab.
	public GameObject ceiling = null;

	/// Default maze material.
	public Material defaultMaterial = null;

    // End point prefab.
	public GameObject endPoint = null;
    // Distance to the end point from the start of the maze.
	private int endPointDist = 0;
    // Index position of the end point.
	private Point endPointCoord = new Point(-1, -1);
    // Y rotation of the end point.
	private float endPointRotation = 0.0f;

    // How much to move the walls inwards in rooms.
	public float wallInset = 0.0f;

	private struct RoomPrefab
	{
		public RoomPrefab(GameObject prefab, Vector3 rotation)
		{
			this.prefab = prefab;
			this.rotation = rotation;
		}
		public GameObject prefab;
		public Vector3 rotation;
	}
    // Room prefabs to use for different shapes of rooms in different orientations.
	private Dictionary<int, RoomPrefab> roomPrefabs = new Dictionary<int, RoomPrefab>();

    // Random number generator to use in generating the maze.
	private System.Random rnd = null;

    // Whether to use the alternative wall prefab or not.
	private bool useAltWall = false;

	void Awake()
	{
		rnd = new System.Random();

        // Add all the room prefabs in their correct shapes and orientations.
        // A bitwise system is used to determine these.
		roomPrefabs.Add(1, new RoomPrefab(uFloor, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(2, new RoomPrefab(uFloor, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(3, new RoomPrefab(cornerFloor, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(4, new RoomPrefab(uFloor, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(5, new RoomPrefab(straightFloor, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(6, new RoomPrefab(cornerFloor, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(7, new RoomPrefab(tFloor, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(8, new RoomPrefab(uFloor, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(9, new RoomPrefab(cornerFloor, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(10, new RoomPrefab(straightFloor, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(11, new RoomPrefab(tFloor, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(12, new RoomPrefab(cornerFloor, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(13, new RoomPrefab(tFloor, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(14, new RoomPrefab(tFloor, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(15, new RoomPrefab(xFloor, new Vector3(0.0f, 0.0f, 0.0f)));
	}

    /// <summary>
    /// Generates a maze with the given dimensions.
    /// </summary>
    /// <returns>A brand-new maze to play with.</returns>
	public Maze GenerateMaze(int width, int height)
	{
		endPointDist = 0;
		endPointCoord = new Point(-1, -1);

		int[,] grid = new int[height, width];
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
	private void CarvePassagesFrom(int x, int y, int[,] grid, int distance)
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
	private void CreateRooms(int[,] grid, Maze maze)
	{
		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
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
				GameObject ceiling = CreateCeiling();
				ceiling.transform.SetParent(roomInstance.transform, false);
				ceiling.transform.position += new Vector3(0.0f, 2.0f, 0.0f);
				ceiling.GetComponent<MaterialSetter>().SetMaterial(defaultMaterial);

                // Create the actual room and add it to the Maze.
				Room room = new Room(grid[y, x], new Point(x, y), roomInstance);
				maze.AddRoom(room);
			}
		}
	}

    /// <summary>
    /// Creates a floor with the shape and orientation corresponding to the given bitwise room value.
    /// </summary>
    /// <param name="value">Bitwise value of the room.</param>
    /// <returns>Floor with the correct shape and orientation.</returns>
	private GameObject CreateFloor(int value)
	{
		GameObject floorInstance = (GameObject)Instantiate(roomPrefabs[value].prefab,
					new Vector3(),
					Quaternion.Euler(roomPrefabs[value].rotation));
		floorInstance.name = "Floor";
		return floorInstance;
	}

    /// <summary>
    /// Creates walls in the directions required by the given bitwise room value.
    /// </summary>
    /// <param name="value">Bitwise value of the room.</param>
    /// <returns>GameObject with walls parented to it.</returns>
	private GameObject CreateWalls(int value)
	{
        // Walls base GameObject.
		GameObject wallsInstance = new GameObject("Walls");
		wallsInstance.AddComponent<MaterialSetter>();

        // Check all directions to see if there should be a wall facing that way.
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
            // If the room value has this direction bit set, create a wall.
			if ((~value & Room.bits[dir]) > 0)
			{
				GameObject wallInstance = (GameObject)Instantiate(wall, new Vector3(),
						Quaternion.Euler(0.0f, Nav.GetRotation(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-roomDim.y / 2.0f + wallInset, 0.0f, 0.0f);
			}
		}

		return wallsInstance;
	}

	private GameObject CreateCeiling()
	{
		GameObject ceilingInstance = (GameObject)Instantiate(ceiling,
					new Vector3(),
					Quaternion.identity);
		ceilingInstance.name = "Ceiling";
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
