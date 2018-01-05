using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);

    public RoomStyle[] roomStyles;
    public RoomRuleset[] rooms;

    public void SetName(string newName)
    {
        if (newName.Length <= 0)
            return;
        name = newName;
    }

    public void SetSize(Point newSize)
    {
        newSize.Set(Mathf.Max(newSize.x, 1), Mathf.Max(newSize.y, 1));
        size = newSize;
    }

    public void Validate(ThemeManager themeManager)
    {
        SetName(name);
        SetSize(size);

        if (roomStyles != null)
        foreach (RoomStyle roomStyle in roomStyles)
            roomStyle.Validate(this, themeManager);
        if (rooms != null)
        foreach (RoomRuleset room in rooms)
            room.Validate(this);
    }
}

[System.Serializable]
public class RoomStyle
{
    public string name = "";
    public string tileset = "default";

    public DecorationRuleset[] decorations;
    public FlavourTileRuleset[] flavourTiles;

    public void SetName(string newName, MazeRuleset mazeRuleset)
    {
        if (newName.Length <= 0)
            newName = "default";
        string[] roomStyleNames = new string[mazeRuleset.roomStyles.Length];
        for (int i = 0; i < roomStyleNames.Length; i++)
            roomStyleNames[i] = mazeRuleset.roomStyles[i].name;
        name = Utils.MakeUniqueName(name, newName, roomStyleNames);
    }

    public void SetTileset(string newTileset, ThemeManager themeManager)
    {
        if (!themeManager.textures.ContainsKey(newTileset))
            tileset = "default";
        else
            tileset = newTileset;
    }

    public void Validate(MazeRuleset mazeRuleset, ThemeManager themeManager)
    {
        SetName(name, mazeRuleset);
        SetTileset(tileset, themeManager);

        if (decorations != null)
        foreach (DecorationRuleset decoration in decorations)
            decoration.Validate(themeManager);
        
        if (flavourTiles != null)
        foreach (FlavourTileRuleset flavourTile in flavourTiles)
            flavourTile.Validate(themeManager);
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
    public TileLocationRule validLocations = new TileLocationRule();

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

    public void Validate(ThemeManager themeManager)
    {
        SetTexture(texture, themeManager);
        SetAmount(amount);
    }
}

[System.Serializable]
public class FlavourTileRuleset
{
    public enum Type { Single, Tile }
    public enum Location:byte { Floor = 1, Wall = 2, Ceiling = 4 }
    public enum AmountType { Chance, Count }

    public Type type = Type.Single;
    public string texture = "";
    public byte location = 0;
    public AmountType amountType = AmountType.Chance;
    public string amount = "";
    public TileLocationRule validLocations = new TileLocationRule();

    public bool TryParseChance(out float chance) { return float.TryParse(amount, out chance); }
    public bool TryParseCount(out Range countRange) { return Utils.TryParseRange(amount, out countRange); }

    public void SetType(Type newType)
    {
        type = newType;
        SetLocation(location);
    }
    
    public void SetTexture(string newTexture, ThemeManager themeManager)
    {
        if (!themeManager.textures.ContainsKey(newTexture))
            texture = "default";
        else
            texture = newTexture;
    }

    public void SetLocation(byte newLocation)
    {
        switch (type)
        {
            case Type.Single:
                if (!System.Enum.IsDefined(typeof(Location), newLocation))
                    location = (byte)Location.Floor;
                else
                    location = newLocation;
                break;
            case Type.Tile:
                location = (byte)Mathf.Min(newLocation, 7);
                break;
        }
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

    public void Validate(ThemeManager themeManager)
    {
        SetTexture(texture, themeManager);
        SetLocation(location);
        SetAmount(amount);
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
        {
            if (mazeRuleset.roomStyles.Length <= 0)
                Debug.LogError("There are no RoomStyles for the Room to use.");
            else
                style = mazeRuleset.roomStyles[0].name;
        }
        else
        {
            style = newStyle;
        }
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

    public void Validate(MazeRuleset mazeRuleset)
    {
        SetStyle(style, mazeRuleset);
        SetCount(count);
        SetSize(size);
    }
}

[System.Serializable]
public class TileLocationRule
{
    public enum Option : uint
    {
        TileO           = 1,
        TileU           = 2,
        TileI           = 4,
        TileL           = 8,
        TileT           = 16,
        TileX           = 32,
        TileGraphicalO  = 64,
        TileGraphicalU  = 128,
        TileGraphicalI  = 256,
        TileGraphicalL  = 512,
        TileGraphicalT  = 1024,
        TileGraphicalX  = 2048,
        Entrance        = 4096,
        Exit            = 8192
    }

    public uint value = uint.MaxValue;
}