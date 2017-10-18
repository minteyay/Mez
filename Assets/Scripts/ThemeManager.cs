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

	private LoadingComplete callback = null;
	private bool rulesetLoaded = false;
	private int tilesetsLoaded = 0;
	private int tilesetsToLoad = 0;

	[SerializeField] private Shader defaultShader = null;
	[SerializeField] private Material defaultMaterial = null;

	public void Awake()
	{
		ThemeNames = new List<string>();
		Rulesets = new Dictionary<string, MazeRuleset>();
		Tilesets = new Dictionary<string, Material>();

		// Add the default material to the tileset dictionary.
		Tilesets.Add("default", defaultMaterial);

		// Enumerate themes.
		string[] themes = System.IO.Directory.GetDirectories(Application.dataPath + themePath);
		foreach (string s in themes)
		{
			// Only store the theme's name.
			string themeName = s.Substring(s.LastIndexOf('/') + 1);
			ThemeNames.Add(themeName);
		}
	}

	public void LoadTheme(string themeName, LoadingComplete callback)
	{
		this.callback = callback;

		LoadThemeRuleset(themeName);
		LoadThemeTilesets(themeName);
	}

	private void UpdateLoadingState()
	{
		if (!rulesetLoaded) return;
		if (tilesetsLoaded < tilesetsToLoad) return;

		if (callback != null)
			callback.Invoke();
	}

	private void LoadThemeRuleset(string themeName)
	{
		rulesetLoaded = false;
		
		string rulesetPath = Application.dataPath + "/Themes/" + themeName + "/" + themeName + ".json";
		if (!System.IO.File.Exists(rulesetPath))
		{
			Debug.LogWarning("Trying to load ruleset \"" + rulesetPath + "\" which doesn't exist!");
			callback();
			return;
		}

		StartCoroutine(DoLoadThemeRuleset(rulesetPath));
	}

	private IEnumerator<WWW> DoLoadThemeRuleset(string rulesetPath)
	{
		WWW www = new WWW("file://" + rulesetPath);
		yield return www;

		MazeRuleset ruleset = MazeRuleset.FromJSON(www.text);
		string rulesetName = rulesetPath.Substring(rulesetPath.LastIndexOf('/') + 1, rulesetPath.LastIndexOf(".json") - rulesetPath.LastIndexOf('/') - 1);
		Rulesets.Add(rulesetName, ruleset);

		rulesetLoaded = true;
		UpdateLoadingState();
	}

	private void LoadThemeTilesets(string themeName)
	{
		string[] tilesetPaths = System.IO.Directory.GetFiles(Application.dataPath + "/Themes/" + themeName, "*.png");
		tilesetsToLoad = tilesetPaths.Length;
		tilesetsLoaded = 0;
		foreach (string path in tilesetPaths)
			LoadTileset(path, () => { tilesetsLoaded++; UpdateLoadingState(); } );
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

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Turn Windows' backslashes into nice regular slashes.
		tilesetPath = tilesetPath.Replace('\\', '/');
	#endif

		string tilesetName = tilesetPath.Substring(tilesetPath.LastIndexOf('/') + 1, tilesetPath.LastIndexOf(".png") - tilesetPath.LastIndexOf('/') - 1);
		Tilesets.Add(tilesetName, tilesetMaterial);

		if (callback != null)
			callback();
	}
}