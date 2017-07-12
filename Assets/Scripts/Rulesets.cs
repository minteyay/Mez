using UnityEngine;
using System.Collections.Generic;
using System;

public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);
    public string tileset = "default";

    public List<CrawlerRuleset> crawlers = new List<CrawlerRuleset>();

    public MazeRuleset(string[][] data)
    {
        for (int block = 0; block < data.GetLength(0); block++)
        {
            KeyValuePair<string, string> kvp = BlockFile.GetKeyValuePair(data[block][0]);

            // Check if the block is for the Maze or for a Crawler.
            if (kvp.Key == "name")
            {
                ParseRulesetBlock(data[block]);
            }
            else if (kvp.Key == "room")
            {
                CrawlerRuleset crawler = new CrawlerRuleset(data[block]);
                crawlers.Add(crawler);
            }
            else
            {
                Debug.LogWarning("Invalid start for a block (" + kvp.Key + "), should be \"name\" or \"room\". Dismissing block.");
                continue;
            }
        }
    }

    private void ParseRulesetBlock(string[] data)
    {
        foreach (string line in data)
        {
            KeyValuePair<string, string> kvp = BlockFile.GetKeyValuePair(line);
            switch (kvp.Key)
            {
                case "name":
                    name = kvp.Value;
                    break;
                case "size":
                    Point newSize;
                    if (!BlockFile.TryParseDimensions(kvp.Value, out newSize))
                    {
                        Debug.LogWarning("Couldn't parse \"" + kvp.Value + "\" into a maze size.");
                        break;
                    }
                    if (newSize.x <= 0 || newSize.y <= 0)
                    {
                        Debug.LogWarning("Maze dimensions must be bigger than 0. Using default maze size.");
                        break;
                    }
                    size = newSize;
                    break;
                case "tileset":
                    tileset = kvp.Value;
                    break;
            }
        }
    }
}

public class CrawlerRuleset
{
    public enum CrawlerStart { Random, Start, End }

    public string name = "";
    public string tileset = "default";
    public CrawlerStart start = CrawlerStart.Random;
    public uint count = 1;
    public uint size = 1;

    public CrawlerRuleset(string[] data)
    {
        foreach (string line in data)
        {
            KeyValuePair<string, string> kvp = BlockFile.GetKeyValuePair(line);
            switch (kvp.Key)
            {
                case "room":
                    name = kvp.Value;
                    break;
                case "tileset":
                    tileset = kvp.Value;
                    break;
                case "start":
                    switch (kvp.Value)
                    {
                        case "random":
                            start = CrawlerStart.Random;
                            break;
                        case "start":
                            start = CrawlerStart.Start;
                            break;
                        case "end":
                            start = CrawlerStart.End;
                            break;
                        default:
                            Debug.LogWarning("\"" + kvp.Value + "\" isn't a valid value for a Crawler's start.");
                            break;
                    }
                    break;
                case "count":
                    uint newCount;
                    if (!UInt32.TryParse(kvp.Value, out newCount))
                    {
                        Debug.LogWarning("Couldn't parse \"" + kvp.Value + "\" into an Int32.");
                        break;
                    }
                    count = newCount;
                    break;
                case "size":
                    uint newSize;
                    if (!UInt32.TryParse(kvp.Value, out newSize))
                    {
                        Debug.LogWarning("Couldn't parse \"" + kvp.Value + "\" into an Int32.");
                        break;
                    }
                    if (newSize == 0)
                    {
                        Debug.LogWarning("Crawler can't have a size of 0. Defaulting to 1.");
                        newSize = 1;
                    }
                    size = newSize;
                    break;
                default:
                    Debug.LogWarning("\"" + kvp.Value + "\" isn't a valid key for a Crawler.");
                    break;
            }
        }
    }
}