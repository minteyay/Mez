using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab = null;
	private GameObject playerInstance = null;
	private Player player = null;

	public GameObject uiPrefab = null;

	private ThemeManager themeManager = null;

	private MazeGenerator mazeGen = null;
	private Maze maze = null;

    // Manager singleton instance and getter.
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
		themeManager = GetComponent<ThemeManager>();
		mazeGen = GetComponent<MazeGenerator>();

#if UNITY_STANDALONE && SCREENSAVER
        // Set the resolution to the highest one available.
		//Resolution[] resolutions = Screen.resolutions;
		//Screen.SetResolution(resolutions[resolutions.GetLength(0) - 1].width, resolutions[resolutions.GetLength(0) - 1].height, true);
#endif
	}

	void Start()
	{
		// Load the theme.
		themeManager.LoadTheme("paperhouse", GenerateLevel);

        // Create the UI.
		GameObject uiInstance = Instantiate(uiPrefab);
		uiInstance.name = "UI";
	}

	void Update()
	{
#if !SCREENSAVER
        // Generate a new maze.
		if (Input.GetKeyDown(KeyCode.N))
		{
			GenerateLevel();
		}
#endif

#if UNITY_WEBGL
        // Toggle fullscreen.
		if (Input.GetKeyDown(KeyCode.F))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
        // Toggle cursor visibility.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.visible = !Cursor.visible;
		}
#elif !SCREENSAVER
        // Quit the executable.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
#else
        // Quit the screensaver.
		if (Input.anyKeyDown)
		{
			Application.Quit();
		}
#endif
	}

	public void GenerateLevel()
	{
        // Destroy the maze if one exists.
		if (maze)
		{
			Destroy(maze.gameObject);
			Resources.UnloadUnusedAssets();
		}

		MazeRuleset ruleset = themeManager.Rulesets["paperhouse"];

        // Generate a new maze.
		mazeGen.GenerateMaze(ruleset, themeManager, LevelGenerated);
	}

	private void LevelGenerated(Maze maze)
	{
		// Store the generated maze.
		this.maze = maze;

		// Create a new player if one doesn't already exist.
		if (playerInstance == null)
		{
			playerInstance = (GameObject)Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
			playerInstance.name = "Player";
			player = playerInstance.GetComponent<Player>();
			player.outOfBoundsCallback = GenerateLevel;
		}

		player.maze = maze;
		playerInstance.transform.position = maze.RoomToWorldPosition(maze.startPosition) - new Vector3(maze.entranceLength * maze.roomDim.y, 0.0f, 0.0f);
		player.facing = Dir.S;
		player.Reset();

		Dir nextFacing;
		Vector3 nextTarget = Nav.IndexToWorldPos(maze.MoveLeftmost(maze.startPosition, Dir.S, out nextFacing), maze.roomDim);
		player.SetTargets(maze.RoomToWorldPosition(maze.startPosition), Dir.S, nextTarget, nextFacing);
	}
}
