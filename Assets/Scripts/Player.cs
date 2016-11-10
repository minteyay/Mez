using System;
using UnityEngine;

public class Player : MonoBehaviour
{
	public float movementSpeed = 0.0f;
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

	[HideInInspector]
	public Maze maze = null;

	void Start()
	{
		target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));
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
					if (angleDelta < -180.0f)
						angleDelta += 360.0f;
					if (angleDelta > 180.0f)
						angleDelta -= 360.0f;
					if (angleDelta == 180.0f)
						angleDelta = -180.0f;

					if (Mathf.Abs(angleDelta) < rotateTreshold)
					{
						state = State.MOVING;
					}
					else if (Mathf.Abs(angleDelta) <= rotationSpeed * Time.deltaTime)
					{
						transform.eulerAngles = new Vector3(0.0f, targetAngle, 0.0f);
						state = State.MOVING;
					}
					else
					{
						transform.Rotate(0.0f, Mathf.Sign(angleDelta) * rotationSpeed * Time.deltaTime, 0.0f);
					}
				}
				if (state == State.MOVING)
				{
					if (Mathf.Abs(angleDelta) < rotateTreshold)
					{
						state = State.ROTATING;
					}
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

	public void Reset()
	{
		state = State.IDLE;
		target = maze.MoveLeftmost(transform.position, Nav.GetFacing(transform.rotation.eulerAngles.y));
	}
}
