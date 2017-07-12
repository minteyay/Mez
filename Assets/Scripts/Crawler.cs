using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// A class that travels along the maze, executing a given function on rooms it passes through.
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

	public static uint MAX_DISTANCE = 2048;

	private Maze maze = null;
	
	private Dir facing = Dir.N;
	private uint distance = 0;
	public Point position = null;
	public bool finished = false;

	private OnUpdate onUpdate = null;
	private OnComplete onComplete = null;

	private bool allowTurns = true;

	public Crawler(Maze maze, Point position, Dir facing, uint distance, OnUpdate onUpdate = null, OnComplete onComplete = null, bool allowTurns = true)
	{
		this.maze = maze;
		this.position = position;
		this.facing = facing;
		if (distance == 0)
			this.distance = MAX_DISTANCE;
		else
			this.distance = distance;
		this.onUpdate = onUpdate;
		this.onComplete = onComplete;
		this.allowTurns = allowTurns;

		// Step on the starting room.
		this.distance--;
		if (onUpdate != null)
			onUpdate.Invoke(maze.rooms[position.y, position.x]);
		if (this.distance == 0)
			if (onComplete != null)
				onComplete.Invoke(maze.rooms[position.y, position.x]);
	}

	public bool Step()
	{
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

			if (newPos == position)
			{
				// Dead end was hit, stop crawling.
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

			if (onUpdate != null)
				onUpdate.Invoke(maze.rooms[newPos.y, newPos.x]);

			facing = newFacing;
			position = newPos;
			return true;
		}

		finished = true;
		if (onComplete != null)
			onComplete.Invoke(maze.rooms[position.y, position.x]);
		return false;
	}

	public static void Crawl(Maze maze, Point position, Dir facing, uint distance, OnUpdate onUpdate = null, OnComplete onComplete = null, bool allowTurns = true)
	{
		Crawler crawler = new Crawler(maze, position, facing, distance, onUpdate, onComplete, allowTurns);
		while (crawler.Step()) {}
	}

    // /// <summary>
    // /// Crawls through a maze.
    // /// </summary>
    // /// <param name="maze">Maze to crawl through.</param>
    // /// <param name="start">World position to start at.</param>
    // /// <param name="facing">Cardinal direction to start crawling towards.</param>
    // /// <param name="distance">Rooms to enter until the crawling is finished.</param>
    // /// <param name="onUpdate">Callback for entering a room.</param>
    // /// <param name="onComplete">Callback for finishing crawling. (can be called multiple times if branch == true)</param>
    // /// <param name="branch">If the crawler is allowed to branch into multiple crawlers when at a crossroads. (with this onComplete can be called multiple times)</param>
    // /// <param name="allowTurns">If the crawler is allowed to turn at all.</param>
	// public static void Crawl(Maze maze, Point start, Dir facing, uint distance, OnUpdate onUpdate = null, OnComplete onComplete = null, bool branch = true, bool allowTurns = true)
	// {
	// 	Dir prevFacing = facing;
	// 	Point prevPos = start;

    //     // OnUpdate callback on the first room the Crawler starts at.
	// 	if (onUpdate != null)
	// 	{
	// 		onUpdate.Invoke(maze.rooms[start.y, start.x]);
	// 	}

    //     // Keep crawling as long as there is distance left.
	// 	while (distance > 0)
	// 	{
	// 		distance--;

    //         // Get a new position for a room to try to move to.
	// 		Point newPos = maze.MoveStraight(prevPos, prevFacing, false);

    //         // Return if the crawler has hit a dead end.
	// 		if (newPos == prevPos)
	// 			return;

    //         // Calculate the new cardinal direction to face when moving to the new room.
	// 		Point posDelta = prevPos - newPos;
	// 		float targetAngle = Quaternion.LookRotation(new Vector3(posDelta.x, posDelta.y), Vector3.up).eulerAngles.y;

	// 		Dir newFacing = Nav.GetFacing(targetAngle);
	// 		if (!allowTurns)
	// 		{
    //             // Break if no turns are allowed but the crawler is trying to turn.
	// 			if (newFacing != prevFacing)
	// 				break;
	// 		}
	// 		prevFacing = newFacing;

    //         // OnUpdate callback on the new room to move to.
    //         if (onUpdate != null)
    //             onUpdate.Invoke(maze.rooms[newPos.y, newPos.x]);

    //         if (branch)
	// 		{
    //             // If branching is allowed, check for all the directions the crawler can branch to.
	// 			List<Dir> connections = new List<Dir>();
	// 			foreach (Dir dir in Enum.GetValues(typeof(Dir)))
	// 			{
	// 				if (dir != Nav.opposite[prevFacing])
	// 					if (Nav.IsConnected(maze.rooms[newPos.y, newPos.x].value, dir))
	// 						connections.Add(dir);
	// 			}
	// 			if (connections.Count > 1)
	// 			{
    //                 // If there were extra connections to branch to, start a new crawler in those directions.
	// 				foreach (Dir dir in connections)
	// 				{
	// 					if (distance > 0)
	// 						Crawl(maze, newPos, dir, distance, onUpdate, onComplete, branch);
	// 				}
	// 				prevPos = newPos;
	// 				break;
	// 			}
	// 		}

	// 		prevPos = newPos;
	// 	}

    //     // OnComplete callback when finishing crawling.
	// 	if (onComplete != null)
	// 	{
	// 		onComplete.Invoke(maze.rooms[prevPos.y, prevPos.x]);
	// 	}
	// }

    // /// <summary>
    // /// Keep crawling through the maze until the crawler has to turn.
    // /// </summary>
    // /// <param name="maze">Maze to crawl through</param>
    // /// <param name="start">World position to start at.</param>
    // /// <param name="facing">Cardinal direction to start crawling towards.</param>
    // /// <param name="onUpdate">Callback for entering a room.</param>
    // /// <param name="onComplete">Callback for finishing crawling.</param>
	// public static void CrawlUntilTurn(Maze maze, Point start, Dir facing, OnUpdate onUpdate = null, OnComplete onComplete = null)
	// {
    //     /*
    //      * See Crawler.Crawl for how this works.
    //      */

	// 	Dir prevFacing = facing;
	// 	Point prevPos = start;

	// 	if (onUpdate != null)
	// 	{
	// 		onUpdate.Invoke(maze.rooms[start.y, start.x]);
	// 	}

	// 	while (true)
	// 	{
	// 		Dir newFacing;
	// 		Point newPos = maze.MoveLeftmost(prevPos, prevFacing, out newFacing);

	// 		if (newPos == prevPos)
	// 			return;

	// 		Point posDelta = prevPos - newPos;
	// 		float targetAngle = Quaternion.LookRotation(new Vector3(posDelta.x, posDelta.y), Vector3.up).eulerAngles.y;

	// 		if (newFacing != prevFacing)
	// 			break;
	// 		prevFacing = newFacing;

	// 		if (onUpdate != null)
	// 			onUpdate.Invoke(maze.rooms[newPos.y, newPos.x]);

	// 		prevPos = newPos;
	// 	}

	// 	if (onComplete != null)
	// 	{
	// 		onComplete.Invoke(maze.rooms[prevPos.y, prevPos.x]);
	// 	}
	// }
}
