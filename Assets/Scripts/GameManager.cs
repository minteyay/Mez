using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab = null;
	private GameObject playerInstance = null;
	private Player player = null;

	public int mazeWidth = 0;
	public int mazeHeight = 0;

	private MazeGenerator mazeGen = null;
	private Maze maze = null;

	private static GameManager _instance;
	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<GameManager>();

				if (_instance == null)
				{
					GameObject container = new GameObject("GameManager");
					_instance = container.AddComponent<GameManager>();
				}
			}

			return _instance;
		}
	}

	void Awake()
	{
		mazeGen = GetComponent<MazeGenerator>();
	}

	void Start()
	{
		GenerateLevel();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			GenerateLevel();
		}
	}

	public void GenerateLevel()
	{
		if (maze)
			Destroy(maze.gameObject);
		maze = mazeGen.GenerateMaze(mazeWidth, mazeHeight);

		if (playerInstance == null)
		{
			playerInstance = (GameObject)Instantiate(playerPrefab,
				new Vector3(),
				Quaternion.Euler(maze.startRotation));
			playerInstance.name = "Player";
			player = playerInstance.GetComponent<Player>();
			player.maze = maze;
		}
		else
		{
			player.maze = maze;
			playerInstance.transform.position = new Vector3();
			playerInstance.transform.rotation = Quaternion.Euler(maze.startRotation);
			player.Reset();
		}
	}
}
