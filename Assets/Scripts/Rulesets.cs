using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);
    public string tileset = "default";

    public SprawlerRuleset[] sprawlers;

    public static MazeRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<MazeRuleset>(data);
    }
}

[System.Serializable]
public class SprawlerRuleset
{
    public enum Start { Random, Start, End }

    public string name = "";
    public string tileset = "default";
    public Start start = Start.Random;
    public uint count = 1;
    public uint size = 1;

    public static SprawlerRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<SprawlerRuleset>(data);
    }

    public override string ToString()
    {
        return "Name : " + name + '\n'
         + "Tileset : " + tileset + '\n'
         + "Start : " + start.ToString() + '\n'
         + "Count : " + count.ToString() + '\n'
         + "Size : " + size.ToString();
    }
}