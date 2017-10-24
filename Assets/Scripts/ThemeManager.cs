using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// An object for loading in and holding assets (ruleset and textures) for themes.
/// </summary>
public class ThemeManager : MonoBehaviour
{
	private const string ThemePath = "/Themes/";

	public List<string> themeNames { get; private set; }
	public MazeRuleset ruleset { get; private set; }
	public Dictionary<string, Texture2D> textures { get; private set; }

	public delegate void LoadingComplete();
	private LoadingComplete _callback = null;
	private bool _rulesetLoaded = false;
	private int _texturesLoaded = 0;
	private int _texturesToLoad = 0;

	[SerializeField] private Texture2D _defaultTexture = null;
	public Texture2D defaultTexture { get; private set; }

	public void Awake()
	{
		themeNames = new List<string>();
		ruleset = null;
		textures = new Dictionary<string, Texture2D>();

		defaultTexture = _defaultTexture;

		// Enumerate themes.
		string[] themes = System.IO.Directory.GetDirectories(Application.dataPath + ThemePath);
		foreach (string s in themes)
		{
			// Only store the theme's name.
			string themeName = s.Substring(s.LastIndexOf('/') + 1);
			themeNames.Add(themeName);
		}
	}

	/// <summary>
	/// Loads the assets for a theme asynchronously.
	/// </summary>
	public void LoadTheme(string themeName, LoadingComplete callback)
	{
		_callback = callback;

		ruleset = null;
		textures.Clear();

		LoadThemeRuleset(themeName);
		LoadThemeTextures(themeName);
	}

	private void UpdateLoadingState()
	{
		if (!_rulesetLoaded) return;
		if (_texturesLoaded < _texturesToLoad) return;

		if (_callback != null)
			_callback.Invoke();
	}

	private void LoadThemeRuleset(string themeName)
	{
		_rulesetLoaded = false;
		
		string rulesetPath = Application.dataPath + "/Themes/" + themeName + "/" + themeName + ".json";
		if (!System.IO.File.Exists(rulesetPath))
		{
			Debug.LogWarning("Trying to load ruleset \"" + rulesetPath + "\" which doesn't exist!");
			_callback();
			return;
		}

		StartCoroutine(DoLoadThemeRuleset(rulesetPath));
	}

	private IEnumerator<WWW> DoLoadThemeRuleset(string rulesetPath)
	{
		WWW www = new WWW("file://" + rulesetPath);
		yield return www;

		ruleset = MazeRuleset.FromJSON(www.text);

		_rulesetLoaded = true;
		UpdateLoadingState();
	}

	private void LoadThemeTextures(string themeName)
	{
		string[] texturePaths = System.IO.Directory.GetFiles(Application.dataPath + "/Themes/" + themeName, "*.png");
		_texturesToLoad = texturePaths.Length;
		_texturesLoaded = 0;

		foreach (string path in texturePaths)
			LoadTexture(path, () => { _texturesLoaded++; UpdateLoadingState(); } );
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

		Texture2D texture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
		texture.anisoLevel = 0;
		texture.filterMode = FilterMode.Point;
		www.LoadImageIntoTexture(texture);

	#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Turn Windows' backslashes into nice regular slashes.
		path = path.Replace('\\', '/');
	#endif
		string textureName = Utils.ParseFileName(path);

		textures.Add(textureName, texture);

		if (callback != null)
			callback();
	}
}