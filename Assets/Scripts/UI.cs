using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    private GameManager _gameManager = null;

    [SerializeField] private Dropdown _themeDropdown = null;
    [SerializeField] private Image _busyBlocker = null;

    private void Awake()
    {
        _gameManager = GameManager.instance;

        _busyBlocker.enabled = false;
    }

    private void Start()
    {
        _themeDropdown.AddOptions(_gameManager.themeManager.themeNames);
    }

    public void ThemeChanged(System.Int32 index)
    {
        string themeName = _themeDropdown.options[index].text;
        _busyBlocker.enabled = true;
        _gameManager.themeManager.LoadTheme(themeName, () => { _busyBlocker.enabled = false; } );
    }
}
