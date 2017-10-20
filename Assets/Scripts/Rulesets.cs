using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);
    public string defaultTileset = "default";

    public RoomStyle[] roomStyles;
    public RoomRuleset[] rooms;

    public static MazeRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<MazeRuleset>(data);
    }
}

[System.Serializable]
public class RoomStyle
{
    public string name = "";
    public string tileset = "";
}

[System.Serializable]
public class RoomRuleset
{
    public enum Start { Random, Start, End }

    public string style = "";
    public Start start = Start.Random;
    public uint count = 1;
    public uint size = 1;

    public static RoomRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<RoomRuleset>(data);
    }

    public override string ToString()
    {
        return "Style : " + style + '\n'
         + "Start : " + start.ToString() + '\n'
         + "Count : " + count.ToString() + '\n'
         + "Size : " + size.ToString();
    }
}