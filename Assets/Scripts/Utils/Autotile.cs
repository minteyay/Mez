using UnityEngine;
using System.Collections.Generic;

class Autotile
{
    public static uint wallTileStartIndex = 0;
    public static uint floorTileStartIndex = 4;
    public static uint ceilingTileStartIndex = 9;

    /// Rotations for tiles indexed by their 4-bit bitwise value.
    public static float[] tileRotations = {
        0.0f, -90.0f, 0.0f, 0.0f, 90.0f, 90.0f, 90.0f, -90.0f,
        180.0f, -90.0f, 0.0f, 180.0f, 180.0f, 90.0f, 0.0f, 0.0f
    };

    /// Tile index offset based on a tile's 4-bit bitwise value.
    public static uint[] fourBitTileIndices = {
        0, 0, 0, 4, 0, 1, 4, 2,
        0, 4, 1, 2, 4, 2, 2, 3
    };

    /// Tile index offset based on a tile's 2-bit bitwise value.
    public static uint[] twoBitTileIndices = {
        3, 2, 0, 1
    };

    /// <summary>
    /// Calculates a normalised (0-1) UV coordinate in a texture based on the index of the desired tile.
    /// Assumes there are 4 x 4 tiles in the texture.
    /// </summary>
    /// <param name="index">Index of the desired tile.</param>
    /// <returns>Normalised (0-1) UV coordinate in a texture.</returns>
    public static Vector2 GetUVOffsetByIndex(uint index)
    {
        return GetUVOffsetByIndex(index, 4, 4);
    }

    /// <summary>
    /// Calculates a normalised (0-1) UV coordinate in a texture based on the index of the desired tile.
    /// </summary>
    /// <param name="index">Index of the desired tile.</param>
    /// <param name="hTiles">Number of horizontal tiles in a texture.</param>
    /// <param name="vTiles">Number of vertical tiles in a texture.</param>
    /// <returns>Normalised (0-1) UV coordinate in a texture.</returns>
    public static Vector2 GetUVOffsetByIndex(uint index, uint hTiles, uint vTiles)
    {
        if (hTiles <= 0 || vTiles <= 0)
        {
            Debug.LogError("hTiles and vTiles have to be > 0 !");
            return new Vector2();
        }
        
        uint hPos = index % hTiles;
        uint vPos = index / vTiles;
        return new Vector2(
            (1.0f / hTiles) * hPos,
            (1.0f / vTiles) * vPos
        );
    }
}