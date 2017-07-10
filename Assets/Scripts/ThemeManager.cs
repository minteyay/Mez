using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
	public delegate void LoadingComplete();

	private const string themePath = "/Themes/";

	public List<string> ThemeNames { get; private set; }
	public Dictionary<string, MazeRuleset> Rulesets { get; private set; }
	public Dictionary<string, Material> Tilesets { get; private set; }

	private int tilesetsLoaded = 0;
	private int tilesetsToLoad = 0;

	public Shader defaultShader = null;

	public void Awake()
	{
		ThemeNames = new List<string>();
		Rulesets = new Dictionary<string, MazeRuleset>();
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

	public void LoadThemeRuleset(string themeName, LoadingComplete callback)
	{
		string rulesetPath = Application.dataPath + "/Themes/" + themeName + "/" + themeName + ".mez";
		if (!System.IO.File.Exists(rulesetPath))
		{
			Debug.LogWarning("Trying to load tileset \"" + rulesetPath + "\" which doesn't exist!");
			callback();
			return;
		}

		StartCoroutine(DoLoadThemeRuleset(rulesetPath, callback));
	}

	private IEnumerator<WWW> DoLoadThemeRuleset(string rulesetPath, LoadingComplete callback)
	{
		WWW www = new WWW("file://" + rulesetPath);
		yield return www;

		string[][] rulesetData = BlockFile.GetBlocks(www.text);
		MazeRuleset ruleset = new MazeRuleset(rulesetData);
		string rulesetName = rulesetPath.Substring(rulesetPath.LastIndexOf('/') + 1, rulesetPath.LastIndexOf(".mez") - rulesetPath.LastIndexOf('/') - 1);
		Rulesets.Add(rulesetName, ruleset);

		if (callback != null)
			callback();
	}

	public void LoadThemeTilesets(string themeName, LoadingComplete callback)
	{
		if (tilesetsToLoad != 0)
		{
			Debug.LogWarning("Already loading a batch of tilesets, wait for it to finish loading before starting another!");
			return;
		}
		tilesetsLoaded = 0;

		string[] tilesetPaths = System.IO.Directory.GetFiles(Application.dataPath + "/Themes/" + themeName, "*.png");
		tilesetsToLoad = tilesetPaths.Length;
		foreach (string path in tilesetPaths)
			LoadTileset(path, () => TilesetLoaded(callback));
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

	public void LoadTileset(string tilesetPath, LoadingComplete callback)
	{
		if (!System.IO.File.Exists(tilesetPath))
		{
			Debug.LogWarning("Trying to load tileset \"" + tilesetPath + "\" which doesn't exist!");
			callback();
			return;
		}

		StartCoroutine(DoLoadTileset(tilesetPath, callback));
	}

	private IEnumerator<WWW> DoLoadTileset(string tilesetPath, LoadingComplete callback)
	{
		WWW www = new WWW("file://" + tilesetPath);
		yield return www;

		Texture2D tilesetTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false, false);
		tilesetTexture.anisoLevel = 0;
		tilesetTexture.filterMode = FilterMode.Point;
		www.LoadImageIntoTexture(tilesetTexture);

		Material tilesetMaterial = new Material(defaultShader);
		tilesetMaterial.mainTexture = tilesetTexture;

		string tilesetName = tilesetPath.Substring(tilesetPath.LastIndexOf('/') + 1, tilesetPath.LastIndexOf(".png") - tilesetPath.LastIndexOf('/') - 1);
		Tilesets.Add(tilesetName, tilesetMaterial);

		if (callback != null)
			callback();
	}
}