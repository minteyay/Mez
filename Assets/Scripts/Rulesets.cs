using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);

    public RoomStyle[] roomStyles;
    public RoomRuleset[] rooms;

    public bool Validate(ThemeManager themeManager)
    {
        if (size.x <= 0 || size.y <= 0)
            return false;
        foreach (RoomStyle roomStyle in roomStyles)
            if (!roomStyle.Validate(this, themeManager))
                return false;
        foreach (RoomRuleset room in rooms)
            if (!room.Validate(this))
                return false;
        return true;
    }
}

[System.Serializable]
public class RoomStyle
{
    public string name = "default";
    public string tileset = "default";

    public DecorationRuleset[] decorations;

    public bool Validate(MazeRuleset mazeRuleset, ThemeManager themeManager)
    {
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
            if (roomStyle != this && roomStyle.name == name)
                return false;
        if (!themeManager.textures.ContainsKey(tileset))
            return false;
        foreach (DecorationRuleset decoration in decorations)
            if (!decoration.Validate(themeManager))
                return false;
        return true;
    }
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

    public bool Validate(ThemeManager themeManager)
    {
        if (!themeManager.textures.ContainsKey(texture))
            return false;
        switch (amountType)
        {
            case AmountType.Chance:
                float chance;
                if (!TryParseChance(out chance))
                    return false;
                break;
            case AmountType.Count:
                Point count;
                if (!TryParseCount(out count))
                    return false;
                break;
        }
        return true;
    }
}

[System.Serializable]
public class RoomRuleset
{
    public enum Start { Random, Start, End }

    public string style = "";
    public Start start = Start.Random;
    public string count = 1.ToString();
    public string size = 1.ToString();

    public bool TryParseCount(out Point countRange) { return Utils.TryParseRange(count, out countRange); }
    public bool TryParseSize(out Point sizeRange) { return Utils.TryParseRange(size, out sizeRange); }

    public bool Validate(MazeRuleset mazeRuleset)
    {
        bool validStyle = false;
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
            if (roomStyle.name == style)
                validStyle = true;
        if (!validStyle)
            return false;
        Point range;
        if (!TryParseCount(out range))
            return false;
        if (!TryParseSize(out range))
            return false;
        return true;
    }
}