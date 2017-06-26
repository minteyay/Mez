using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameObject playerPrefab = null;
	private GameObject playerInstance = null;
	private Player player = null;

	public uint mazeWidth = 0;
	public uint mazeHeight = 0;

	public GameObject uiPrefab = null;
	private UI ui = null;

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
		Cursor.visible = false;

#if UNITY_STANDALONE && SCREENSAVER
        // Set the resolution to the highest one available.
		//Resolution[] resolutions = Screen.resolutions;
		//Screen.SetResolution(resolutions[resolutions.GetLength(0) - 1].width, resolutions[resolutions.GetLength(0) - 1].height, true);
#endif
	}

	void Start()
	{
		// Load all tilesets
		themeManager.LoadThemeTilesets("paperhouse", StartLevel);

        // Create the UI overlay.
		GameObject uiInstance = Instantiate(uiPrefab);
		uiInstance.name = "UI";
		ui = uiInstance.GetComponent<UI>();
	}

	void Update()
	{
#if !SCREENSAVER
        // Generate a new maze (only when the player is moving).
		if (Input.GetKeyDown(KeyCode.N) && player.CanMove)
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

        // Generate a new maze.
		maze = mazeGen.GenerateMaze(mazeWidth, mazeHeight);
		mazeGen.TextureMaze(maze, themeManager);

        // Create a new player if one doesn't already exist.
		if (playerInstance == null)
		{
			playerInstance = (GameObject)Instantiate(playerPrefab,
				new Vector3(),
				Quaternion.Euler(maze.startRotation));
			playerInstance.name = "Player";
			player = playerInstance.GetComponent<Player>();
			player.maze = maze;
		}
        // Reposition the player to the maze start if it exists.
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
        // Generate a new maze and fade it in.
		GenerateLevel();
		ui.FadeIn(StartMoving);
	}

	public void StartMoving()
	{
		player.CanMove = true;
	}

	public void ResetLevel()
	{
        // Stop the player and fade the maze out.
		player.CanMove = false;
		ui.FadeOut(StartLevel);
	}
}
