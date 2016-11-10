using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	public GameObject mazePrefab = null;

	public Vector2 roomDim;

	public GameObject uFloor = null;
	public GameObject cornerFloor = null;
	public GameObject straightFloor = null;
	public GameObject tFloor = null;
	public GameObject xFloor = null;
	public GameObject wall = null;
	public GameObject ceiling = null;

	public GameObject lamp = null;
	private List<Point> lampPositions;
	public int lampInterval = 5;
	private int currentLampDistance = 0;

	public GameObject endPoint = null;
	private int endPointDist = 0;
	private Point endPointCoord = new Point(-1, -1);
	private float endPointRotation = 0.0f;

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
	private Dictionary<int, RoomPrefab> roomPrefabs = new Dictionary<int, RoomPrefab>();

	private System.Random rnd = null;

	void Awake()
	{
		rnd = new System.Random();

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

	public Maze GenerateMaze(int width, int height)
	{
		endPointDist = 0;
		endPointCoord = new Point(-1, -1);

		lampPositions = new List<Point>();

		int[,] grid = new int[height, width];
		CarvePassagesFrom(0, 0, grid, 0);
		//PrintGrid(grid);

		GameObject mazeInstance = Instantiate(mazePrefab);
		mazeInstance.name = "Maze";
		Maze maze = mazeInstance.GetComponent<Maze>();
		if (maze == null)
		{
			Debug.Log("Maze prefab has no Maze script attached!");
			return null;
		}
		maze.Initialise(width, height, roomDim);
		CreateRooms(grid, maze);
		CreateLamps(maze);

		maze.AddEndPoint(endPointCoord, endPoint, Quaternion.Euler(0.0f, endPointRotation, 0.0f));

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

	private void CarvePassagesFrom(int x, int y, int[,] grid, int distance)
	{
		List<Dir> directions = new List<Dir> { Dir.N, Dir.S, Dir.E, Dir.W };
		Utils.Shuffle(rnd, directions);

		foreach (Dir dir in directions)
		{
			int nx = x + Nav.DX[dir];
			int ny = y + Nav.DY[dir];

			if ((ny >= 0 && ny < grid.GetLength(0)) && (nx >= 0 && nx < grid.GetLength(1)) && (grid[ny, nx] == 0))
			{
				currentLampDistance++;
				if (currentLampDistance >= lampInterval)
				{
					currentLampDistance = 0;
					lampPositions.Add(new Point(nx, ny));
				}

				grid[y, x] |= Room.bits[dir];
				grid[ny, nx] |= Room.oppositeBits[dir];
				CarvePassagesFrom(nx, ny, grid, distance + 1);
			}
		}

		currentLampDistance = 0;
		if (distance > endPointDist)
		{
			endPointCoord.Set(x, y);
			endPointDist = distance;

			lampPositions.Remove(new Point(x, y));

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

	private void CreateRooms(int[,] grid, Maze maze)
	{
		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
			{
				GameObject roomInstance = new GameObject(grid[y, x].ToString());
				roomInstance.transform.position = new Vector3(y * roomDim.y, 0.0f, x * roomDim.x);
				roomInstance.name = grid[y, x].ToString();

				GameObject floor = CreateFloor(grid[y, x]);
				floor.transform.SetParent(roomInstance.transform, false);

				GameObject walls = CreateWalls(grid[y, x]);
				walls.transform.SetParent(roomInstance.transform, false);

				GameObject ceiling = CreateCeiling();
				ceiling.transform.SetParent(roomInstance.transform, false);
				ceiling.transform.position += new Vector3(0.0f, 2.0f, 0.0f);

				Room room = new Room(grid[y, x], roomInstance);
				maze.AddRoom(new Point(x, y), room);
			}
		}
	}

	private void CreateLamps(Maze maze)
	{
		foreach (Point lampPos in lampPositions)
		{
			GameObject lampInstance = (GameObject)Instantiate(lamp, new Vector3(), Quaternion.identity);
			lampInstance.name = "Lamp";
			maze.AddItem(lampPos, lampInstance);
		}
	}

	private GameObject CreateFloor(int value)
	{
		GameObject floorInstance = (GameObject)Instantiate(roomPrefabs[value].prefab,
					new Vector3(),
					Quaternion.Euler(roomPrefabs[value].rotation));
		floorInstance.name = "Floor";
		return floorInstance;
	}

	private GameObject CreateWalls(int value)
	{
		GameObject wallsInstance = new GameObject("Walls");

		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if ((~value & Room.bits[dir]) > 0)
			{
				GameObject wallInstance = (GameObject)Instantiate(wall,
						new Vector3(),
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
