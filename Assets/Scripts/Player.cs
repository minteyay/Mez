using System;
using UnityEngine;

public class Player : MonoBehaviour
{
	public float movementSpeed = 0.0f;
	public float accelDecelTime = 0.0f;
	public float rotationSpeed = 0.0f;

	public Vector3 target;

	[HideInInspector]
	public bool canMove = false;

	private enum State
	{
		IDLE,
		ROTATING,
		MOVING
	}
	private State state = State.IDLE;

	private float moveTreshold = 0.001f;
	private float rotateTreshold = 0.01f;

	private enum MoveState
	{
		ACCEL,
		MOVE,
		DECEL
	}
	private MoveState _moveState = MoveState.ACCEL;
	private MoveState moveState { get { return _moveState; } set { _moveState = value; moveStateChanged = true; } }
	private bool moveStateChanged = false;

	private float accelDecelDist = 0.0f;
	private float curMoveSpeed = 0.0f;

	private bool rotating = false;

	[HideInInspector]
	public Maze maze = null;

	void Start()
	{
		target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));

		moveState = MoveState.ACCEL;
	}

	void Update()
	{
		if (canMove)
		{
			Vector3 delta = transform.position - target;
			float deltaDist = delta.magnitude;

			if (state == State.IDLE)
			{
				if (deltaDist > moveTreshold)
					state = State.ROTATING;
			}
			else
			{
				float targetAngle = 0.0f;
				if (delta.magnitude != 0.0f)
					targetAngle = Quaternion.LookRotation(delta, transform.up).eulerAngles.y;
				float angleDelta = targetAngle - transform.rotation.eulerAngles.y;

				if (state == State.ROTATING)
				{
					if (!rotating)
					{
						if (Mathf.Abs(angleDelta) > rotateTreshold)
						{
							if (angleDelta == 180.0f)
								transform.Rotate(0f, -0.1f, 0f);

							rotating = true;
							iTween.RotateTo(gameObject, iTween.Hash("rotation", new Vector3(0f, targetAngle, 0f), "speed", rotationSpeed, "oncomplete", "RotationComplete", "easetype", iTween.EaseType.easeInOutSine));
						}
						else
						{
							state = State.MOVING;
						}
					}
					//if (angleDelta < -180.0f)
					//	angleDelta += 360.0f;
					//if (angleDelta > 180.0f)
					//	angleDelta -= 360.0f;
					//if (angleDelta == 180.0f)
					//	angleDelta = -180.0f;

					//if (Mathf.Abs(angleDelta) < rotateTreshold)
					//{
					//	state = State.MOVING;
					//}
					//else if (Mathf.Abs(angleDelta) <= rotationSpeed * Time.deltaTime)
					//{
					//	transform.eulerAngles = new Vector3(0.0f, targetAngle, 0.0f);
					//	state = State.MOVING;
					//}
					//else
					//{
					//	transform.Rotate(0.0f, Mathf.Sign(angleDelta) * rotationSpeed * Time.deltaTime, 0.0f);
					//}
				}
				if (state == State.MOVING)
				{
					if (Mathf.Abs(angleDelta) > rotateTreshold)
					{
						state = State.ROTATING;
					}

					//if (moveState == MoveState.ACCEL && moveStateChanged)
					//{
					//	target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));
					//	iTween.ValueTo(gameObject, iTween.Hash("from", 0.0f, "to", movementSpeed, "time", accelDecelTime, "onupdate", "SpeedChanged", "oncomplete", "AccelComplete"));
					//	moveStateChanged = false;
					//}

					//if (curMoveSpeed > 0.0f)
					//{
					//	transform.position += -delta.normalized * Time.deltaTime * curMoveSpeed;
					//	if (moveState == MoveState.MOVE)
					//	{
					//		float decelDist = (movementSpeed / 2.0f) * accelDecelTime;
					//		if (deltaDist <= decelDist)
					//		{
					//			moveState = MoveState.DECEL;
					//		}
					//	}
					//}

					//if (moveState == MoveState.DECEL && moveStateChanged)
					//{
					//	iTween.ValueTo(gameObject, iTween.Hash("from", movementSpeed, "to", 0.0f, "time", accelDecelTime, "onupdate", "SpeedChanged", "oncomplete", "DecelComplete"));
					//	moveStateChanged = false;
					//}

					if (deltaDist <= movementSpeed * Time.deltaTime)
					{
						transform.position = target;
						state = State.ROTATING;
						target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));
					}
					else
					{
						transform.position += -delta.normalized * Time.deltaTime * movementSpeed;
					}
				}
			}
		}
	}

	public void SpeedChanged(float newSpeed)
	{
		curMoveSpeed = newSpeed;
	}

	public void AccelComplete()
	{
		moveState = MoveState.MOVE;
	}

	public void DecelComplete()
	{
		transform.position = target;
		state = State.ROTATING;
		moveState = MoveState.ACCEL;
	}

	public void RotationComplete()
	{
		rotating = false;
		state = State.MOVING;
	}

	public void Reset()
	{
		state = State.IDLE;
		target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));
	}
}
