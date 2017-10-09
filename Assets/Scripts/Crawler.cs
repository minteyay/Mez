using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A class that travels along the maze, executing a given function on rooms it passes through.
/// Can be stepped forwards one room at a time. Stops when it hits a dead end or when trying to turn if allowTurns == false.
/// </summary>
public class Crawler
{
	/// <summary>
	/// Callback that gets whenever the Crawler enters a room.
	/// </summary>
	/// <param name="room">Room that was crawled to.</param>
	public delegate void OnUpdate(Room room);

	/// <summary>
	/// Callback that gets called when the Crawler is finished crawling.
	/// </summary>
	/// <param name="room">Room the Crawler stopped at.</param>
	public delegate void OnComplete(Room room);

	/// Default distance to crawl when it's not specified.
	private static uint DefaultDistance = 2048;

	private Maze maze = null;
	/// Parent sprawler. When this is set, the Crawler will try to create new branching Crawlers.
	public Sprawler sprawler = null;
	
	private Dir facing = Dir.N;
	private uint distance = 0;
	public Point position { get; private set; }

	public Dir nextFacing { get; private set; }
	/// Position to move to on the next call to Step.
	private Point nextPosition = null;

	/// Has the Crawler visited the starting room?
	private bool started = false;
	/// Did the Crawler reach the desired distance?
	public bool success { get; private set; }

	/// Callback on every visited room.
	private OnUpdate onUpdate = null;
	/// Callback when crawling finishes.
	private OnComplete onComplete = null;

	/// Can the Crawler turn?
	private bool allowTurns = true;
	/// Can the Crawler only step on Rooms with the default theme?
	private bool onlyStepOnDefault = false;

	/// <summary>
	/// Create a new Crawler.
	/// </summary>
	/// <param name="maze">Maze to crawl in.</param>
	/// <param name="position">Position of the room to start crawling from.</param>
	/// <param name="facing">Direction to start crawling towards.</param>
	/// <param name="distance">Number of rooms to move through, including the starting room. Passing 0 means the Crawler will try to move a distance of DefaultDistance.</param>
	/// <param name="onUpdate">Callback on all visited rooms.</param>
	/// <param name="onComplete">Callback on the room this Crawler stops in.</param>
	/// <param name="allowTurns">Whether or not this Crawler is allowed to turn at all. Dead ends will still stop the Crawler.</param>
	public Crawler(Maze maze, Point position, Dir facing, uint distance = 0,
		OnUpdate onUpdate = null, OnComplete onComplete = null, bool allowTurns = true,
		bool onlyStepOnDefault = true)
	{
		this.maze = maze;
		this.position = nextPosition = position;
		this.facing = nextFacing = facing;
		if (distance == 0)
			this.distance = DefaultDistance;
		else
			this.distance = distance;
		this.onUpdate = onUpdate;
		this.onComplete = onComplete;
		this.allowTurns = allowTurns;
		this.onlyStepOnDefault = onlyStepOnDefault;

		success = false;
	}

	/// <summary>
	/// Step on the starting room. This will get called in the first call to Step if not called otherwise.
	/// </summary>
	public bool Start()
	{
		// Check the starting room's theme if it's relevant.
		if (onlyStepOnDefault && maze.GetRoom(position).theme != "default")
			return false;
		
		started = true;
		return true;
	}

	/// <summary>
	/// Step the Crawler forwards by one room (including the starting position).
	/// </summary>
	/// <returns>True if the Crawler still has distance to go, false if it finished.</returns>
	public bool Step()
	{
		// Start the Crawler if it hasn't been already.
		bool justStarted = false;
		if (!started)
		{
			if (!Start())
				return false;
			justStarted = true;
		}

		if (distance > 0)
		{
			distance--;

			// Step on the next position.
			facing = nextFacing;
			position = nextPosition;

			// Callback on the new room.
			if (onUpdate != null)
				onUpdate.Invoke(maze.GetRoom(position));
			
			// Calculate the next position to move to.
			nextPosition = maze.MoveStraight(position, facing, false);

			// Check the validity of the next position.
			if (nextPosition == position || (onlyStepOnDefault && maze.GetRoom(nextPosition).theme != "default"))
			{
				// Dead end or another room was hit, stop crawling.
				if (onComplete != null)
					onComplete.Invoke(maze.GetRoom(position));
				return false;
			}

			// Check the validity of the next facing.
			Point posDelta = nextPosition - position;
			float deltaAngle = Mathf.Atan2((float)posDelta.y, (float)posDelta.x) * Mathf.Rad2Deg;
			nextFacing = Nav.AngleToFacing(deltaAngle);

			if (!allowTurns && nextFacing != facing)
			{
				// Turns aren't allowed, but the crawler is trying to turn. Stop crawling.
				if (onComplete != null)
					onComplete.Invoke(maze.GetRoom(position));
				return false;
			}

			// Check if there's a parent Sprawler and possible directions to branch in.
			if (sprawler != null)
			{
				foreach (Dir dir in Enum.GetValues(typeof(Dir)))
				{
					// Don't start branches in the direction this Crawler is moving in.
					if (dir != facing)
					{
						/*
						Don't start branches in the direction this Crawler just came from.
						(unless it just started in which case there's no direction it came from)
						*/
						if (dir != Nav.opposite[facing] || justStarted)
						{
							if (Nav.IsConnected(maze.GetRoom(position).value, dir))
							{
								// Queue a branch in the parent Sprawler.
								Point branchPos = position + new Point(Nav.DX[dir], Nav.DY[dir]);
								sprawler.QueueBranch(new Crawler(maze, branchPos, dir, 0, onUpdate, onComplete));
							}
						}
					}
				}
			}
		}

		if (distance <= 0)
		{
			// No distance left, the Crawler finished successfully, stop crawling.
			success = true;
			if (onComplete != null)
				onComplete.Invoke(maze.GetRoom(position));
			return false;
		}
		return true;
	}
}

/// <summary>
/// A class that starts from a room in a maze and starts running Crawlers in all directions until a given amount of rooms have been visited.
/// </summary>
public class Sprawler
{
	public List<Crawler> crawlers { get; private set; }
	private int currentCrawlerIndex = 0;
	private uint size = 0;

	/// Did the Sprawler reach the desired size?
	public bool success { get; private set; }

	private List<Crawler> queuedBranches = new List<Crawler>();

	/// <summary>
	/// Create a Sprawler and run it.
	/// </summary>
	/// <param name="maze">Maze to sprawl in.</param>
	/// <param name="position">Room position to start sprawling from.</param>
	/// <param name="size">Number of rooms to visit.</param>
	/// <param name="onUpdate">Callback on all visited rooms.</param>
	public Sprawler(Maze maze, Point position, uint size, Crawler.OnUpdate onUpdate = null)
	{
		crawlers = new List<Crawler>();
		if (size <= 0)
		{
			Debug.LogWarning("Sprawler can't have a size of 0, defaulting to 1.");
			this.size = 1;
		}
		else
			this.size = size;
		success = false;

		// Check all possible directions to start crawlers in.
		List<Dir> possibleDirs = new List<Dir>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(maze.rooms[position.y, position.x].value, dir))
				possibleDirs.Add(dir);
		}
		Utils.Shuffle(Random.instance, possibleDirs);

		// Create the first crawler.
		AddCrawler(new Crawler(maze, position, possibleDirs[0], 0, onUpdate));
	}

	public bool Step()
	{
		// Add branches that were queued in the last step.
		foreach (Crawler branch in queuedBranches)
			AddCrawler(branch);
		queuedBranches.Clear();

		// Step the current crawler and remove it if it's finished.
		if (!crawlers[currentCrawlerIndex].Step())
			crawlers.RemoveAt(currentCrawlerIndex);
		size--;

		// Stop successfully if the desired size was reached.
		if (size <= 0)
		{
			success = true;
			return false;
		}

		// Stop if there aren't any crawlers left.
		if (crawlers.Count == 0)
			return false;
		
		// Move to the next Crawler.
		currentCrawlerIndex++;
		if (currentCrawlerIndex >= crawlers.Count)
			currentCrawlerIndex = 0;

		return true;
	}

	/// <summary>
	/// Queue a branch to add in the sprawling loop in the constructor.
	/// This shouldn't be called manually, it's used for branching between a Sprawler and its child Crawlers.
	/// </summary>
	/// <param name="branch"></param>
	public void QueueBranch(Crawler branch)
	{
		queuedBranches.Add(branch);
	}

	/// <summary>
	/// Adds a Crawler to be run in the sprawling loop.
	/// </summary>
	/// <param name="crawler"></param>
	/// <returns></returns>
	private void AddCrawler(Crawler crawler)
	{
		if (crawler == null)
		{
			Debug.LogWarning("Can't add a null Crawler to a Sprawler.");
			return;
		}
		crawlers.Add(crawler);
		crawler.sprawler = this;
	}
}