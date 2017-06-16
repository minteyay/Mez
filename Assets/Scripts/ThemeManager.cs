using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
	public delegate void LoadingComplete();

	private const string tilesetPath = "/Textures/Tilesets/";

	public List<string> TilesetNames { get; private set; }
	public Dictionary<string, Texture2D> Tilesets { get; private set; }

	private int tilesetsLoaded = 0;
	private int tilesetsToLoad = 0;

	public void Awake()
	{
		TilesetNames = new List<string>();
		Tilesets = new Dictionary<string, Texture2D>();

		// Enumerate tilesets.
		string[] tilesets = System.IO.Directory.GetFiles(Application.dataPath + tilesetPath, "*.png");
		foreach (string s in tilesets)
		{
			// Only store the tileset's name.
			string tilesetName = s.Substring(s.LastIndexOf('/') + 1, s.LastIndexOf(".png") - s.LastIndexOf('/') - 1);
			TilesetNames.Add(tilesetName);
		}
	}

	public void LoadTilesets(string[] tilesetNames, LoadingComplete callback)
	{
		if (tilesetsToLoad != 0)
		{
			Debug.LogError("Already loading a batch of tilesets, wait for it to finish loading before starting another!");
			return;
		}
		tilesetsLoaded = 0;
		tilesetsToLoad = tilesetNames.Length;
		foreach (string tilesetName in tilesetNames)
			LoadTileset(tilesetName, () => TilesetLoaded(callback));
	}

	private void TilesetLoaded(LoadingComplete callback)
	{
		tilesetsLoaded++;
		if (tilesetsLoaded < tilesetsToLoad)
			return;
		if (callback != null)
			callback();
		tilesetsToLoad = 0;
	}

	public void LoadTileset(string tilesetName, LoadingComplete callback)
	{
		StartCoroutine(DoLoadTileset(tilesetName, callback));
	}

	private IEnumerator<WWW> DoLoadTileset(string tilesetName, LoadingComplete callback)
	{
		WWW www = new WWW("file://" + Application.dataPath + tilesetPath + tilesetName + ".png");
		yield return www;
		Tilesets.Add(tilesetName, www.texture);
		if (callback != null)
			callback();
	}
}