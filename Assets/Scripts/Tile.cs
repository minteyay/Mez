using UnityEngine;

public class Tile
{
	public Tile(uint value, Point position)
	{
		this.value = value;
        this.position = position;
        instance = new GameObject(value.ToString());
	}

	public uint value = 0;
    public string theme = "";
    public Point position = null;
	public GameObject instance = null;
}
