using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);

    public RoomStyle[] roomStyles;
    public RoomRuleset[] rooms;

    public void SetName(string newName, ThemeManager themeManager)
    {
        // TODO: Utils function for checking duplicate names in a list and generating a unique name.
        foreach (string themeName in themeManager.themeNames)
            if (themeName != name && themeName == newName)
                newName += " (Copy)";
        name = newName;
    }

    public void SetSize(Point newSize)
    {
        newSize.Set(Mathf.Max(newSize.x, 1), Mathf.Max(newSize.y, 1));
        size = newSize;
    }

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

    public void SetName(string newName, MazeRuleset mazeRuleset)
    {
        // TODO: Utils function for checking duplicate names in a list and generating a unique name.
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
            if (roomStyle != this && roomStyle.name == newName)
                newName += " (Copy)";
        name = newName;
    }

    public void SetTileset(string newTileset, ThemeManager themeManager)
    {
        if (!themeManager.textures.ContainsKey(newTileset))
            tileset = "default";
        else
            tileset = newTileset;
    }

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
    public bool TryParseCount(out Range countRange) { return Utils.TryParseRange(amount, out countRange); }

    public void SetTexture(string newTexture, ThemeManager themeManager)
    {
        if (!themeManager.textures.ContainsKey(newTexture))
            texture = "default";
        else
            texture = newTexture;
    }

    public void SetAmountType(AmountType newAmountType)
    {
        amountType = newAmountType;
        SetAmount(amount);
    }

    public void SetAmount(string newAmount)
    {
        amount = newAmount;
        switch (amountType)
        {
            case AmountType.Chance:
                float chance;
                if (!TryParseChance(out chance))
                {
                    amount = 0.0f.ToString();
                    break;
                }
                amount = Mathf.Max(0.0f, Mathf.Min(chance, 100.0f)).ToString();
                break;
            case AmountType.Count:
                Range countRange;
                if (!TryParseCount(out countRange))
                {
                    amount = 0.ToString();
                    break;
                }
                countRange.Set(Mathf.Max(countRange.x, 0), Mathf.Max(countRange.y, 0));
                amount = countRange.ToString();
                break;
        }
    }

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
                Range countRange;
                if (!TryParseCount(out countRange))
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

    public bool TryParseCount(out Range countRange) { return Utils.TryParseRange(count, out countRange); }
    public bool TryParseSize(out Range sizeRange) { return Utils.TryParseRange(size, out sizeRange); }

    public void SetStyle(string newStyle, MazeRuleset mazeRuleset)
    {
        bool styleExists = false;
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
        {
            if (roomStyle.name == newStyle)
            {
                styleExists = true;
                break;
            }
        }
        if (!styleExists)
            style = "default";
        else
            style = newStyle;
    }

    public void SetCount(string newCount)
    {
        count = newCount;
        Range countRange;
        if (!TryParseCount(out countRange))
        {
            count = 0.ToString();
            return;
        }
        countRange.Set(Mathf.Max(countRange.x, 0), Mathf.Max(countRange.y, 0));
        count = countRange.ToString();
    }

    public void SetSize(string newSize)
    {
        size = newSize;
        Range sizeRange;
        if (!TryParseSize(out sizeRange))
        {
            size = 0.ToString();
        }
        sizeRange.Set(Mathf.Max(sizeRange.x, 0), Mathf.Max(sizeRange.y, 0));
        size = sizeRange.ToString();
    }

    public bool Validate(MazeRuleset mazeRuleset)
    {
        bool validStyle = false;
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
            if (roomStyle.name == style)
                validStyle = true;
        if (!validStyle)
            return false;
        Range range;
        if (!TryParseCount(out range))
            return false;
        if (!TryParseSize(out range))
            return false;
        return true;
    }
}