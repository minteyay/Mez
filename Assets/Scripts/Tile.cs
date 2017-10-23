using UnityEngine;

public class Tile
{
	public uint value = 0;
    public string theme = "";
    public Point position = null;

	public GameObject instance = null;
	public GameObject floor = null;
	public GameObject ceiling = null;
	public GameObject[] walls = null;

	public Tile(uint value, Point position)
	{
		this.value = value;
        this.position = position;
        instance = new GameObject(value.ToString());
	}

	public void AddDecoration(GameObject decoration)
	{
		decoration.transform.SetParent(instance.transform, false);
	}
}
