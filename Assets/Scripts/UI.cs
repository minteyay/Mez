using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private GameManager _gameManager = null;

    [SerializeField] private GameObject _pauseMenu = null;

    [SerializeField] private GameObject _editorGUI = null;
    [SerializeField] private Dropdown _themeDropdown = null;
    [SerializeField] private GameObject _busyScreen = null;

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
        _editorGUI.SetActive(enabled);
    }

    public void ThemeChanged(System.Int32 index)
    {
        string themeName = _themeDropdown.options[index].text;
        _busyScreen.SetActive(true);
        _gameManager.themeManager.LoadTheme(themeName, () => { _busyScreen.SetActive(false); GenerateMaze(); } );
    }

    public void GenerateMaze()
    {
        _gameManager.GenerateMaze(_gameManager.themeManager.ruleset);
    }

    public void RunMaze()
    {
        SetEditorGUIEnabled(false);
        _gameManager.RunMaze();
    }
}
