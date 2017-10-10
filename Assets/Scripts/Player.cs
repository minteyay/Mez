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
	public float turnDistance = 1.0f;

	[SerializeField]
    private Vector3 target;
	[SerializeField]
	private Vector3? nextTarget;

	[SerializeField]
	private Dir _facing;
	public Dir facing { get { return _facing; } set { _facing = nextFacing = value; } }
	[SerializeField]
	private Dir nextFacing;

	private const float RIGHT_ANGLE_TURN_ANGLE = Mathf.PI / 2.0f;
	private const float U_TURN_ANGLE = Mathf.PI;

	[SerializeField]
	private float remainingDist = 0.0f;
	[SerializeField]
	private float remainingTurnDist = 0.0f;

	[HideInInspector]
	public Maze maze = null;

	void Start()
	{
		Reset();
	}

	void Update()
	{
		// Move the player.
		float toMove = movementSpeed * Time.deltaTime;
		Move(ref toMove);

		// Get a new target once all the forwards moving and turning have been done.
		if (remainingDist <= 0.0f && remainingTurnDist <= 0.0f)
			NewTarget();

		// Move again if there's still distance left to go.
		if (toMove > 0.0f)
			Move(ref toMove);
		
		if (toMove > 0.0f)
			Debug.LogWarning("There's still " + toMove + " to move this frame, you're going too fast!");
	}

	private void Move(ref float movementAmount)
	{
		if (remainingDist > 0.0f)	// Keep moving forwards.
		{
			Vector3 towardsTarget = target - transform.position;
			float actualMovement = Mathf.Min(movementAmount, towardsTarget.magnitude);

			// Move forwards.
			if (actualMovement < movementAmount)	// Snap to target if it's closer than the distance we're trying to move.
			{
				transform.position = target;
				remainingDist = 0.0f;
			}
			else
			{
				transform.Translate(towardsTarget.normalized * movementAmount, Space.World);
				remainingDist -= actualMovement;
			}
			movementAmount -= actualMovement;
		}

		if (movementAmount > 0.0f && remainingTurnDist > 0.0f)	// Keep turning.
		{
			float actualMovement = Mathf.Min(movementAmount, remainingTurnDist);
			remainingTurnDist -= actualMovement;
			
			if (nextFacing == Nav.opposite[facing])		// U-turn.
			{
				float turnLength = U_TURN_ANGLE * turnDistance;

				// Calculate position in the 180 degree turn.
				float turnPhase = (turnLength - remainingTurnDist) / turnLength;
				float angle = turnPhase * U_TURN_ANGLE;

				// Update rotation.
				transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing) - (angle * Mathf.Rad2Deg), 0.0f);
			}
			else	// Right angle turn.
			{
				float turnLength = RIGHT_ANGLE_TURN_ANGLE * turnDistance;

				// Calculate position in the 90 degree turn.
				float turnPhase = (turnLength - remainingTurnDist) / turnLength;
				float angle = turnPhase * RIGHT_ANGLE_TURN_ANGLE;

				// Calculate movement along the turn.
				float forwards = Mathf.Sin(angle) * turnDistance;
				float sidewards = 1.0f - Mathf.Cos(angle) * turnDistance;
				float xMovement = Utils.NonZero(Nav.DX[facing] * forwards, Nav.DX[nextFacing] * sidewards);
				float yMovement = Utils.NonZero(Nav.DY[facing] * forwards, Nav.DY[nextFacing] * sidewards);
				Vector3 turnDelta = new Vector3(yMovement, 0.0f, xMovement);

				// Adjust the position for the turn.
				Vector3 turnAdjust = new Vector3(Nav.DY[facing] * turnDistance, 0.0f, Nav.DX[facing] * turnDistance);

				// Update position on the turn.
				transform.position = target + turnDelta - turnAdjust;

				// Update rotation.
				int turnDir = 0;
				if (nextFacing == Nav.left[facing])
					turnDir = -1;
				else if (nextFacing == Nav.right[facing])
					turnDir = 1;
				transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing) + (angle * Mathf.Rad2Deg) * turnDir, 0.0f);
			}

			movementAmount -= actualMovement;
		}
	}

	public void SetTarget(Vector3 target, Dir facing)
	{
		this.target = target;
		this.facing = facing;
		nextTarget = null;
		
		CalculateMoveDistance();
	}

    /// <summary>
    /// Set a new target world position to move towards.
    /// </summary>
	private void NewTarget()
	{
		// If there's no next target, calculate one.
		if (nextTarget == null)
			nextTarget = maze.RoomToWorldPosition(maze.MoveLeftmost(maze.WorldToRoomPosition(target), facing, out nextFacing));

		target = (Vector3)nextTarget;
		facing = nextFacing;

		nextTarget = maze.RoomToWorldPosition(maze.MoveLeftmost(maze.WorldToRoomPosition(target), facing, out nextFacing));

		CalculateMoveDistance();
	}

	private void CalculateMoveDistance()
	{
		Vector3 targetDelta = transform.position - target;
		if (facing != nextFacing)
		{
			if (nextFacing == Nav.opposite[facing])		// U-turn.
			{
				remainingDist += targetDelta.magnitude;
				remainingTurnDist = U_TURN_ANGLE * turnDistance;
			}
			else	// Right angle turn.
			{
				remainingDist += targetDelta.magnitude - turnDistance;
				remainingTurnDist = RIGHT_ANGLE_TURN_ANGLE * turnDistance;
			}
		}
		else
		{
			remainingDist += targetDelta.magnitude;
		}
	}

	public void Reset()
	{
        // Reset the player.
		target = new Vector3();
		nextTarget = new Vector3();
		remainingDist = 0.0f;
		remainingTurnDist = 0.0f;
		transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing), 0.0f);
		NewTarget();
	}
}
