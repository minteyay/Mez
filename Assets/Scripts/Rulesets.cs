﻿using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);

    public RoomStyle[] roomStyles;
    public RoomRuleset[] rooms;
}

[System.Serializable]
public class RoomStyle
{
    public string name = "default";
    public string tileset = "default";

    public DecorationRuleset[] decorations;
}

[System.Serializable]
public class DecorationRuleset
{
    public enum Location { Floor, Wall, Ceiling }
    public enum AmountType { Chance, Count }

    public string texture = "";
    public Location location = Location.Floor;
    public AmountType amountType = AmountType.Chance;
    public string amount = "";

    public bool TryParseChance(out float chance) { return float.TryParse(amount, out chance); }
    public bool TryParseCount(out Point countRange) { return Utils.TryParseRange(amount, out countRange); }
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
}