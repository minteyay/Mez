using UnityEngine;
using System;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
	/* Editor parameters. */

    /// Size of a tile in world dimensions.
	[SerializeField] private Vector2 tileDim;
	/// Length of the entrance corridor before a maze (in tiles).
	[SerializeField] private uint entranceLength = 0;

    /// Floor model prefab.
	[SerializeField] private GameObject floor = null;
	/// Wall model prefab.
	[SerializeField] private GameObject wall = null;
	/// Ceiling model prefab.
	[SerializeField] private GameObject ceiling = null;
	/// Corridor prefab.
	[SerializeField] private GameObject corridor = null;

	[SerializeField] private Shader regularShader = null;
	[SerializeField] private Shader seamlessShader = null;

	/// Should maze generation be stepped through manually?
	[SerializeField] private bool stepThrough = false;

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

	private uint[,] grid = null;
	private Maze maze = null;

	private MazeRuleset ruleset = null;
	private Dictionary<string, RoomStyle> roomStyles = null;
	
	private uint currentSprawlerRulesetIndex = 0;
	public RoomRuleset currentSprawlerRuleset { get; private set; }
	private uint numSprawlersRun = 0;
	private uint numSprawlersFailed = 0;
	private Sprawler currentSprawler = null;
	private List<Tile> currentSprawlerTiles = null;
	private List<Tile> newSprawlerTiles = null;

	private static uint MaxSprawlerFailures = 8;

	public List<string> messageLog { get; private set; }

	private ThemeManager themeManager = null;
	private const string floorSuffix = "_floor";
	private const string ceilingSuffix = "_ceiling";
	private Dictionary<string, Material> tilesetMaterials = null;

	public delegate void OnComplete(Maze maze);
	private OnComplete onComplete = null;

	/// Distance to the end point from the start of the maze.
	private uint endPointDist = 0;
    /// Index position of the end point.
	private Point endPointCoord = new Point(-1, -1);
	/// Direction of the end point.
	private Dir endPointDir = Dir.N;

    /// <summary>
    /// Generates a maze with the given ruleset.
    /// </summary>
    /// <returns>A brand-new maze to play with.</returns>
	public void GenerateMaze(MazeRuleset ruleset, ThemeManager themeManager, OnComplete onComplete)
	{
		this.ruleset = ruleset;
		this.themeManager = themeManager;
		this.onComplete = onComplete;

		roomStyles = new Dictionary<string, RoomStyle>();
		foreach (RoomStyle tileset in ruleset.roomStyles)
			roomStyles.Add(tileset.name, tileset);

		messageLog = new List<string>();
		newSprawlerTiles = new List<Tile>();
		tilesetMaterials = new Dictionary<string, Material>();

		endPointDist = 0;
		endPointCoord = new Point(-1, -1);

		grid = new uint[ruleset.size.y, ruleset.size.x];

		int startX = Random.instance.Next(ruleset.size.x);
		CarvePassagesFrom(startX, 0, grid, 0);

		grid[0, startX] |= Nav.bits[Dir.N];
		grid[endPointCoord.y, endPointCoord.x] |= Nav.bits[endPointDir];

        // Base GameObject for the maze.
		GameObject mazeInstance = new GameObject();
		mazeInstance.name = "Maze";
		maze = mazeInstance.AddComponent<Maze>();
		if (maze == null)
		{
			Debug.LogError("Maze prefab has no Maze script attached!");
			return;
		}
		maze.Initialise((uint)ruleset.size.x, (uint)ruleset.size.y, tileDim);
		maze.defaultTheme = ruleset.defaultTileset;
		maze.startPosition = new Point(startX, 0);
		maze.entranceLength = entranceLength;

		CreateCorridors(mazeInstance);
		CreateTiles(grid, maze);
		CreateTileGeometry(maze);
		TextureMaze();
		UpdateMazeUVs();

		if (ruleset.rooms.Length > 0)
			state = GenerationState.RunningSprawlers;
		else
			state = GenerationState.Finished;
		
		if (!stepThrough)
			while (Step()) {}
	}

	private void FinishMaze()
	{
		if (!stepThrough)
		{
			TextureMaze();
			UpdateMazeUVs();
		}

		if (onComplete != null)
			onComplete.Invoke(maze);
		state = GenerationState.Idle;

		// Remove all of the unnecessary objects.
		grid = null;
		maze = null;
		ruleset = null;
		roomStyles = null;
		currentSprawlerRuleset = null;
		currentSprawlerTiles = null;
		newSprawlerTiles = null;
		messageLog = null;
		tilesetMaterials = null;
	}

	public bool Step()
	{
		messageLog.Clear();

		switch (state)
		{
			// Running the Sprawlers in the current ruleset.
			case GenerationState.RunningSprawlers:
				currentSprawlerRuleset = ruleset.rooms[currentSprawlerRulesetIndex];
				
				// Create a new Sprawler if we don't have one to Step.
				if (currentSprawler == null)
				{
					Tile startTile = null;
					switch (currentSprawlerRuleset.start)
					{
						case RoomRuleset.Start.Start:
							startTile = maze.tiles[maze.startPosition.y, maze.startPosition.x];
							break;
						case RoomRuleset.Start.Random:
							Point randomPoint = new Point(Random.instance.Next(maze.size.x), Random.instance.Next(maze.size.y));
							startTile = maze.tiles[randomPoint.y, randomPoint.x];
							break;
						case RoomRuleset.Start.End:
							startTile = maze.tiles[endPointCoord.y, endPointCoord.x];
							break;
					}

					currentSprawlerTiles = new List<Tile>();
					currentSprawler = new Sprawler(maze, startTile.position, currentSprawlerRuleset.size,
						(Tile tile) => { newSprawlerTiles.Add(tile); } );
					messageLog.Add("Added sprawler at " + startTile.position.ToString());
				}

				newSprawlerTiles.Clear();

				// Step the current Sprawler.
				bool sprawlerFinished = !currentSprawler.Step();

				// Copy the newly added Tiles to the current Sprawler's Tiles.
				foreach (Tile tile in newSprawlerTiles)
					currentSprawlerTiles.Add(tile);

				if (sprawlerFinished)
				{
					if (!currentSprawler.success)
					{
						messageLog.Add("Sprawler failed");
						numSprawlersFailed++;
						if (numSprawlersFailed >= MaxSprawlerFailures)
						{
							Debug.LogWarning("Maximum amount of Sprawlers failed, moving to the next ruleset.");
							NextSprawlerRuleset();
						}
					}
					else
					{
						messageLog.Add("Sprawler finished successfully");

						// Apply the new theme to the Sprawler's Tiles.
						foreach (Tile tile in currentSprawlerTiles)
							tile.theme = roomStyles[ruleset.rooms[currentSprawlerRulesetIndex].style].tileset;
						
						if (stepThrough)
						{
							TextureMaze();
							UpdateMazeUVs();
						}

						// Move to the next Sprawler.
						numSprawlersRun++;
						if (numSprawlersRun >= currentSprawlerRuleset.count)
						{
							// If we've run all the Sprawlers in the current SprawlerRuleset, move to the next one.
							NextSprawlerRuleset();
						}
					}
					currentSprawler = null;
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
		if (newSprawlerTiles != null)
		{
			foreach (Tile tile in newSprawlerTiles)
			{
				Gizmos.color = new Color(0.5f, 0.9f, 0.5f, 0.5f);
				Gizmos.DrawCube(Nav.TileToWorldPos(tile.position, maze.tileSize) + new Vector3(0.0f, 1.0f, 0.0f), new Vector3(maze.tileSize.x, 2.0f, maze.tileSize.y));
			}
		}
		if (currentSprawler != null)
		{
			foreach (Crawler c in currentSprawler.crawlers)
			{
				Vector3 crawlerPosition = Nav.TileToWorldPos(c.position, maze.tileSize) + new Vector3(0.0f, 1.0f, 0.0f);

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
		numSprawlersRun = 0;
		numSprawlersFailed = 0;
		currentSprawlerRulesetIndex++;
		if (currentSprawlerRulesetIndex >= ruleset.rooms.GetLength(0))
		{
			// If we've run all the SprawlerRulesets in the MazeRuleset, move to the next state.
			currentSprawlerRulesetIndex = 0;
			state = GenerationState.Finished;
		}
	}

	private void TextureMaze()
	{
		for (uint y = 0; y < maze.tiles.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.tiles.GetLength(1); x++)
			{
				TextureTile(maze.tiles[y, x]);
			}
		}
	}

	private void TextureTile(Tile tile)
	{
		// Create the material(s) for the tile if they haven't been created yet.
		if (!tilesetMaterials.ContainsKey(tile.theme))
		{
			Material regularMaterial = new Material(regularShader);
			Material floorMaterial = regularMaterial;
			Material ceilingMaterial = regularMaterial;

			// Use the tile's tileset if it's loaded.
			if (themeManager.Textures.ContainsKey(tile.theme))
			{
				Texture2D tileset = themeManager.Textures[tile.theme];
				regularMaterial.mainTexture = tileset;

				// Create a seamless texture for the floor if one exists.
				if (themeManager.Textures.ContainsKey(tile.theme + floorSuffix))
				{
					floorMaterial = new Material(seamlessShader);
					floorMaterial.mainTexture = tileset;
					floorMaterial.SetTexture("_SeamlessTex", themeManager.Textures[tile.theme + floorSuffix]);
					floorMaterial.SetTextureScale("_SeamlessTex", new Vector2(1.0f / tileDim.x, 1.0f / tileDim.y));
				}

				// Create a seamless texture for the ceiling if one exists.
				if (themeManager.Textures.ContainsKey(tile.theme + ceilingSuffix))
				{
					ceilingMaterial = new Material(seamlessShader);
					ceilingMaterial.mainTexture = tileset;
					ceilingMaterial.SetTexture("_SeamlessTex", themeManager.Textures[tile.theme + ceilingSuffix]);
					ceilingMaterial.SetTextureScale("_SeamlessTex", new Vector2(1.0f / tileDim.x, 1.0f / tileDim.y));
				}
			}
			// If the tile's tileset isn't loaded, try using the default one.
			else if (themeManager.Textures.ContainsKey("default"))
			{
				regularMaterial.mainTexture = themeManager.Textures["default"];
				Debug.LogWarning("Tried using tileset called \"" + tile.theme + "\" but it isn't loaded, using the default tileset.", tile.instance);
			}
			// The default tileset wasn't loaded either.
			else
			{
				Debug.LogWarning("Tried using the default tileset since a tileset named \"" + tile.theme + "\" isn't loaded, but the default one isn't loaded either.", tile.instance);
			}

			tilesetMaterials.Add(tile.theme, regularMaterial);
			tilesetMaterials.Add(tile.theme + floorSuffix, floorMaterial);
			tilesetMaterials.Add(tile.theme + ceilingSuffix, ceilingMaterial);
		}

		tile.instance.transform.Find("Walls").GetComponent<MaterialSetter>().SetMaterial(tilesetMaterials[tile.theme]);
		tile.instance.transform.Find("Floor").GetComponent<MaterialSetter>().SetMaterial(tilesetMaterials[tile.theme + floorSuffix]);
		tile.instance.transform.Find("Ceiling").GetComponent<MaterialSetter>().SetMaterial(tilesetMaterials[tile.theme + ceilingSuffix]);
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

		if (distance > endPointDist)
		{
			List<Dir> possibleEndPointDirs = new List<Dir>();
			if (x == 0)							possibleEndPointDirs.Add(Dir.W);
			if (y == 0)							possibleEndPointDirs.Add(Dir.N);
			if (x == (grid.GetLength(0) - 1))	possibleEndPointDirs.Add(Dir.E);
			if (y == (grid.GetLength(1) - 1))	possibleEndPointDirs.Add(Dir.S);

			if (possibleEndPointDirs.Count > 0)
			{
				endPointCoord.Set(x, y);
				endPointDist = distance;

				Utils.Shuffle(Random.instance, possibleEndPointDirs);
				endPointDir = possibleEndPointDirs[0];
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
				tile.theme = ruleset.defaultTileset;
				maze.AddTile(tile);
			}
		}
	}

	private void CreateCorridors(GameObject mazeInstance)
	{
		GameObject entrance = Instantiate(corridor, maze.TileToWorldPosition(maze.startPosition) - new Vector3(maze.tileSize.y / 2.0f, 0.0f, 0.0f), Quaternion.identity, mazeInstance.transform);
		entrance.transform.localScale = new Vector3(entranceLength, 1.0f, 1.0f);
		entrance.name = "Entrance";

		GameObject exit = Instantiate(corridor,
			maze.TileToWorldPosition(endPointCoord) + new Vector3(Nav.DY[endPointDir] * (maze.tileSize.y / 2.0f), 0.0f, Nav.DX[endPointDir] * (maze.tileSize.x / 2.0f)),
			Quaternion.Euler(0.0f, Nav.FacingToAngle(endPointDir), 0.0f), mazeInstance.transform);
		exit.transform.localScale = new Vector3(entranceLength, 1.0f, 1.0f);
		exit.name = "Exit";
	}

	/// <summary>
	/// Creates the tile geometries for all the tiles in a maze.
	/// </summary>
	/// <param name="maze">Maze to generate tile geometries for.</param>
	private void CreateTileGeometry(Maze maze)
	{
		for (uint y = 0; y < maze.tiles.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.tiles.GetLength(1); x++)
			{
				maze.tiles[y, x].instance.AddComponent<MaterialSetter>();
				maze.tiles[y, x].instance.transform.position = new Vector3(y * tileDim.y, 0.0f, x * tileDim.x);

				CreateFloor(maze.tiles[y, x]);
				CreateCeiling(maze.tiles[y, x]);
				CreateWalls(maze.tiles[y, x]);
			}
		}
	}

	/// <summary>
	/// Creates the floor instance for a Tile and parents it.
	/// </summary>
	/// <param name="tile"></param>
	private void CreateFloor(Tile tile)
	{
		GameObject floorInstance = (GameObject)Instantiate(floor,
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
		GameObject ceilingInstance = (GameObject)Instantiate(ceiling,
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
				GameObject wallInstance = (GameObject)Instantiate(wall, new Vector3(),
						Quaternion.Euler(0.0f, Nav.FacingToAngle(dir), 0.0f));
				wallInstance.transform.SetParent(wallsInstance.transform, false);
				wallInstance.transform.position += wallInstance.transform.rotation * new Vector3(-tileDim.y / 2.0f, 0.0f, 0.0f);
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
				Tile neighbourTile = maze.GetTile(neighbourPos);
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
		for (uint y = 0; y < maze.tiles.GetLength(0); y++)
		{
			for (uint x = 0; x < maze.tiles.GetLength(1); x++)
			{
				UpdateTileUV(maze.tiles[y, x]);
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
					if (leftPos.x >= 0 && leftPos.x < maze.tiles.GetLength(1) && leftPos.y >= 0 && leftPos.y < maze.tiles.GetLength(0))
					{
						if (Autotile.IsWallConnected(tile.value, maze.tiles[leftPos.y, leftPos.x].value, dir))
							wallValue |= 1;
					}
				}
				// Check to the right of the wall direction.
				if (Nav.IsConnected(fixedTileValue, Nav.right[dir]))
				{
					Point rightPos = tile.position + new Point(Nav.DX[Nav.right[dir]], Nav.DY[Nav.right[dir]]);
					if (rightPos.x >= 0 && rightPos.x < maze.tiles.GetLength(1) && rightPos.y >= 0 && rightPos.y < maze.tiles.GetLength(0))
					{
						if (Autotile.IsWallConnected(tile.value, maze.tiles[rightPos.y, rightPos.x].value, dir))
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
