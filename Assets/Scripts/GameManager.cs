using UnityEngine;

public class GameManager : MonoBehaviour
{
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

	public ThemeManager themeManager { get; private set; }

	[SerializeField] private Camera _editorCamera = null;
	[SerializeField] private UI _gui = null;

	[SerializeField] private GameObject _playerPrefab = null;
	private Player _player = null;

	private MazeGenerator _mazeGen = null;
	private Maze _maze = null;

	private bool _paused = false;

	void Awake()
	{
		themeManager = GetComponent<ThemeManager>();
		_mazeGen = GetComponent<MazeGenerator>();

#if UNITY_STANDALONE && SCREENSAVER
        // Set the resolution to the highest one available.
		//Resolution[] resolutions = Screen.resolutions;
		//Screen.SetResolution(resolutions[resolutions.GetLength(0) - 1].width, resolutions[resolutions.GetLength(0) - 1].height, true);
#endif
	}

	void Start()
	{
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SetPause(!_paused);
		}
	}

	public void SetPause(bool pause)
	{
		_paused = pause;
		if (_player != null)
			_player.enabled = !_paused;
		_gui.SetPauseMenuEnabled(_paused);
	}

	public void GenerateMaze(MazeRuleset ruleset)
	{
        // Destroy the maze if one exists.
		if (_maze)
		{
			Destroy(_maze.gameObject);
			Resources.UnloadUnusedAssets();
		}

        // Generate a new maze.
		_mazeGen.GenerateMaze(ruleset, themeManager, (Maze maze) => { _maze = maze; } );
	}

	public void RunMaze()
	{
		_editorCamera.enabled = false;

		if (_player == null)
		{
			GameObject playerInstance = (GameObject)Instantiate(_playerPrefab, new Vector3(), Quaternion.identity);
			playerInstance.name = "Player";
			_player = playerInstance.GetComponent<Player>();
			_player.outOfBoundsCallback = StopRunningMaze;
		}

		_player.maze = _maze;
		_player.transform.position = _maze.TileToWorldPosition(_maze.startPosition) - new Vector3(_maze.entranceLength * _maze.tileSize.y, 0.0f, 0.0f);
		_player.facing = Dir.S;
		_player.Reset();

		Point nextTarget = _maze.MoveForwards(_maze.startPosition, Dir.S, Maze.MovementPreference.Leftmost, true);
		Dir nextFacing = Nav.DeltaToFacing(nextTarget - _maze.startPosition);
		_player.SetTargets(_maze.TileToWorldPosition(_maze.startPosition), Dir.S, _maze.TileToWorldPosition(nextTarget), nextFacing);
	}

	public void StopRunningMaze()
	{
		if (_player != null)
		{
			Destroy(_player.gameObject);
			_player = null;
		}

		_editorCamera.enabled = true;
		_gui.SetEditorGUIEnabled(true);
	}
}
