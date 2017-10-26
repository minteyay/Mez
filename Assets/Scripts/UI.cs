using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private GameManager _gameManager = null;

    [SerializeField] private GameObject _pauseMenu = null;

    [SerializeField] private GameObject _editorUI = null;
    [SerializeField] private Dropdown _themeDropdown = null;
    [SerializeField] private GameObject _busyScreen = null;

    [SerializeField] private RulesetUI _rulesetUI = null;

    private void Awake()
    {
        _gameManager = GameManager.instance;

        _busyScreen.SetActive(false);
    }

    private void Start()
    {
        _themeDropdown.AddOptions(_gameManager.themeManager.themeNames);
        ThemeChanged(0);
    }

    public void SetPauseMenuEnabled(bool enabled)
    {
        _pauseMenu.SetActive(enabled);
    }

    public void SetEditorGUIEnabled(bool enabled)
    {
        _editorUI.SetActive(enabled);
    }

    public void ThemeChanged(System.Int32 index)
    {
        string themeName = _themeDropdown.options[_themeDropdown.value].text;
        _busyScreen.SetActive(true);
        _gameManager.themeManager.LoadTheme(themeName, ThemeLoaded );
    }

    private void ThemeLoaded()
    {
        _busyScreen.SetActive(false);
        _rulesetUI.LoadThemeRuleset();
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        _busyScreen.SetActive(true);
        _gameManager.GenerateMaze(_gameManager.themeManager.ruleset, () => { _busyScreen.SetActive(false); });
    }
}
