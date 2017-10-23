using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// An object that travels along the maze for as long as it can.
/// Can be stepped forwards one tile at a time.
/// Stops when it hits a dead end or an invalid tile.
/// </summary>
public class Crawler
{
	/// Callback for each tile the Crawler moves to.
	public delegate void OnTileEntered(Tile tile);

	/// Callback for the tile the Crawler stops at.
	public delegate void OnComplete(Tile tile);

	/// Default amount of tiles to move.
	private static uint DefaultDistance = 2048;

	private Maze _maze = null;
	/// Parent Sprawler. When this is set, the Crawler will try to branch when possible.
	public Sprawler sprawler = null;
	

	private uint _distance = 0;
	private Dir _facing = Dir.N;
	public Point position { get; private set; }

	public Dir nextFacing { get; private set; }
	private Point _nextPosition = null;

	/// Has the Crawler started moving?
	private bool _started = false;
	/// Did the Crawler reach the desired distance?
	public bool success { get; private set; }

	private OnTileEntered _onTileEntered = null;
	private OnComplete _onComplete = null;

	/// Can the Crawler turn?
	private bool _allowTurns = true;
	/// Can the Crawler only step on tiles with the default theme?
	private bool _onlyStepOnDefault = false;

	/// <param name="facing">Direction to start moving in.</param>
	/// <param name="distance">Distance to move (in tiles). A value of 0 means the crawler will travel DefaultDistance tiles.</param>
	/// <param name="allowTurns">Is this Crawler allowed to turn? Dead ends will still stop it.</param>
	/// <param name="onlyStepOnDefault">Is this Crawler only allowed to move on tiles using the Maze's default tileset?</param>
	public Crawler(Maze maze, Point startPosition, Dir facing, uint distance = 0,
		OnTileEntered onTileEntered = null, OnComplete onComplete = null, bool allowTurns = true,
		bool onlyStepOnDefault = true)
	{
		_maze = maze;
		position = _nextPosition = startPosition;
		_facing = nextFacing = facing;
		if (distance == 0)
			_distance = DefaultDistance;
		else
			_distance = distance;
		_onTileEntered = onTileEntered;
		_onComplete = onComplete;
		_allowTurns = allowTurns;
		_onlyStepOnDefault = onlyStepOnDefault;

		success = false;
	}

	/// <summary>
	/// Tries to start the Crawler.
	/// </summary>
	/// <returns>True if the Crawler started successfully, false otherwise.</returns>
	public bool Start()
	{
		// Check the starting tile's theme if it's relevant.
		if (_onlyStepOnDefault && _maze.GetTile(position).theme != "default")
			return false;
		
		_started = true;
		return true;
	}

	/// <summary>
	/// Move the Crawler forwards by one tile (including the starting position).
	/// </summary>
	/// <returns>True if the Crawler can still move and has distance to go, false otherwise.</returns>
	public bool Step()
	{
		// Start the Crawler if it hasn't been already.
		if (!_started && !Start())
			return false;

		bool validNextStep = true;

		if (_distance > 0)
		{
			_distance--;

			// Move to the next position.
			_facing = nextFacing;
			position = _nextPosition;

			// Callback on the new tile.
			if (_onTileEntered != null)
				_onTileEntered.Invoke(_maze.GetTile(position));
			
			// Calculate the next position to move to.
			_nextPosition = _maze.MoveForwards(position, _facing, Maze.MovementPreference.Straight);

			// Check the validity of the next position.
			if (_nextPosition == position || _maze.GetTile(_nextPosition) == null || (_onlyStepOnDefault && _maze.GetTile(_nextPosition).theme != "default"))
			{
				// The next position isn't valid, stop crawling.
				if (_onComplete != null)
					_onComplete.Invoke(_maze.GetTile(position));
				validNextStep = false;
			}

			// Check the validity of the next facing.
			Point posDelta = _nextPosition - position;
			float deltaAngle = Mathf.Atan2((float)posDelta.y, (float)posDelta.x) * Mathf.Rad2Deg;
			nextFacing = Nav.AngleToFacing(deltaAngle);

			if (!_allowTurns && nextFacing != _facing)
			{
				// The next facing isn't valid, stop crawling.
				if (_onComplete != null)
					_onComplete.Invoke(_maze.GetTile(position));
				validNextStep = false;
			}

			// Check if there's a parent Sprawler and possible directions to branch in.
			if (sprawler != null)
			{
				List<Dir> possibleDirs = Nav.GetConnections(_maze.GetTile(position).value);
				foreach (Dir dir in possibleDirs)
				{
					if (dir != nextFacing && dir != Nav.opposite[_facing])
					{
						// Queue a branch in the parent Sprawler if the branch is inside the maze.
						Point branchPos = position + new Point(Nav.DX[dir], Nav.DY[dir]);
						if (_maze.GetTile(branchPos) != null)
							sprawler.QueueBranch(new Crawler(_maze, branchPos, dir, 0, _onTileEntered, _onComplete));
					}
				}
			}
		}

		if (_distance <= 0)
		{
			// No distance left, the Crawler finished successfully, stop crawling.
			success = true;
			if (_onComplete != null)
				_onComplete.Invoke(_maze.GetTile(position));
			return false;
		}
		else if (!validNextStep)
		{
			// The next step won't be valid, stop crawling.
			if (_onComplete != null)
				_onComplete.Invoke(_maze.GetTile(position));
			return false;
		}

		return true;
	}
}

/// <summary>
/// An object that starts from a tile in a maze and runs Crawlers in all possible directions until a given amount of tiles have been visited.
/// </summary>
public class Sprawler
{
	/// Did the Sprawler reach the desired size?
	public bool success { get; private set; }
	private uint _size = 0;

	public List<Crawler> crawlers { get; private set; }
	private int _currentCrawlerIndex = 0;
	private List<Crawler> _queuedBranches = new List<Crawler>();

	/// <param name="size">Number of tiles to visit.</param>
	public Sprawler(Maze maze, Point startPosition, uint size, Crawler.OnTileEntered onTileEntered = null)
	{
		crawlers = new List<Crawler>();
		if (size <= 0)
		{
			Debug.LogWarning("Sprawler can't have a size of 0, defaulting to 1.");
			_size = 1;
		}
		else
			_size = size;
		success = false;

		// Check possible directions to start Crawlers in.
		List<Dir> possibleDirs = maze.GetConnections(maze.GetTile(startPosition));
		Dir randomDir = possibleDirs[Random.instance.Next(possibleDirs.Count)];
		
		// Start with two Crawlers in opposite directions if we can.
		if (possibleDirs.Contains(Nav.opposite[randomDir]) && size > 1)
		{
			AddCrawler(new Crawler(maze, startPosition + new Point(Nav.DX[randomDir], Nav.DY[randomDir]), randomDir, 0, onTileEntered));
			AddCrawler(new Crawler(maze, startPosition, Nav.opposite[randomDir], 0, onTileEntered));
		}
		else // Otherwise just start with one.
		{
			AddCrawler(new Crawler(maze, startPosition, randomDir, 0, onTileEntered));
		}
	}

	public bool Step()
	{
		// Add branches that were queued in the last step.
		foreach (Crawler branch in _queuedBranches)
			AddCrawler(branch);
		_queuedBranches.Clear();

		// Step the current crawler and remove it if it's finished.
		if (crawlers.Count > 0 && !crawlers[_currentCrawlerIndex].Step())
			crawlers.RemoveAt(_currentCrawlerIndex);
		_size--;

		// Stop successfully if the desired size was reached.
		if (_size <= 0)
		{
			success = true;
			return false;
		}

		// Stop if there aren't any Crawlers left (active or queued).
		if (crawlers.Count == 0 && _queuedBranches.Count == 0)
			return false;
		
		// Move to the next Crawler.
		_currentCrawlerIndex++;
		if (_currentCrawlerIndex >= crawlers.Count)
			_currentCrawlerIndex = 0;

		return true;
	}

	/// <summary>
	/// Queue a branch to be run in the Sprawler.
	/// This shouldn't be called manually, it's used for branching between a Sprawler and its child Crawlers.
	/// </summary>
	public void QueueBranch(Crawler branch)
	{
		// Don't queue Crawlers that can't even start.
		if (!branch.Start())
			return;

		_queuedBranches.Add(branch);
	}

	/// <summary>
	/// Adds a Crawler to be run in the Sprawler.
	/// </summary>
	private void AddCrawler(Crawler crawler)
	{
		if (crawler == null)
		{
			Debug.LogWarning("Can't add a null Crawler to a Sprawler.");
			return;
		}

		// Don't add Crawlers that can't even start.
		if (!crawler.Start())
			return;

		crawlers.Add(crawler);
		crawler.sprawler = this;
	}
}