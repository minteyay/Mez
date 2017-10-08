using System;
using System.Collections.Generic;

public class Utils
{
    /// <summary>
    /// Shuffles a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rnd">Random number generator to use in shuffling.</param>
    /// <param name="list">List to shuffle.</param>
    public static void Shuffle<T>(System.Random rnd, IList<T> list)
    {
        // Shuffle the list for an amount of times equals to its size.
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
    /// Gets a pretty string representation of a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
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
    /// Formats a 2D grid neatly.
    /// </summary>
    /// <param name="grid"></param>
	private string Format2DGrid(int[,] grid)
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

    public static float NonZero(params float[] values)
    {
        foreach (float v in values)
            if (v != 0.0f)
                return v;
        return 0.0f;
    }

    public static int NonZero(params int[] values)
    {
        foreach (int v in values)
            if (v != 0)
                return v;
        return 0;
    }
}
