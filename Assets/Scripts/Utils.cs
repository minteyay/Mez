using System;
using System.Collections.Generic;

public class Utils
{
    public static void Shuffle<T>(Random rnd, IList<T> list)
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
