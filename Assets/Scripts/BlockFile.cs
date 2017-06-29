using UnityEngine;
using System.Collections.Generic;
using System;

public class BlockFile
{
	public static string[][] GetBlocks(string data)
    {
        string[] lines = data.Split('\n');

        List<string[]> blocks = new List<string[]>();
        List<string> curBlock = new List<string>();
        foreach (string line in lines)
        {
            // The line is either empty or just whitespace.
            if (String.IsNullOrEmpty(line) || line.Trim().Length == 0)
            {
                // If the stored block has lines in it, add it to the list and clear it for the next lines.
                if (curBlock.Count > 0)
                {
                    blocks.Add(curBlock.ToArray());
                    curBlock.Clear();
                }
                continue;
            }
            curBlock.Add(line);
        }

        if (curBlock.Count > 0)
            blocks.Add(curBlock.ToArray());
        
        return blocks.ToArray();
    }

    public static KeyValuePair<string, string> GetKeyValuePair(string line)
    {
        int separatorIndex = line.IndexOf(':');
        string key = line.Substring(0, separatorIndex).Trim();
        string value = line.Substring(separatorIndex + 1).Trim();
        return new KeyValuePair<string, string>(key, value);
    }

    public static bool TryParseDimensions(string input, out Point point)
    {
        int separatorIndex = input.IndexOf('x');

        int leftInt = 0;
        int rightInt = 0;

        string left = input.Substring(0, separatorIndex).Trim();
        string right = input.Substring(separatorIndex + 1).Trim();

        if (!Int32.TryParse(left, out leftInt))
        {
            Debug.LogError("Couldn't parse string \"" + left + "\" into an Int32.");
            point = new Point();
            return false;
        }
        if (!Int32.TryParse(right, out rightInt))
        {
            Debug.LogError("Couldn't parse string \"" + right + "\" into an Int32.");
            point = new Point();
            return false;
        }

        point = new Point(leftInt, rightInt);
        return true;
    }
}
