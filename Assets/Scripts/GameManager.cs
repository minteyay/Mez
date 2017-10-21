using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab = null;
	private GameObject _playerInstance = null;
	private Player _player = null;

	public GameObject uiPrefab = null;

	private ThemeManager _themeManager = null;

	private MazeGenerator _mazeGen = null;
	private Maze _maze = null;

    // Manager singleton instance and getter.
	private static GameManager _instance;
	public static GameManager instance
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
		_themeManager = GetComponent<ThemeManager>();
		_mazeGen = GetComponent<MazeGenerator>();

#if UNITY_STANDALONE && SCREENSAVER
        // Set the resolution to the highest one available.
		//Resolution[] resolutions = Screen.resolutions;
		//Screen.SetResolution(resolutions[resolutions.GetLength(0) - 1].width, resolutions[resolutions.GetLength(0) - 1].height, true);
#endif
	}

	void Start()
	{
		// Load the theme.
		_themeManager.LoadTheme("dark", GenerateLevel);

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
		if (_maze)
		{
			Destroy(_maze.gameObject);
			Resources.UnloadUnusedAssets();
		}

		MazeRuleset ruleset = _themeManager.rulesets["dark"];

        // Generate a new maze.
		_mazeGen.GenerateMaze(ruleset, _themeManager, LevelGenerated);
	}

	private void LevelGenerated(Maze maze)
	{
		// Store the generated maze.
		this._maze = maze;

		// Create a new player if one doesn't already exist.
		if (_playerInstance == null)
		{
			_playerInstance = (GameObject)Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
			_playerInstance.name = "Player";
			_player = _playerInstance.GetComponent<Player>();
			_player.outOfBoundsCallback = GenerateLevel;
		}

		_player.maze = maze;
		_playerInstance.transform.position = maze.TileToWorldPosition(maze.startPosition) - new Vector3(maze.entranceLength * maze.tileSize.y, 0.0f, 0.0f);
		_player.facing = Dir.S;
		_player.Reset();

		Point nextTarget = maze.MoveForwards(maze.startPosition, Dir.S, Maze.MovementPreference.Leftmost, true);
		Dir nextFacing = Nav.DeltaToFacing(nextTarget - maze.startPosition);
		_player.SetTargets(maze.TileToWorldPosition(maze.startPosition), Dir.S, maze.TileToWorldPosition(nextTarget), nextFacing);
	}
}
