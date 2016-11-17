using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab = null;
	private GameObject playerInstance = null;
	private Player player = null;

	public int mazeWidth = 0;
	public int mazeHeight = 0;

	public GameObject uiPrefab = null;
	private UI ui = null;

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
		Cursor.visible = false;
	}

	void Start()
	{
		GameObject uiInstance = Instantiate(uiPrefab);
		uiInstance.name = "UI";
		ui = uiInstance.GetComponent<UI>();

		StartLevel();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.N) && player.canMove)
		{
			ResetLevel();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.visible = !Cursor.visible;
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			Screen.fullScreen = !Screen.fullScreen;
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

	public void StartLevel()
	{
		GenerateLevel();
		ui.FadeIn(StartMoving);
	}

	public void StartMoving()
	{
		player.canMove = true;
	}

	public void ResetLevel()
	{
		player.canMove = false;
		ui.FadeOut(StartLevel);
	}
}
