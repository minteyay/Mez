using UnityEngine;

public class Maze : MonoBehaviour
{
	private Room[,] rooms;
	private Vector2 roomDim;

	[HideInInspector]
	public Vector3 startRotation;

	[HideInInspector]
	public GameObject endPoint = null;

	public void Initialise(int width, int height, Vector2 roomDim)
	{
		rooms = new Room[height, width];
		this.roomDim = roomDim;
	}

	public void AddRoom(int x, int y, Room room)
	{
		rooms[y, x] = room;
		room.instance.transform.parent = transform;
	}

	public void AddEndPoint(int x, int y, GameObject endPoint)
	{
		this.endPoint = (GameObject)Instantiate(endPoint,
			new Vector3(y * roomDim.y, 0.0f, x * roomDim.x),
			Quaternion.identity);
		this.endPoint.name = "End Point";
		this.endPoint.transform.parent = transform;
	}

	public Vector3 MoveLeftmost(Vector3 position, Dir facing)
	{
		int[] posIndex = GetIndexAt(position);
		int newX = 0;
		int newY = 0;
		if (IsConnected(posIndex[0], posIndex[1], Nav.left[facing]))
		{
			newX = posIndex[0] + Nav.DX[Nav.left[facing]];
			newY = posIndex[1] + Nav.DY[Nav.left[facing]];
		}
		else if (IsConnected(posIndex[0], posIndex[1], facing))
		{
			newX = posIndex[0] + Nav.DX[facing];
			newY = posIndex[1] + Nav.DY[facing];
		}
		else if (IsConnected(posIndex[0], posIndex[1], Nav.right[facing]))
		{
			newX = posIndex[0] + Nav.DX[Nav.right[facing]];
			newY = posIndex[1] + Nav.DY[Nav.right[facing]];
		}
		else
		{
			newX = posIndex[0] + Nav.DX[Nav.opposite[facing]];
			newY = posIndex[1] + Nav.DY[Nav.opposite[facing]];
		}
		return rooms[newY, newX].instance.transform.position;
	}

	private int[] GetIndexAt(Vector3 position)
	{
		return new int[] { Mathf.RoundToInt(position.z / roomDim.x), Mathf.RoundToInt(position.x / roomDim.y) };
	}

	private bool IsConnected(int x, int y, Dir dir)
	{
		int newX = x + Nav.DX[dir];
		int newY = y + Nav.DY[dir];
		if (newX >= 0 && newX < rooms.GetLength(1) && newY >= 0 && newY < rooms.GetLength(0))
		{
			int testRoom = rooms[newY, newX].value;
			if ((testRoom & Room.oppositeBits[dir]) != 0)
				return true;
		}
		return false;
	}
}
