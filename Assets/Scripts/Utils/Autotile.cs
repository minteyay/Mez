using UnityEngine;

/// <summary>
/// Class containing utilities for matching bitwise values of tiles to their graphical representations in tilesets.
/// </summary>
class Autotile
{
    /// Tile index offset to the start of wall tiles.
    public static uint wallTileStartIndex = 0;
    /// Tile index offset to the start of floor tiles.
    public static uint floorTileStartIndex = 4;
    /// Tile index offset to the start of ceiling tiles.
    public static uint ceilingTileStartIndex = 10;

    /// Rotations for tiles indexed by their 4-bit bitwise value.
    public static float[] tileRotations = {
        0.0f, -90.0f, 0.0f, 180.0f, 90.0f, 90.0f, -90.0f, -90.0f,
        180.0f, 90.0f, 0.0f, 180.0f, 0.0f, 90.0f, 0.0f, 0.0f
    };

    public enum TileType : byte { O = 0, U = 1, I = 2, L = 3, T = 4, X = 5 }

    /// Tile index offset based on a tile's 4-bit bitwise value.
    public static uint[] fourBitTileIndices = {
        0, 1, 1, 3, 1, 2, 3, 4,
        1, 3, 2, 4, 3, 4, 4, 5
    };

    /// Tile index offset based on a tile's 2-bit bitwise value.
    public static uint[] twoBitTileIndices = {
        0, 3, 1, 2
    };

    /// <summary>
    /// Calculates a normalised UV coordinate in a texture based on the index position of the desired tile and the number of tiles in the texture.
    /// </summary>
    public static Vector2 GetUVOffsetByIndex(uint index, uint horizontalTiles = 4, uint verticalTiles = 4)
    {
        if (horizontalTiles <= 0 || verticalTiles <= 0)
        {
            Debug.LogError("hTiles and vTiles have to be > 0 !");
            return new Vector2();
        }
        
        uint hPos = index % horizontalTiles;
        uint vPos = index / verticalTiles;
        return new Vector2(hPos * (1.0f / horizontalTiles), vPos * (1.0f / verticalTiles));
    }

    /// <summary>
    /// Checks if two tiles both have walls facing the given direction.
    /// Does not check for a wall in between the tiles since it doesn't know their positions.
    /// </summary>
    public static bool IsWallConnected(uint tileA, uint tileB, Dir dir)
    {
        return (!Nav.IsConnected(tileA, dir)) && (!Nav.IsConnected(tileB, dir));
    }
}