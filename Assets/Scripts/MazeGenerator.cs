using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	public GameObject mazePrefab = null;

	public Vector2 roomDim;

	public GameObject uRoom = null;
	public GameObject cornerRoom = null;
	public GameObject straightRoom = null;
	public GameObject tRoom = null;
	public GameObject xRoom = null;

	public GameObject endPoint = null;
	private int endPointDist = 0;
	private int[] endPointCoord = { -1, -1 };

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

		roomPrefabs.Add(1, new RoomPrefab(uRoom, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(2, new RoomPrefab(uRoom, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(3, new RoomPrefab(cornerRoom, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(4, new RoomPrefab(uRoom, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(5, new RoomPrefab(straightRoom, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(6, new RoomPrefab(cornerRoom, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(7, new RoomPrefab(tRoom, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(8, new RoomPrefab(uRoom, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(9, new RoomPrefab(cornerRoom, new Vector3(0.0f, -90.0f, 0.0f)));
		roomPrefabs.Add(10, new RoomPrefab(straightRoom, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(11, new RoomPrefab(tRoom, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(12, new RoomPrefab(cornerRoom, new Vector3(0.0f, 180.0f, 0.0f)));
		roomPrefabs.Add(13, new RoomPrefab(tRoom, new Vector3(0.0f, 90.0f, 0.0f)));
		roomPrefabs.Add(14, new RoomPrefab(tRoom, new Vector3(0.0f, 0.0f, 0.0f)));
		roomPrefabs.Add(15, new RoomPrefab(xRoom, new Vector3(0.0f, 0.0f, 0.0f)));
	}

	public Maze GenerateMaze(int width, int height)
	{
		endPointDist = 0;
		endPointCoord = new int[] { -1, -1 };

		int[,] grid = new int[height, width];
		CarvePassagesFrom(0, 0, grid, 0);
		PrintGrid(grid);

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

		maze.AddEndPoint(endPointCoord[0], endPointCoord[1], endPoint);

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
				grid[y, x] |= Room.bits[dir];
				grid[ny, nx] |= Room.oppositeBits[dir];
				CarvePassagesFrom(nx, ny, grid, distance + 1);
			}
		}
		if (distance > endPointDist)
		{
			endPointCoord[0] = x;
			endPointCoord[1] = y;
			endPointDist = distance;
		}
	}

	private void CreateRooms(int[,] grid, Maze maze)
	{
		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
			{
				GameObject roomInstance = (GameObject)Instantiate(roomPrefabs[grid[y, x]].prefab,
					new Vector3(y * roomDim.y, 0.0f, x * roomDim.x),
					Quaternion.Euler(roomPrefabs[grid[y, x]].rotation));
				roomInstance.name = grid[y, x].ToString();
				Room room = new Room(grid[y, x], roomInstance);
				maze.AddRoom(x, y, room);
			}
		}
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
