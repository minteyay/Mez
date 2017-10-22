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

    public DecorationRuleset[] decorations;
}

[System.Serializable]
public class DecorationRuleset
{
    public enum Location { Floor, Wall, Ceiling }

    public Location location = Location.Floor;
    public string texture = "";
    public float occurrence = 1.0f;
}

[System.Serializable]
public class RoomRuleset
{
    public enum Start { Random, Start, End }

    public string style = "";
    public Start start = Start.Random;
    public string count = "";
    public string size = "";

    public bool TryParseCount(out Point countRange) { return Utils.TryParseRange(count, out countRange); }
    public bool TryParseSize(out Point sizeRange) { return Utils.TryParseRange(size, out sizeRange); }

    public override string ToString()
    {
        return "Style : " + style + '\n'
         + "Start : " + start.ToString() + '\n'
         + "Count : " + count.ToString() + '\n'
         + "Size : " + size.ToString();
    }
}