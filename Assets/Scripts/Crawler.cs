using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A class that travels along the maze, executing a given function on rooms it passes through.
/// Can be stepped forwards one room at a time. Stops when it hits a dead end or when trying to turn if allowTurns == false.
/// </summary>
class Crawler
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
	public Point position = null;

	/// Has the Crawler visited the starting room?
	private bool started = false;
	/// Has the Crawler finished its crawling?
	public bool finished = false;

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
		this.position = position;
		this.facing = facing;
		if (distance == 0)
			this.distance = DefaultDistance;
		else
			this.distance = distance;
		this.onUpdate = onUpdate;
		this.onComplete = onComplete;
		this.allowTurns = allowTurns;
		this.onlyStepOnDefault = onlyStepOnDefault;
	}

	/// <summary>
	/// Step on the starting room. This will get called in the first call to Step if not called otherwise.
	/// </summary>
	public bool Start()
	{
		// Check the starting room's theme if it's relevant.
		if (onlyStepOnDefault && maze.GetRoom(position).theme != "default")
			return false;

		// Step on the starting room.
		distance--;
		if (onUpdate != null)
			onUpdate.Invoke(maze.rooms[position.y, position.x]);
		if (distance == 0)
			if (onComplete != null)
				onComplete.Invoke(maze.rooms[position.y, position.x]);
		
		started = true;
		return true;
	}

	/// <summary>
	/// Step the Crawler forwards by one room.
	/// If the Crawler hasn't been started, steps on the starting room instead of moving.
	/// </summary>
	/// <returns>True if the Crawler visited a new room successfully, false otherwise.</returns>
	public bool Step()
	{
		// Start the crawler if it hasn't been already.
		if (!started)
			return Start();

		if (finished)
		{
			Debug.LogWarning("Can't step a finished Crawler!");
			return false;
		}

		if (distance > 0)
		{
			distance--;

			// Get a new position for a room to try to move to.
			Point newPos = maze.MoveStraight(position, facing, false);

			if (newPos == position || (onlyStepOnDefault && maze.GetRoom(newPos).theme != "default"))
			{
				// Dead end or another room was hit, stop crawling.
				finished = true;
				if (onComplete != null)
					onComplete.Invoke(maze.rooms[newPos.y, newPos.x]);
				return false;
			}

			// Calculate new facing for the crawler.
			Point posDelta = position - newPos;
			float deltaAngle = Mathf.Atan2((float)posDelta.y, (float)posDelta.x) * Mathf.Rad2Deg;
			Dir newFacing = Nav.AngleToFacing(deltaAngle);

			if (!allowTurns)
			{
				if (newFacing != facing)
				{
					// Turns aren't allowed, but the crawler is trying to turn. Stop crawling.
					finished = true;
					if (onComplete != null)
						onComplete.Invoke(maze.rooms[position.y, position.x]);
					return false;
				}
			}

			// Callback on the new room.
			if (onUpdate != null)
				onUpdate.Invoke(maze.rooms[newPos.y, newPos.x]);
			
			// Check if there's a parent Sprawler and possible directions to branch in (from the previous room).
			if (sprawler != null)
			{
				foreach (Dir dir in Enum.GetValues(typeof(Dir)))
				{
					// Don't start branching crawlers in the direction this crawler is moving in and has already visited.
					if (dir != newFacing && dir != Nav.opposite[facing])
					{
						if (Nav.IsConnected(maze.rooms[position.y, position.x].value, dir))
						{
							// Queue a branching crawler in the parent Sprawler.
							Point branchPos = position + new Point(Nav.DX[dir], Nav.DY[dir]);
							sprawler.QueueBranch(new Crawler(maze, branchPos, dir, 0, null, null));
						}
					}
				}
			}

			// Update the crawler's current facing and position.
			facing = newFacing;
			position = newPos;
			return true;
		}

		// No distance left, stop crawling.
		finished = true;
		if (onComplete != null)
			onComplete.Invoke(maze.rooms[position.y, position.x]);
		return false;
	}

	/// <summary>
	/// Runs a Crawler until it stops.
	/// </summary>
	public static void Crawl(Crawler crawler)
	{
		if (crawler == null)
		{
			Debug.LogError("Can't run a null Crawler!");
			return;
		}
		while (crawler.Step()) {}
	}
}

/// <summary>
/// A class that starts from a room in a maze and starts running Crawlers in all directions until a given amount of rooms have been visited.
/// </summary>
class Sprawler
{
	private List<Crawler> crawlers = new List<Crawler>();
	private List<Crawler> queuedBranches = new List<Crawler>();
	private int size = 0;

	/// <summary>
	/// Create a Sprawler and run it.
	/// </summary>
	/// <param name="maze">Maze to sprawl in.</param>
	/// <param name="position">Room position to start sprawling from.</param>
	/// <param name="size">Number of rooms to visit.</param>
	/// <param name="onUpdate">Callback on all visited rooms.</param>
	public Sprawler(Maze maze, Point position, int size, Crawler.OnUpdate onUpdate = null)
	{
		if (size <= 0)
		{
			Debug.LogWarning("Sprawler can't have a size of 0, defaulting to 1.");
			this.size = 1;
		}
		else
			this.size = size;

		// Check all possible directions to start crawlers in.
		List<Dir> possibleDirs = new List<Dir>();
		foreach (Dir dir in Enum.GetValues(typeof(Dir)))
		{
			if (Nav.IsConnected(maze.rooms[position.y, position.x].value, dir))
				possibleDirs.Add(dir);
		}
		Utils.Shuffle(Random.instance, possibleDirs);

		// Create the first crawler.
		Crawler firstCrawler = new Crawler(maze, position, possibleDirs[0], 0, onUpdate);
		AddCrawler(firstCrawler);

		// Try to step the first crawler.
		StepCrawler(firstCrawler);
		
		// If there's still rooms to go, try to create one in the opposite direction to the first one.
		if (this.size > 0 && possibleDirs.Contains(Nav.opposite[possibleDirs[0]]))
		{
			Dir newDir = Nav.opposite[possibleDirs[0]];
			Point newPos = position + new Point(Nav.DX[newDir], Nav.DY[newDir]);

			AddCrawler(new Crawler(maze, newPos, newDir, 0, onUpdate));
		}

		// SPRAWLING LOOP
		// Run the crawlers until the sprawling size is reached or until they all stop.
		int currentCrawler = 0;
		while (this.size > 0 && crawlers.Count > 0)
		{
			// Loop the crawler index if it's out of bounds.
			if (currentCrawler >= crawlers.Count)
				currentCrawler = 0;
			
			// Step the current crawler and check if it's finished.
			if (!StepCrawler(crawlers[currentCrawler]))
				crawlers.RemoveAt(currentCrawler);
			else
				currentCrawler++;
			
			// If new branches were queued, add them.
			while (queuedBranches.Count > 0 && this.size > 0)
			{
				AddCrawler(queuedBranches[0]);
				queuedBranches.RemoveAt(0);
			}
		}
	}

	/// <summary>
	/// Queue a branch to add in the sprawling loop in the constructor.
	/// This shouldn't be called manually, it's used for branching between a Sprawler and its Crawlers.
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
	private bool AddCrawler(Crawler crawler)
	{
		if (crawler == null)
		{
			Debug.LogWarning("Can't add a null Crawler to a Sprawler.");
			return false;
		}
		if (size > 0)
		{
			crawler.Start();
			crawlers.Add(crawler);
			crawler.sprawler = this;
			size--;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Steps a given Crawler.
	/// </summary>
	/// <param name="crawler"></param>
	/// <returns>True if the Crawler was stepped succesfully, false if the Crawler or this Sprawler is finished.</returns>
	private bool StepCrawler(Crawler crawler)
	{
		if (size > 0)
		{
			if (!crawler.Step())
				return false;
			size--;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Create a Sprawler and run it.
	/// </summary>
	/// <param name="maze">Maze to sprawl in.</param>
	/// <param name="position">Room position to start sprawling from.</param>
	/// <param name="size">Number of rooms to visit.</param>
	/// <param name="onUpdate">Callback on all visited rooms.</param>
	public static void Sprawl(Maze maze, Point position, int size, Crawler.OnUpdate onUpdate = null)
	{
		new Sprawler(maze, position, size, onUpdate);
	}
}