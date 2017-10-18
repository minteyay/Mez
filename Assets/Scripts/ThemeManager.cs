using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ThemeManager : MonoBehaviour
{
	public delegate void LoadingComplete();

	private const string themePath = "/Themes/";

	public List<string> ThemeNames { get; private set; }
	public Dictionary<string, MazeRuleset> Rulesets { get; private set; }
	public Dictionary<string, Texture2D> Textures { get; private set; }

	private LoadingComplete callback = null;
	private bool rulesetLoaded = false;
	private int texturesLoaded = 0;
	private int texturesToLoad = 0;

	[SerializeField] private Texture2D defaultTexture = null;

	public void Awake()
	{
		ThemeNames = new List<string>();
		Rulesets = new Dictionary<string, MazeRuleset>();
		Textures = new Dictionary<string, Texture2D>();

		// Add the default texture to the dictionary.
		Textures.Add("default", defaultTexture);

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
		LoadThemeTextures(themeName);
	}

	private void UpdateLoadingState()
	{
		if (!rulesetLoaded) return;
		if (texturesLoaded < texturesToLoad) return;

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
		string rulesetName = Utils.ParseFileName(rulesetPath);
		Rulesets.Add(rulesetName, ruleset);

		rulesetLoaded = true;
		UpdateLoadingState();
	}

	private void LoadThemeTextures(string themeName)
	{
		string[] texturePaths = System.IO.Directory.GetFiles(Application.dataPath + "/Themes/" + themeName, "*.png");
		texturesToLoad = texturePaths.Length;
		texturesLoaded = 0;

		foreach (string path in texturePaths)
			LoadTexture(path, () => { texturesLoaded++; UpdateLoadingState(); } );
	}

	public void LoadTexture(string path, LoadingComplete callback)
	{
		if (!System.IO.File.Exists(path))
		{
			Debug.LogWarning("Trying to load texture \"" + path + "\" which doesn't exist!");
			callback();
			return;
		}

		StartCoroutine(DoLoadTexture(path, callback));
	}

	private IEnumerator<WWW> DoLoadTexture(string path, LoadingComplete callback)
	{
		WWW www = new WWW("file://" + path);
		yield return www;

		Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false, false);
		texture.anisoLevel = 0;
		texture.filterMode = FilterMode.Point;
		www.LoadImageIntoTexture(texture);

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Turn Windows' backslashes into nice regular slashes.
		path = path.Replace('\\', '/');
	#endif
		string textureName = Utils.ParseFileName(path);

		Textures.Add(textureName, texture);

		if (callback != null)
			callback();
	}
}