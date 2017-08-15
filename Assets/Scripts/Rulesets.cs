using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class MazeRuleset
{
    public string name = "";
    public Point size = new Point(2, 2);
    public string tileset = "default";

    public CrawlerRuleset[] crawlers;

    public static MazeRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<MazeRuleset>(data);
    }
}

[Serializable]
public class CrawlerRuleset
{
    public enum CrawlerStart { Random, Start, End }

    public string name = "";
    public string tileset = "default";
    public CrawlerStart start = CrawlerStart.Random;
    public uint count = 1;
    public uint size = 1;

    public static CrawlerRuleset FromJSON(string data)
    {
        return JsonUtility.FromJson<CrawlerRuleset>(data);
    }
}