using UnityEngine;
using System;
using System.Collections.Generic;

class Crawler
{
    public delegate void OnUpdate(Room room, int distance);
    public delegate void OnComplete(Room room);

    public static void Crawl(Maze maze, Vector3 start, Dir facing, int distance, OnUpdate onUpdate = null, OnComplete onComplete = null, bool branch = true)
    {
        Dir prevFacing = facing;
        Vector3 prevPos = start;

        if (onUpdate != null)
        {
            Point startPoint = Nav.GetIndexAt(start, maze.roomDim);
            onUpdate.Invoke(maze.rooms[startPoint.y, startPoint.x], distance);
        }

        while (distance > 0)
        {
            distance--;

            Vector3 newPos = maze.MoveStraight(prevPos, prevFacing, false);
            if (newPos == prevPos)
                return;
            Point newPoint = Nav.GetIndexAt(newPos, maze.roomDim);

            if (onUpdate != null)
                onUpdate.Invoke(maze.rooms[newPoint.y, newPoint.x], distance);

            Vector3 posDelta = prevPos - newPos;
            float targetAngle = Quaternion.LookRotation(posDelta, Vector3.up).eulerAngles.y;
            prevFacing = Nav.GetFacing(targetAngle);

            if (branch)
            {
                List<Dir> connections = new List<Dir>();
                foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                {
                    if (dir != Nav.opposite[prevFacing])
                        if (Nav.IsConnected(maze.rooms[newPoint.y, newPoint.x].value, dir))
                            connections.Add(dir);
                }
                if (connections.Count > 1)
                {
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

        if (onComplete != null)
        {
            Point finalPoint = Nav.GetIndexAt(prevPos, maze.roomDim);
            onComplete.Invoke(maze.rooms[finalPoint.y, finalPoint.x]);
        }
    }
}
