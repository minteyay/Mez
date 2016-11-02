using UnityEngine;
using System.Collections.Generic;

public class Room
{
	public static Dictionary<Dir, int> bits = new Dictionary<Dir, int>()
	{ { Dir.N, 1 }, { Dir.E, 2 }, { Dir.S, 4 }, { Dir.W, 8 } };
	public static Dictionary<Dir, int> oppositeBits = new Dictionary<Dir, int>()
	{ { Dir.N, 4 }, { Dir.E, 8 }, { Dir.S, 1 }, { Dir.W, 2 } };

	public Room(int value, GameObject instance)
	{
		this.value = value;
		this.instance = instance;
	}
	public int value = 1;
	public GameObject instance = null;
}
