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
    public static void Shuffle<T>(Random rnd, IList<T> list)
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
}
