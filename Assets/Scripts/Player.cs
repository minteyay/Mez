using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A player object that moves automatically in a maze.
/// </summary>
public class Player : MonoBehaviour
{
	[SerializeField] private float _movementSpeed = 0.0f;
	[SerializeField] private float _turnRadius = 1.0f;

	[SerializeField] private Vector3 _target;
	[SerializeField] private Vector3 _nextTarget;

	[SerializeField] private Dir _facing;
	public Dir facing { get { return _facing; } set { _facing = _nextFacing = value; } }
	[SerializeField] private Dir _nextFacing;

	private const float RightAngleTurnAngle = Mathf.PI / 2.0f;
	private const float UTurnAngle = Mathf.PI;

	[SerializeField] private float _remainingDist = 0.0f;
	[SerializeField] private float _remainingTurnDist = 0.0f;

	[HideInInspector] public Maze maze = null;

	public delegate void OutOfBoundsCallback();
	[HideInInspector] public OutOfBoundsCallback outOfBoundsCallback = null;

	private void Update()
	{
		// Move the player.
		float toMove = _movementSpeed * Time.deltaTime;
		Move(ref toMove);

		// Get a new target once all the forwards moving and turning have been done.
		if (_remainingDist <= 0.0f && _remainingTurnDist <= 0.0f)
			NewTarget();

		// Move again if there's still distance left to go.
		if (toMove > 0.0f)
			Move(ref toMove);
		
		if (toMove > 0.0f)
			Debug.LogWarning("There's still " + toMove + " to move this frame, you're going too fast!");
	}

	public void SetTargets(Vector3 target, Dir facing, Vector3 nextTarget, Dir nextFacing)
	{
		_target = target;
		this.facing = facing;
		_nextTarget = nextTarget;
		_nextFacing = nextFacing;
		
		CalculateMoveDistance();
	}

	public void Reset()
	{
        // Reset the player.
		_target = new Vector3();
		_nextTarget = new Vector3();
		_remainingDist = 0.0f;
		_remainingTurnDist = 0.0f;
		transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing), 0.0f);
	}

	/// <param name="movementAmount">Amount to try to move. If this isn't zero after the function call, the current target was reached.</param>
	private void Move(ref float movementAmount)
	{
		if (_remainingDist > 0.0f)	// Keep moving forwards.
		{
			Vector3 towardsTarget = _target - transform.position;
			float actualMovement = Mathf.Min(movementAmount, towardsTarget.magnitude);

			// Move forwards.
			if (actualMovement < movementAmount)	// Snap to target if it's closer than the distance we're trying to move.
			{
				transform.position = _target;
				_remainingDist = 0.0f;
			}
			else
			{
				transform.Translate(towardsTarget.normalized * movementAmount, Space.World);
				_remainingDist -= actualMovement;
			}
			movementAmount -= actualMovement;
		}

		if (movementAmount > 0.0f && _remainingTurnDist > 0.0f)	// Keep turning.
		{
			float actualMovement = Mathf.Min(movementAmount, _remainingTurnDist);
			_remainingTurnDist -= actualMovement;
			
			if (_nextFacing == Nav.opposite[facing])		// U-turn.
			{
				float turnLength = UTurnAngle * _turnRadius;

				// Calculate position in the 180 degree turn.
				float turnPhase = (turnLength - _remainingTurnDist) / turnLength;
				float angle = turnPhase * UTurnAngle;

				// Update rotation.
				transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing) - (angle * Mathf.Rad2Deg), 0.0f);
			}
			else	// Right angle turn.
			{
				float turnLength = RightAngleTurnAngle * _turnRadius;

				// Calculate position in the 90 degree turn.
				float turnPhase = (turnLength - _remainingTurnDist) / turnLength;
				float angle = turnPhase * RightAngleTurnAngle;

				// Calculate movement along the turn.
				float forwards = Mathf.Sin(angle) * _turnRadius;
				float sidewards = 1.0f - Mathf.Cos(angle) * _turnRadius;
				float xMovement = Utils.NonZero(Nav.DX[facing] * forwards, Nav.DX[_nextFacing] * sidewards);
				float yMovement = Utils.NonZero(Nav.DY[facing] * forwards, Nav.DY[_nextFacing] * sidewards);
				Vector3 turnDelta = new Vector3(yMovement, 0.0f, xMovement);

				// Adjust the position for the turn.
				Vector3 turnAdjust = new Vector3(Nav.DY[facing] * _turnRadius, 0.0f, Nav.DX[facing] * _turnRadius);

				// Update position on the turn.
				transform.position = _target + turnDelta - turnAdjust;

				// Update rotation.
				int turnDir = 0;
				if (_nextFacing == Nav.left[facing])
					turnDir = -1;
				else if (_nextFacing == Nav.right[facing])
					turnDir = 1;
				transform.rotation = Quaternion.Euler(0.0f, Nav.FacingToAngle(facing) + (angle * Mathf.Rad2Deg) * turnDir, 0.0f);
			}

			movementAmount -= actualMovement;
		}
	}

    /// <summary>
    /// Calculate a new target position to move towards.
    /// </summary>
	private void NewTarget()
	{
		// If the current target falls outside the maze, we're out of bounds.
		if (maze.GetTile(maze.WorldToTilePosition(_target)) == null)
		{
			if (outOfBoundsCallback != null)
				outOfBoundsCallback.Invoke();
			return;
		}

		_target = _nextTarget;
		facing = _nextFacing;

		_nextTarget = maze.TileToWorldPosition(maze.MoveForwards(maze.WorldToTilePosition(_target), facing, Maze.MovementPreference.Leftmost, true));
		if (_nextTarget != _target)
			_nextFacing = Nav.DeltaToFacing(maze.WorldToTilePosition(_nextTarget) - maze.WorldToTilePosition(_target));

		CalculateMoveDistance();
	}

	/// <summary>
	/// Calculate how much to move and how much to turn to reach the current target.
	/// </summary>
	private void CalculateMoveDistance()
	{
		Vector3 targetDelta = transform.position - _target;
		if (facing != _nextFacing)
		{
			if (_nextFacing == Nav.opposite[facing]) // U-turn.
			{
				_remainingDist += targetDelta.magnitude;
				_remainingTurnDist = UTurnAngle * _turnRadius;
			}
			else // Right angle turn.
			{
				_remainingDist += targetDelta.magnitude - _turnRadius;
				_remainingTurnDist = RightAngleTurnAngle * _turnRadius;
			}
		}
		else
		{
			_remainingDist += targetDelta.magnitude;
		}
	}
}
