using UnityEngine;

public class Maze : MonoBehaviour
{
	[HideInInspector]
	public Room[,] rooms;
	[HideInInspector]
	public Vector2 roomDim;

	[HideInInspector]
	public Vector3 startRotation;

	[HideInInspector]
	public GameObject endPoint = null;

	public void Initialise(int width, int height, Vector2 roomDim)
	{
		rooms = new Room[height, width];
		this.roomDim = roomDim;
	}

	public void AddRoom(Room room)
	{
		rooms[room.position.y, room.position.x] = room;
		room.instance.transform.parent = transform;
	}

	public void AddItem(Point pos, GameObject item)
	{
		item.transform.SetParent(rooms[pos.y, pos.x].instance.transform, false);
	}

	public void AddEndPoint(Point pos, GameObject endPoint, Quaternion rotation)
	{
		this.endPoint = (GameObject)Instantiate(endPoint,
			new Vector3(pos.y * roomDim.y, 0.0f, pos.x * roomDim.x),
			rotation);
		this.endPoint.name = "End Point";
		this.endPoint.transform.parent = transform;
	}

	public Vector3 MoveLeftmost(Vector3 position, Dir facing)
	{
		Point posIndex = Nav.GetIndexAt(position, roomDim);
		Point newPos = new Point(posIndex);
		if (IsConnected(posIndex, Nav.left[facing]))
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.left[facing]], posIndex.y + Nav.DY[Nav.left[facing]]);
		}
		else if (IsConnected(posIndex, facing))
		{
			newPos.Set(posIndex.x + Nav.DX[facing], posIndex.y + Nav.DY[facing]);
		}
		else if (IsConnected(posIndex, Nav.right[facing]))
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.right[facing]], posIndex.y + Nav.DY[Nav.right[facing]]);
		}
		else
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.opposite[facing]], posIndex.y + Nav.DY[Nav.opposite[facing]]);
		}
		return rooms[newPos.y, newPos.x].instance.transform.position;
	}

	public Vector3 MoveStraight(Vector3 position, Dir facing, bool allowUTurns = true)
	{
		Point posIndex = Nav.GetIndexAt(position, roomDim);
		Point newPos = new Point(posIndex);
		if (IsConnected(posIndex, facing))
		{
			newPos.Set(posIndex.x + Nav.DX[facing], posIndex.y + Nav.DY[facing]);
		}
		else if (IsConnected(posIndex, Nav.left[facing]))
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.left[facing]], posIndex.y + Nav.DY[Nav.left[facing]]);
		}
		else if (IsConnected(posIndex, Nav.right[facing]))
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.right[facing]], posIndex.y + Nav.DY[Nav.right[facing]]);
		}
		else if (allowUTurns)
		{
			newPos.Set(posIndex.x + Nav.DX[Nav.opposite[facing]], posIndex.y + Nav.DY[Nav.opposite[facing]]);
		}
		return rooms[newPos.y, newPos.x].instance.transform.position;
	}

	private bool IsConnected(Point pos, Dir dir)
	{
		Point newPos = new Point(pos.x + Nav.DX[dir], pos.y + Nav.DY[dir]);
		if (newPos.x >= 0 && newPos.x < rooms.GetLength(1) && newPos.y >= 0 && newPos.y < rooms.GetLength(0))
		{
			if (Nav.IsConnected(rooms[newPos.y, newPos.x].value, Nav.opposite[dir]))
				return true;
		}
		return false;
	}
}
