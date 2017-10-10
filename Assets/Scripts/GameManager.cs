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
		themeManager.LoadThemeRuleset("paperhouse", null);

		// Load all tilesets
		themeManager.LoadThemeTilesets("paperhouse", GenerateLevel);

        // Create the UI.
		GameObject uiInstance = Instantiate(uiPrefab);
		uiInstance.name = "UI";
	}

	void Update()
	{
#if !SCREENSAVER
        // Generate a new maze (only when the player is moving).
		if (Input.GetKeyDown(KeyCode.N) && player.canMove)
		{
			ResetLevel();
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
			playerInstance = (GameObject)Instantiate(playerPrefab, new Vector3(), Quaternion.Euler(maze.startRotation));
			playerInstance.name = "Player";
			player = playerInstance.GetComponent<Player>();
			player.maze = maze;
			player.facing = Nav.AngleToFacing(maze.startRotation.y);
		}
        // Reposition the player to the maze start if it exists.
        else
        {
			player.maze = maze;
			player.facing = Nav.AngleToFacing(maze.startRotation.y);
			playerInstance.transform.position = new Vector3();
			player.Reset();
		}

		player.canMove = true;
	}

	public void ResetLevel()
	{
        // Stop the player and generate a new maze.
		player.canMove = false;
		GenerateLevel();
	}
}
