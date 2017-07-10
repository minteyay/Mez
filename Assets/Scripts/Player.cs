using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A player object that moves automatically in a maze.
/// </summary>
public class Player : MonoBehaviour
{
    /// <summary>
    /// Base movement speed of the player.
    /// </summary>
	public float movementSpeed = 0.0f;
    /// <summary>
    /// Time it takes to accelerate from a standstill to the base movement speed (in seconds).
    /// </summary>
	public float accelTime = 0.0f;
    /// <summary>
    /// Time it takes to turn (in seconds).
    /// </summary>
	public float rotationSpeed = 0.0f;

    /// <summary>
    /// Target world position to move to.
    /// </summary>
	public Vector3 target;
	private float targetAngle = 0.0f;
	private int pathIndex = 0;
    /// <summary>
    /// List of world positions to move along when moving to the target.
    /// </summary>
	private List<Vector3> pathToTarget = new List<Vector3>();

	private bool canMove = false;
	[HideInInspector]
	public bool CanMove { get { return canMove; } set { canMove = value; if (!canMove) iTween.Stop(gameObject); } }

	private enum State
	{
		ROTATING,
		MOVING
	}
	private State state = State.ROTATING;

	private bool rotating = false;
	private bool moving = false;

    /// <summary>
    /// Maze to move automatically in.
    /// </summary>
	[HideInInspector]
	public Maze maze = null;

	void Start()
	{
		NewTarget();
	}

	void Update()
	{
		if (canMove)
		{
			Vector3 delta = transform.position - target;
			float deltaDist = delta.magnitude;

			float angleDelta = targetAngle - transform.rotation.eulerAngles.y;

			if (state == State.ROTATING)
			{
                // Rotate towards the target angle.
				if (!rotating)
				{
                    // Always prefer rotating counterclockwise.
					if (angleDelta == 180.0f)
						transform.Rotate(0f, -0.1f, 0f);

					rotating = true;
					iTween.RotateTo(gameObject, iTween.Hash("rotation", new Vector3(0f, targetAngle, 0f), "speed", rotationSpeed, "oncomplete", "RotationComplete", "easetype", iTween.EaseType.easeInOutSine));
				}
			}
			if (state == State.MOVING)
			{
                // Start moving towards the target world position.
				if (!moving && !rotating)
				{
					moving = true;
					if (pathIndex == 0)
					{
						iTween.MoveTo(gameObject, iTween.Hash("position", pathToTarget[pathIndex], "time", accelTime, "oncomplete", "StepComplete", "easeType", iTween.EaseType.easeInQuad));
					}
				}
			}
		}
	}

    /// <summary>
    /// Set a new target world position to move towards.
    /// </summary>
	private void NewTarget()
	{
		Dir newFacing = Dir.N;
		Vector3 oldTarget = target;
        // Get a new target to move towards, always hugging the left wall.
		Vector3 newTarget = maze.RoomToWorldPosition(maze.MoveLeftmost(Nav.GetIndexAt(oldTarget, maze.roomDim), Nav.GetFacing(transform.rotation.eulerAngles.y), out newFacing));
		Vector3 delta = oldTarget;

        // Plot a new path to the target position.
		pathToTarget.Clear();
		pathToTarget.Add(oldTarget + (newTarget - oldTarget).normalized * maze.roomDim.x * accelTime * (movementSpeed / 4));
		Crawler.CrawlUntilTurn(maze, Nav.GetIndexAt(newTarget, maze.roomDim), newFacing, null, room => target = Nav.GetPosAt(room.position, maze.roomDim));
		pathToTarget.Add(target - (target - oldTarget).normalized * maze.roomDim.x * accelTime * (movementSpeed / 4));
		pathToTarget.Add(target);

		delta = delta - target;
		targetAngle = 0.0f;
		if (delta.magnitude > 0.0f)
			targetAngle = Quaternion.LookRotation(delta, transform.up).eulerAngles.y;
	}

	private void RotationComplete()
	{
		rotating = false;
		state = State.MOVING;
	}

	private void StepComplete()
	{
		pathIndex++;
		if (pathIndex == pathToTarget.Count - 1)
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", pathToTarget[pathIndex], "time", accelTime, "oncomplete", "PathComplete", "easeType", iTween.EaseType.easeOutQuad));
		}
		else if (pathIndex != 0)
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", pathToTarget[pathIndex], "speed", movementSpeed, "oncomplete", "StepComplete", "easeType", iTween.EaseType.linear));
		}
	}

	private void PathComplete()
	{
        // Completed the current path, get a new target and rotate towards it.
		moving = false;
		state = State.ROTATING;
		pathIndex = 0;
		NewTarget();
	}

	public void Reset()
	{
        // Reset the player.
		state = State.ROTATING;
		target = new Vector3();
		pathIndex = 0;
		rotating = false;
		moving = false;
		NewTarget();
	}
}
