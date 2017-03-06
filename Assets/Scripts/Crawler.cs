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
    /// <param name="distance">Distance (in rooms) that the crawler still has until it's finished.</param>
	public delegate void OnUpdate(Room room, int distance);
    /// <summary>
    /// Callback that gets called when the Crawler is finished crawling.
    /// </summary>
    /// <param name="room">Room the Crawler stopped at.</param>
	public delegate void OnComplete(Room room);

    /// <summary>
    /// Crawls through a maze.
    /// </summary>
    /// <param name="maze">Maze to crawl through.</param>
    /// <param name="start">World position to start at.</param>
    /// <param name="facing">Cardinal direction to start crawling towards.</param>
    /// <param name="distance">Rooms to enter until the crawling is finished.</param>
    /// <param name="onUpdate">Callback for entering a room.</param>
    /// <param name="onComplete">Callback for finishing crawling. (can be called multiple times if branch == true)</param>
    /// <param name="branch">If the crawler is allowed to branch into multiple crawlers when at a crossroads. (with this onComplete can be called multiple times)</param>
    /// <param name="allowTurns">If the crawler is allowed to turn at all.</param>
	public static void Crawl(Maze maze, Vector3 start, Dir facing, int distance, OnUpdate onUpdate = null, OnComplete onComplete = null, bool branch = true, bool allowTurns = true)
	{
		Dir prevFacing = facing;
		Vector3 prevPos = start;

        // OnUpdate callback on the first room the Crawler starts at.
		if (onUpdate != null)
		{
			Point startPoint = Nav.GetIndexAt(start, maze.roomDim);
			onUpdate.Invoke(maze.rooms[startPoint.y, startPoint.x], distance);
		}

        // Keep crawling as long as there is distance left.
		while (distance > 0)
		{
			distance--;

            // Get a new world position for a room to try to move to.
			Vector3 newPos = maze.MoveStraight(prevPos, prevFacing, false);

            // Return if the crawler has hit a dead end.
			if (newPos == prevPos)
				return;
			Point newPoint = Nav.GetIndexAt(newPos, maze.roomDim);

            // Calculate the new cardinal direction to face when moving to the new room.
			Vector3 posDelta = prevPos - newPos;
			float targetAngle = Quaternion.LookRotation(posDelta, Vector3.up).eulerAngles.y;

			Dir newFacing = Nav.GetFacing(targetAngle);
			if (!allowTurns)
			{
                // Break if no turns are allowed but the crawler is trying to turn.
				if (newFacing != prevFacing)
					break;
			}
			prevFacing = newFacing;

            // OnUpdate callback on the new room to move to.
            if (onUpdate != null)
                onUpdate.Invoke(maze.rooms[newPoint.y, newPoint.x], distance);

            if (branch)
			{
                // If branching is allowed, check for all the directions the crawler can branch to.
				List<Dir> connections = new List<Dir>();
				foreach (Dir dir in Enum.GetValues(typeof(Dir)))
				{
					if (dir != Nav.opposite[prevFacing])
						if (Nav.IsConnected(maze.rooms[newPoint.y, newPoint.x].value, dir))
							connections.Add(dir);
				}
				if (connections.Count > 1)
				{
                    // If there were extra connections to branch to, start a new crawler in those directions.
					foreach (Dir dir in connections)
					{
						if (distance > 0)
							Crawl(maze, newPos, dir, distance, onUpdate, onComplete, branch);
					}
					prevPos = newPos;
					break;
				}
			}

			prevPos = newPos;
		}

        // OnComplete callback when finishing crawling.
		if (onComplete != null)
		{
			Point finalPoint = Nav.GetIndexAt(prevPos, maze.roomDim);
			onComplete.Invoke(maze.rooms[finalPoint.y, finalPoint.x]);
		}
	}

    /// <summary>
    /// Keep crawling through the maze until the crawler has to turn.
    /// </summary>
    /// <param name="maze">Maze to crawl through</param>
    /// <param name="start">World position to start at.</param>
    /// <param name="facing">Cardinal direction to start crawling towards.</param>
    /// <param name="onUpdate">Callback for entering a room.</param>
    /// <param name="onComplete">Callback for finishing crawling.</param>
	public static void CrawlUntilTurn(Maze maze, Vector3 start, Dir facing, OnUpdate onUpdate = null, OnComplete onComplete = null)
	{
        /*
         * See Crawler.Crawl for how this works.
         */

		Dir prevFacing = facing;
		Vector3 prevPos = start;

		if (onUpdate != null)
		{
			Point startPoint = Nav.GetIndexAt(start, maze.roomDim);
			onUpdate.Invoke(maze.rooms[startPoint.y, startPoint.x], 0);
		}

		while (true)
		{
			Dir newFacing;
			Vector3 newPos = maze.MoveLeftmost(prevPos, prevFacing, out newFacing);

			if (newPos == prevPos)
				return;
			Point newPoint = Nav.GetIndexAt(newPos, maze.roomDim);

			Vector3 posDelta = prevPos - newPos;
			float targetAngle = Quaternion.LookRotation(posDelta, Vector3.up).eulerAngles.y;

			if (newFacing != prevFacing)
				break;
			prevFacing = newFacing;

			if (onUpdate != null)
				onUpdate.Invoke(maze.rooms[newPoint.y, newPoint.x], 0);

			prevPos = newPos;
		}

		if (onComplete != null)
		{
			Point finalPoint = Nav.GetIndexAt(prevPos, maze.roomDim);
			onComplete.Invoke(maze.rooms[finalPoint.y, finalPoint.x]);
		}
	}
}
