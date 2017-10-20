using System;
using System.Collections.Generic;

/// <summary>
/// Class containing a mishmash of different utility functions.
/// </summary>
public class Utils
{
    /// <summary>
    /// Shuffles a list.
    /// </summary>
    public static void Shuffle<T>(System.Random rnd, IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Makes a pretty string representation of a list.
    /// </summary>
    public static string ListToString<T>(IList<T> list)
    {
        string output = "";
        for (int i = 0; i < list.Count; i++)
        {
            output += list[i].ToString();
            if (i < list.Count - 1)
                output += ", ";
        }
        return output;
    }

    /// <summary>
    /// Formats a 2D integer grid neatly.
    /// </summary>
	public static string Format2DGrid(int[,] grid)
	{
		string output = "";
		for (int y = 0; y < grid.GetLength(0); y++)
		{
			for (int x = 0; x < grid.GetLength(1); x++)
			{
				output += grid[y, x];
				if (x < grid.GetLength(1) - 1)
					output += ", ";
			}
			output += "\n";
		}
		return output;
	}

    /// <summary>
    /// Returns the first non-zero value given.
    /// </summary>
    public static float NonZero(params float[] values)
    {
        foreach (float v in values)
            if (v != 0.0f)
                return v;
        return 0.0f;
    }

    /// <summary>
    /// Returns the first non-zero value given.
    /// </summary>
    public static int NonZero(params int[] values)
    {
        foreach (int v in values)
            if (v != 0)
                return v;
        return 0;
    }

    /// <summary>
    /// Parses a file name from a path by stripping it of the folder structure and file extension.
    /// </summary>
    public static string ParseFileName(string path)
    {
        int begin = path.LastIndexOf('/');
        if (begin == -1)
            begin = 0;
        else
            begin++;
        int end = path.LastIndexOf('.');
        if (end == -1)
            end = path.Length;
        return path.Substring(begin, end - begin);
    }
}
