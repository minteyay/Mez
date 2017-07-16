using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A player object that moves automatically in a maze.
/// </summary>
public class Player : MonoBehaviour
{
    /// Base movement speed of the player.
	public float movementSpeed = 0.0f;

    private Vector3 target;
	private Vector3 nextTarget;

	private Dir _facing;
	public Dir facing { get { return _facing; } set { _facing = nextFacing = value; } }
	private Dir nextFacing;

	[HideInInspector]
	public bool canMove = false;

	private enum State
	{
		Rotating,
		Moving
	}
	private State state = State.Rotating;

	[HideInInspector]
	public Maze maze = null;

	void Start()
	{
		Reset();
	}

	void Update()
	{
		if (canMove)
		{
			Vector3 delta = target - transform.position;
			float deltaDist = delta.magnitude;

			switch (state)
			{
				case State.Rotating:
					transform.rotation = Quaternion.Euler(new Vector3(0.0f, Nav.FacingToAngle(facing) + 90.0f, 0.0f));
					state = State.Moving;
					break;
				case State.Moving:
					transform.Translate(delta.normalized * Mathf.Min(movementSpeed * Time.deltaTime, deltaDist), Space.World);
					if (transform.position == target)
					{
						NewTarget();
						state = State.Rotating;
					}
					break;
			}
		}
	}

    /// <summary>
    /// Set a new target world position to move towards.
    /// </summary>
	private void NewTarget()
	{
		target = nextTarget;
		facing = nextFacing;

		nextTarget = maze.RoomToWorldPosition(maze.MoveLeftmost(maze.WorldToRoomPosition(target), facing, out nextFacing));

		// Dir newFacing = Dir.N;
		// Vector3 oldTarget = target;
        // // Get a new target to move towards, always hugging the left wall.
		// Vector3 newTarget = maze.RoomToWorldPosition(maze.MoveLeftmost(Nav.WorldToIndexPos(oldTarget, maze.roomDim), Nav.AngleToFacing(transform.rotation.eulerAngles.y), out newFacing));
		// Vector3 delta = oldTarget;

        // // Plot a new path to the target position.
		// pathToTarget.Clear();
		// pathToTarget.Add(oldTarget + (newTarget - oldTarget).normalized * maze.roomDim.x * accelTime * (movementSpeed / 4));
		// Crawler.Crawl(maze, Nav.WorldToIndexPos(newTarget, maze.roomDim), newFacing, 1000, null, room => target = Nav.IndexToWorldPos(room.position, maze.roomDim), false);
		// pathToTarget.Add(target - (target - oldTarget).normalized * maze.roomDim.x * accelTime * (movementSpeed / 4));
		// pathToTarget.Add(target);

		// delta = delta - target;
		// targetAngle = 0.0f;
		// if (delta.magnitude > 0.0f)
		// 	targetAngle = Quaternion.LookRotation(delta, transform.up).eulerAngles.y;
	}

	public void Reset()
	{
        // Reset the player.
		state = State.Rotating;
		target = new Vector3();
		nextTarget = new Vector3();
		NewTarget();
	}
}
