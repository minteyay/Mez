using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
	public delegate void LoadingComplete();

	private const string themePath = "/Themes/";

	public List<string> ThemeNames { get; private set; }
	public Dictionary<string, Material> Tilesets { get; private set; }

	private int tilesetsLoaded = 0;
	private int tilesetsToLoad = 0;

	public Shader defaultShader = null;

	public void Awake()
	{
		ThemeNames = new List<string>();
		Tilesets = new Dictionary<string, Material>();

		// Enumerate themes.
		string[] themes = System.IO.Directory.GetDirectories(Application.dataPath + themePath);
		foreach (string s in themes)
		{
			// Only store the theme's name.
			string themeName = s.Substring(s.LastIndexOf('/') + 1);
			ThemeNames.Add(themeName);
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
		string tilesetPath = Application.dataPath + themePath + tilesetName + "/" + tilesetName + ".png";
		if (!System.IO.File.Exists(tilesetPath))
		{
			Debug.LogWarning("Trying to load tileset " + tilesetPath + " which doesn't exist!");
			callback();
			return;
		}

		StartCoroutine(DoLoadTileset(tilesetName, callback));
	}

	private IEnumerator<WWW> DoLoadTileset(string tilesetName, LoadingComplete callback)
	{
		WWW www = new WWW("file://" + Application.dataPath + themePath + tilesetName + "/" + tilesetName + ".png");
		yield return www;

		Texture2D tilesetTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false, false);
		tilesetTexture.anisoLevel = 0;
		tilesetTexture.filterMode = FilterMode.Point;
		www.LoadImageIntoTexture(tilesetTexture);

		Material tilesetMaterial = new Material(defaultShader);
		tilesetMaterial.mainTexture = tilesetTexture;
		Tilesets.Add(tilesetName, tilesetMaterial);

		if (callback != null)
			callback();
	}
}