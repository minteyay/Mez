using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	/* Editor parameters. */

    /// Size of a tile in world dimensions.
	[SerializeField] private Vector2 _tileSize;
	/// Length of the entrance corridor before a maze (in tiles).
	[SerializeField] private uint _entranceLength = 0;

	[SerializeField] private GameObject _floor = null;
	[SerializeField] private GameObject _wall = null;
	[SerializeField] private GameObject _ceiling = null;
	[SerializeField] private GameObject _corridor = null;

	[SerializeField] private Shader _regularShader = null;
	[SerializeField] private Shader _seamlessShader = null;

	/// Should maze generation be stepped through manually?
	[SerializeField] private bool _stepThrough = false;

	/* Variables used during maze generation. */

	public enum GenerationState
	{
		Idle,
		GeneratingGrid,
		RunningSprawlers,
		Finished
	}
	[SerializeField] private GenerationState _state;
	public GenerationState state { get { return _state; } private set { _state = value; } }

	private uint[,] _grid = null;
	private Maze _maze = null;

	private MazeRuleset _ruleset = null;
	private Dictionary<string, RoomStyle> _roomStyles = null;
	
	private uint _currentSprawlerRulesetIndex = 0;
	public RoomRuleset currentSprawlerRuleset { get; private set; }
	private uint _numSprawlersRun = 0;
	private uint _numSprawlersFailed = 0;
	private Sprawler _currentSprawler = null;
	private List<Tile> _currentSprawlerTiles = null;
	private List<Tile> _newSprawlerTiles = null;

	private static uint MaxSprawlerFailures = 8;

	public List<string> messageLog { get; private set; }

	private ThemeManager _themeManager = null;
	private const string FloorSuffix = "_floor";
	private const string CeilingSuffix = "_ceiling";
	private Dictionary<string, Material> _tilesetMaterials = null;

	public delegate void OnComplete(Maze maze);
	private OnComplete _onComplete = null;

	/// Distance to the end point from the start of the maze.
	private uint _endPointDist = 0;
    /// Index position of the end point.
	private Point _endPointCoord = new Point(-1, -1);
	/// Direction of the end point.
	private Dir _endPointDir = Dir.N;

    /// <summary>
    /// Asynchronously generates a maze.
	/// Returns the finished maze as a parameter to onComplete.
    /// </summary>
	public void GenerateMaze(MazeRuleset ruleset, ThemeManager themeManager, OnComplete onComplete)
	{
		_ruleset = ruleset;
		_themeManager = themeManager;
		_onComplete = onComplete;

		_roomStyles = new Dictionary<string, RoomStyle>();
		foreach (RoomStyle tileset in ruleset.roomStyles)
			_roomStyles.Add(tileset.name, tileset);

		messageLog = new List<string>();
		_newSprawlerTiles = new List<Tile>();
		_tilesetMaterials = new Dictionary<string, Material>();

		_endPointDist = 0;
		_endPointCoord = new Point(-1, -1);

		_grid = new uint[ruleset.size.y, ruleset.size.x];

		int startX = Random.instance.Next(ruleset.size.x);
		CarvePassagesFrom(startX, 0, _grid, 0);

		_grid[0, startX] |= Nav.bits[Dir.N];
		_grid[_endPointCoord.y, _endPointCoord.x] |= Nav.bits[_endPointDir];

        // Base GameObject for the maze.
		GameObject mazeInstance = new GameObject();
		mazeInstance.name = "Maze";
		_maze = mazeInstance.AddComponent<Maze>();
		if (_maze == null)
		{
			Debug.LogError("Maze prefab has no Maze script attached!");
			return;
		}
		_maze.Initialise((uint)ruleset.size.x, (uint)ruleset.size.y, _tileSize);
		_maze.defaultTheme = ruleset.defaultTileset;
		_maze.startPosition = new Point(startX, 0);
		_maze.entranceLength = _entranceLength;

		CreateCorridors(mazeInstance);
		CreateTiles(_grid, _maze);
		CreateTileGeometry(_maze);
		TextureMaze();
		UpdateMazeUVs();

		if (ruleset.rooms.Length > 0)
			state = GenerationState.RunningSprawlers;
		else
			state = GenerationState.Finished;
		
		if (!_stepThrough)
			while (Step()) {}
	}

	private void FinishMaze()
	{
		if (!_stepThrough)
		{
			TextureMaze();
			UpdateMazeUVs();
		}

		if (_onComplete != null)
			_onComplete.Invoke(_maze);
		state = GenerationState.Idle;

		// Remove all of the unnecessary objects.
		_grid = null;
		_maze = null;
		_ruleset = null;
		_roomStyles = null;
		currentSprawlerRuleset = null;
		_currentSprawlerTiles = null;
		_newSprawlerTiles = null;
		messageLog = null;
		_tilesetMaterials = null;
	}

	public bool Step()
	{
		messageLog.Clear();

		switch (state)
		{
			// Running the Sprawlers in the current ruleset.
			case GenerationState.RunningSprawlers:
				currentSprawlerRuleset = _ruleset.rooms[_currentSprawlerRulesetIndex];
				
				// Create a new Sprawler if we don't have one to Step.
				if (_currentSprawler == null)
				{
					Tile startTile = null;
					switch (currentSprawlerRuleset.start)
					{
						case RoomRuleset.Start.Start:
							startTile = _maze.GetTile(_maze.startPosition);
							break;
						case RoomRuleset.Start.Random:
							Point randomPoint = new Point(Random.instance.Next(_maze.size.x), Random.instance.Next(_maze.size.y));
							startTile = _maze.GetTile(randomPoint);
							break;
						case RoomRuleset.Start.End:
							startTile = _maze.GetTile(_endPointCoord);
							break;
					}
					if (startTile == null)
						Debug.LogError("Sprawler starting room went out of bounds!");

					_currentSprawlerTiles = new List<Tile>();
					_currentSprawler = new Sprawler(_maze, startTile.position, currentSprawlerRuleset.size,
						(Tile tile) => { _newSprawlerTiles.Add(tile); } );
					messageLog.Add("Added sprawler at " + startTile.position.ToString());
				}

				_newSprawlerTiles.Clear();

				// Step the current Sprawler.
				bool sprawlerFinished = !_currentSprawler.Step();

				// Copy the newly added Tiles to the current Sprawler's Tiles.
				foreach (Tile tile in _newSprawlerTiles)
					_currentSprawlerTiles.Add(tile);

				if (sprawlerFinished)
				{
					if (!_currentSprawler.success)
					{
						messageLog.Add("Sprawler failed");
						_numSprawlersFailed++;
						if (_numSprawlersFailed >= MaxSprawlerFailures)
						{
							Debug.LogWarning("Maximum amount of Sprawlers failed, moving to the next ruleset.");
							NextSprawlerRuleset();
						}
					}
					else
					{
						messageLog.Add("Sprawler finished successfully");

						// Apply the new theme to the Sprawler's Tiles.
						foreach (Tile tile in _currentSprawlerTiles)
							tile.theme = _roomStyles[_ruleset.rooms[_currentSprawlerRulesetIndex].style].tileset;
						
						if (_stepThrough)
						{
							TextureMaze();
							UpdateMazeUVs();
						}

						// Move to the next Sprawler.
						_numSprawlersRun++;
						if (_numSprawlersRun >= currentSprawlerRuleset.count)
						{
							// If we've run all the Sprawlers in the current SprawlerRuleset, move to the next one.
							NextSprawlerRuleset();
						}
					}
					_currentSprawler = null;
				}
				break;

			case GenerationState.Finished:
				FinishMaze();
				return false;
		}
		return true;
	}

#if DEBUG
	private void OnDrawGizmos()
	{
		if (_newSprawlerTiles != null)
		{
			foreach (Tile tile in _newSprawlerTiles)
			{
				Gizmos.color = new Color(0.5f, 0.9f, 0.5f, 0.5f);
				Gizmos.DrawCube(Nav.TileToWorldPos(tile.position, _maze.tileSize) + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(_maze.tileSize.x, 2.0f, _maze.tileSize.y));
			}
		}
		if (_currentSprawler != null)
		{
			foreach (Crawler c in _currentSprawler.crawlers)
			{
				Vector3 crawlerPosition = Nav.TileToWorldPos(c.position, _maze.tileSize) + new Vector3(0.0f, 1.0f, 0.0f);

				Gizmos.color = Color.red;
				Gizmos.DrawSphere(crawlerPosition, 0.2f);

				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(crawlerPosition, crawlerPosition + new Vector3(Nav.DY[c.nextFacing], 0.0f, Nav.DX[c.nextFacing]));
			}
		}
	}
#endif

	private void NextSprawlerRuleset()
	{
		_numSprawlersRun = 0;
		_numSprawlersFailed = 0;
		_currentSprawlerRulesetIndex++;
		if (_currentSprawlerRulesetIndex >= _ruleset.rooms.GetLength(0))
		{
			// If we've run all the SprawlerRulesets in the MazeRuleset, move to the next state.
			_currentSprawlerRulesetIndex = 0;
			state = GenerationState.Finished;
		}
	}

	private void TextureMaze()
	{
		for (int y = 0; y < _maze.size.y; y++)
		{
			for (int x = 0; x < _maze.size.x; x++)
			{
				TextureTile(_maze.GetTile(x, y));
			}
		}
	}

	private void TextureTile(Tile tile)
	{
		// Create the material(s) for the tile if they haven't been created yet.
		if (!_tilesetMaterials.ContainsKey(tile.theme))
		{
			Material regularMaterial = new Material(_regularShader);
			Material floorMaterial = regularMaterial;
			Material ceilingMaterial = regularMaterial;

			// Use the tile's tileset if it's loaded.
			if (_themeManager.Textures.ContainsKey(tile.theme))
			{
				Texture2D tileset = _themeManager.Textures[tile.theme];
				regularMaterial.mainTexture = tileset;

				// Create a seamless texture for the floor if one exists.
				if (_themeManager.Textures.ContainsKey(tile.theme + FloorSuffix))
				{
					floorMaterial = new Material(_seamlessShader);
					floorMaterial.mainTexture = tileset;
					floorMaterial.SetTexture("_SeamlessTex", _themeManager.Textures[tile.theme + FloorSuffix]);
					floorMaterial.SetTextureScale("_SeamlessTex", new Vector2(1.0f / _tileSize.x, 1.0f / _tileSize.y));
				}

				// Create a seamless texture for the ceiling if one exists.
				if (_themeManager.Textures.ContainsKey(tile.theme + CeilingSuffix))
				{
					ceilingMaterial = new Material(_seamlessShader);
					ceilingMaterial.mainTexture = tileset;
					ceilingMaterial.SetTexture("_SeamlessTex", _themeManager.Textures[tile.theme + CeilingSuffix]);
					ceilingMaterial.SetTextureScale("_SeamlessTex", new Vector2(1.0f / _tileSize.x, 1.0f / _tileSize.y));
				}
			}
			// If the tile's tileset isn't loaded, try using the default one.
			else if (_themeManager.Textures.ContainsKey("default"))
			{
				regularMaterial.mainTexture = _themeManager.Textures["default"];
				Debug.LogWarning("Tried using tileset called \"" + tile.theme + "\" but it isn't loaded, using the default tileset.", tile.instance);
			}
			// The default tileset wasn't loaded either.
			else
			{
				Debug.LogWarning("Tried using the default tileset since a tileset named \"" + tile.theme + "\" isn't loaded, but the default one isn't loaded either.", tile.instance);
			}

			_tilesetMaterials.Add(tile.theme, regularMaterial);
			_tilesetMaterials.Add(tile.theme + FloorSuffix, floorMaterial);
			_tilesetMaterials.Add(tile.theme + CeilingSuffix, ceilingMaterial);
		}

		tile.instance.transform.Find("Walls").GetComponent<MaterialSetter>().SetMaterial(_tilesetMaterials[tile.theme]);
		tile.instance.transform.Find("Floor").GetComponent<MaterialSetter>().SetMaterial(_tilesetMaterials[tile.theme + FloorSuffix]);
		tile.instance.transform.Find("Ceiling").GetComponent<MaterialSetter>().SetMaterial(_tilesetMaterials[tile.theme + CeilingSuffix]);
	}

    /// <summary>
    /// Generates a maze into a 2D array.
    /// Calls itself recursively until the maze is complete.
    /// Uses bitwise integers to denote directions a tile is connected in (these can be found in Tile).
    /// </summary>
    /// <param name="x">Index x position to continue generating the maze from.</param>
    /// <param name="y">Index y position to continue generating the maze from.</param>
    /// <param name="grid">2D array to generate the maze into.</param>
    /// <param name="distance">Distance from the beginning of the maze (in tiles).</param>
	private void CarvePassagesFrom(int x, int y, uint[,] grid, uint distance)
	{
        // Try moving in directions in a random order.
		List<Dir> directions = new List<Dir> { Dir.N, Dir.S, Dir.E, Dir.W };
		Utils.Shuffle(Random.instance, directions);

		foreach (Dir dir in directions)
		{
            // Calculate new index position in the direction to try to move in.
			int nx = x + Nav.DX[dir];
			int ny = y + Nav.DY[dir];

            // Check that the new position is within the maze dimensions.
			if ((ny >= 0 && ny < grid.GetLength(0)) && (nx >= 0 && nx < grid.GetLength(1)) && (grid[ny, nx] == 0))
			{
                // Set the connection bits in this new tile and the tile we came from.
				grid[y, x] |= Nav.bits[dir];
				grid[ny, nx] |= Nav.bits[Nav.opposite[dir]];

                // Continue generating the maze.
				CarvePassagesFrom(nx, ny, grid, distance + 1);
			}
		}

		if (distance > _endPointDist)
		{
			List<Dir> possibleEndPointDirs = new List<Dir>();
			if (x == 0)							possibleEndPointDirs.Add(Dir.W);
			if (y == 0)							possibleEndPointDirs.Add(Dir.N);
			if (x == (grid.GetLength(0) - 1))	possibleEndPointDirs.Add(Dir.E);
			if (y == (grid.GetLength(1) - 1))	possibleEndPointDirs.Add(Dir.S);

			if (possibleEndPointDirs.Count > 0)
			{
				_endPointCoord.Set(x, y);
				_endPointDist = distance;

				Utils.Shuffle(Random.instance, possibleEndPointDirs);
				_endPointDir = possibleEndPointDirs[0];
			}
		}
	}

    /// <summary>
    /// Creates tiles into a Maze according to the bitwise data in a given 2D array.
    /// </summary>
    /// <param name="grid">2D grid of bitwise tile values.</param>
    /// <param name="maze">Maze to add the tiles into.</param>
	private void CreateTiles(uint[,] grid, Maze maze)
	{
		for (uint y = 0; y < grid.GetLength(0); y++)
		{
			for (uint x = 0; x < grid.GetLength(1); x++)
			{
                // Create the tile and add it to the Maze.
				Tile tile = new Tile(grid[y, x], new Point((int)x, (int)y));
				tile.theme = _ruleset.defaultTileset;
				maze.AddTile(tile);
			}
		}
	}

	private void CreateCorridors(GameObject mazeInstance)
	{
		GameObject entrance = Instantiate(_corridor, _maze.TileToWorldPosition(_maze.startPosition) - new Vector3(_maze.tileSize.y / 2.0f, 0.0f, 0.0f), Quaternion.identity, mazeInstance.transform);
		entrance.transform.localScale = new Vector3(_entranceLength, 1.0f, 1.0f);
		entrance.name = "Entrance";

		GameObject exit = Instantiate(_corridor,
			_maze.TileToWorldPosition(_endPointCoord) + new Vector3(Nav.DY[_endPointDir] * (_maze.tileSize.y / 2.0f), 0.0f, Nav.DX[_endPointDir] * (_maze.tileSize.x / 2.0f)),
			Quaternion.Euler(0.0f, Nav.FacingToAngle(_endPointDir), 0.0f), mazeInstance.transform);
		exit.transform.localScale = new Vector3(_entranceLength, 1.0f, 1.0f);
		exit.name = "Exit";
	}

	/// <summary>
	/// Creates the tile geometries for all the tiles in a maze.
	/// </summary>
	/// <param name="maze">Maze to generate tile geometries for.</param>
	private void CreateTileGeometry(Maze maze)
	{
		for (int y = 0; y < maze.size.y; y++)
		{
			for (int x = 0; x < maze.size.x; x++)
			{
				maze.GetTile(x, y).instance.AddComponent<MaterialSetter>();
				maze.GetTile(x, y).instance.transform.position = new Vector3(y * _tileSize.y, 0.0f, x * _tileSize.x);

				CreateFloor(maze.GetTile(x, y));
				CreateCeiling(maze.GetTile(x, y));
				CreateWalls(maze.GetTile(x, y));
			}
		}
	}

	/// <summary>
	/// Creates the floor instance for a Tile and parents it.
	/// </summary>
	/// <param name="tile"></param>
	private void CreateFloor(Tile tile)
	{
		GameObject floorInstance = (GameObject)Instantiate(_floor,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[tile.value], 0.0f));
		floorInstance.name = "Floor";
		floorInstance.transform.SetParent(tile.instance.transform, false);
	}

	/// <summary>
	/// Creates the ceiling instance for a Tile and parents it.
	/// </summary>
	/// <param name="tile"></param>
	private void CreateCeiling(Tile tile)
	{
		GameObject ceilingInstance = (GameObject)Instantiate(_ceiling,
					new Vector3(),
					Quaternion.Euler(0.0f, Autotile.tileRotations[tile.value], 0.0f));
		ceilingInstance.name = "Ceiling";
		ceilingInstance.transform.SetParent(tile.instance.transform, false);
	}

	/// <summary>
	/// Creates wall instances for a Tile and parents them.
	/// </summary>
	/// <param name="tile"></param>
	private void CreateWalls(Tile tile)
	{
        // Walls base GameObject.
		GameObject wallsInstance = new GameObject("Walls");
		wallsInstance.AddComponent<MaterialSetter>();
		wallsInstance.transform.SetParent(tile.instance.transform, false);

        // Check all directions to see if there should be a wall facing that way.
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
            // If the tile isn't connected in this direction, create a wall.
			if ((tile.value & Nav.bits[dir]) == 0)
			{
				GameObject wallInstance = (GameObject)Instantiate(_wall, new Vector3(),
						Quaternion.Euler(0.0f, Nav.FacingToAngle(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-_tileSize.y / 2.0f, 0.0f, 0.0f);
				wallInstance.name = Nav.bits[dir].ToString();
			}
		}
	}

	private void UpdateTileUV(Tile tile)
	{
		uint fixedValue = tile.value;
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if ((tile.value & Nav.bits[dir]) != 0)
			{
				Point neighbourPos = tile.position + new Point(Nav.DX[dir], Nav.DY[dir]);
				Tile neighbourTile = _maze.GetTile(neighbourPos);
				if (neighbourTile == null || tile.theme != neighbourTile.theme)
					fixedValue &= ~Nav.bits[dir];
			}
		}

		AutotileFloor(tile, fixedValue);
		AutotileCeiling(tile, fixedValue);
		AutotileWalls(tile, fixedValue);
	}

	/// <summary>
	/// Updates the UV coordinates of all Tile meshes in a Maze by autotiling them.
	/// </summary>
	private void UpdateMazeUVs()
	{
		for (int y = 0; y < _maze.size.y; y++)
		{
			for (int x = 0; x < _maze.size.x; x++)
			{
				UpdateTileUV(_maze.GetTile(x, y));
			}
		}
	}

	private void AutotileFloor(Tile tile, uint fixedTileValue)
	{
		Transform floorTransform = tile.instance.transform.Find("Floor");

		floorTransform.Find("Mesh").GetComponent<UVRect>().offset = Autotile.GetUVOffsetByIndex(Autotile.floorTileStartIndex + Autotile.fourBitTileIndices[fixedTileValue]);
		floorTransform.rotation = Quaternion.Euler(0.0f, Autotile.tileRotations[fixedTileValue], 0.0f);
	}

	private void AutotileCeiling(Tile tile, uint fixedTileValue)
	{
		Transform ceilingTransform = tile.instance.transform.Find("Ceiling");

		ceilingTransform.Find("Mesh").GetComponent<UVRect>().offset = Autotile.GetUVOffsetByIndex(Autotile.ceilingTileStartIndex + Autotile.fourBitTileIndices[fixedTileValue]);
		ceilingTransform.rotation = Quaternion.Euler(0.0f, Autotile.tileRotations[fixedTileValue], 0.0f);
	}

	private void AutotileWalls(Tile tile, uint fixedTileValue)
	{
		// TODO: Refactor to use utils instead of reimplementing stuff.

		// Autotile the wall, using the other tiles around it.
		uint wallValue = 0;

		Transform wallsInstance = tile.instance.transform.Find("Walls");

		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			wallValue = 0;

			if ((tile.value & Nav.bits[dir]) == 0)
			{
				// Check to the left of the wall direction.
				if (Nav.IsConnected(fixedTileValue, Nav.left[dir]))
				{
					Point leftPos = tile.position + new Point(Nav.DX[Nav.left[dir]], Nav.DY[Nav.left[dir]]);
					if (leftPos.x >= 0 && leftPos.x < _maze.size.x && leftPos.y >= 0 && leftPos.y < _maze.size.y)
					{
						if (Autotile.IsWallConnected(tile.value, _maze.GetTile(leftPos).value, dir))
							wallValue |= 1;
					}
				}
				// Check to the right of the wall direction.
				if (Nav.IsConnected(fixedTileValue, Nav.right[dir]))
				{
					Point rightPos = tile.position + new Point(Nav.DX[Nav.right[dir]], Nav.DY[Nav.right[dir]]);
					if (rightPos.x >= 0 && rightPos.x < _maze.size.x && rightPos.y >= 0 && rightPos.y < _maze.size.y)
					{
						if (Autotile.IsWallConnected(tile.value, _maze.GetTile(rightPos).value, dir))
							wallValue |= 2;
					}
				}

				// Set the wall texture UV.
				Transform wallInstance = wallsInstance.Find(Nav.bits[dir].ToString());
				wallInstance.Find("Mesh").GetComponent<UVRect>().offset = Autotile.GetUVOffsetByIndex(Autotile.wallTileStartIndex + Autotile.twoBitTileIndices[wallValue]);
			}
		}
	}
}
