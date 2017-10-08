using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
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

[System.Serializable]
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

    public override string ToString()
    {
        return "Name : " + name + '\n'
         + "Tileset : " + tileset + '\n'
         + "Start : " + start.ToString() + '\n'
         + "Count : " + count.ToString() + '\n'
         + "Size : " + size.ToString();
    }
}