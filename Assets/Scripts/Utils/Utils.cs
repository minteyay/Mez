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
    /// Pushes a value to the end of an array.
    /// Lengthens the array by one element.
    /// </summary>
    public static void PushToArray<T>(ref T[] array, T value)
    {
        int count = 0;
		if (array != null)
			count = array.Length;
		
		T[] newArray = new T[count + 1];
		for (int i = 0; i < count; i++)
			newArray[i] = array[i];
        newArray[count] = value;
		array = newArray;
    }

    /// <summary>
    /// Removes a value from an array at the given index.
    /// Shortens the array by one element.
    /// </summary>
    public static void RemoveAtIndex<T>(ref T[] array, int index)
    {
		if (array == null)
			return;
		
		if ((array.Length - 1) <= 0)
		{
			array = null;
            return;
		}

        T[] newArray = new T[array.Length - 1];
        for (int i = 0; i < array.Length; i++)
        {
            if (i < index)
                newArray[i] = array[i];
            else if (i > index)
                newArray[i - 1] = array[i];
        }
        array = newArray;
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
    /// Swaps the two given variables.
    /// </summary>
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
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

    /// <summary>
    /// Tries to parse an integer range (from-to, separated with a dash) from a string.
    /// If no separator is present, tries to parse a single integer from the string.
    /// </summary>
    public static bool TryParseRange(string input, out Range range)
    {
        int separatorIndex = input.IndexOf('-');
        if (separatorIndex >= 0)
        {
            string left = input.Substring(0, separatorIndex);
            string right = input.Substring(separatorIndex + 1);

            int from, to;
            if (!int.TryParse(left, out from) || !int.TryParse(right, out to))
            {
                range = new Range();
                return false;
            }

            range = new Range(from, to);
            return true;
        }

        int countNum;
        if (!int.TryParse(input, out countNum))
        {
            range = new Range();
            return false;
        }
        range = new Range(countNum, countNum);
        return true;
    }

    public static string MakeUniqueName(string ignoreName, string newName, string[] allNames)
    {
        bool duplicateFound;
        do
        {
            duplicateFound = false;
            foreach (string name in allNames)
            {
                if (name != ignoreName && name == newName)
                {
                    duplicateFound = true;
                    newName += "_copy";
                    break;
                }
            }
        }
        while (duplicateFound);
        return newName;
    }
}
