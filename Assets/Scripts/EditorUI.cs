using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditorUI : MonoBehaviour
{
	private GameManager _gameManager = null;
	private ThemeManager _themeManager = null;

	[SerializeField] private GameObject _busyScreen = null;

	[SerializeField] private Dropdown _themeDropdown = null;

	[SerializeField] private InputField _mazeNameField = null;
	[SerializeField] private InputField _mazeWidthField = null;
	[SerializeField] private InputField _mazeHeightField = null;

	[SerializeField] private GameObject _roomStyleList = null;
	[SerializeField] private GameObject _roomStyleEntryPrefab = null;
	private List<RoomStyleUI> _roomStyleEntries = new List<RoomStyleUI>();

	private void Start()
	{
		_gameManager = GameManager.instance;
		_themeManager = _gameManager.themeManager;

		_busyScreen.SetActive(false);

		_themeDropdown.AddOptions(_themeManager.themeNames);
        ThemeChanged(0);
	}

	public void ThemeChanged(System.Int32 index)
    {
        string themeName = _themeDropdown.options[index].text;
        _busyScreen.SetActive(true);
        _themeManager.LoadTheme(themeName, ThemeLoaded);
    }

    private void ThemeLoaded()
    {
        _busyScreen.SetActive(false);
		UpdateValues();
        GenerateMaze();
    }

	public void UpdateValues()
	{
		MazeRuleset ruleset = _themeManager.ruleset;

		_mazeNameField.text = ruleset.name;
		_mazeWidthField.text = ruleset.size.x.ToString();
		_mazeHeightField.text = ruleset.size.y.ToString();

		foreach (RoomStyleUI roomStyleEntry in _roomStyleEntries)
			Destroy(roomStyleEntry.gameObject);
		_roomStyleEntries.Clear();
		
		foreach (RoomStyle roomStyle in ruleset.roomStyles)
		{
			GameObject roomStyleEntry = Instantiate(_roomStyleEntryPrefab);
			roomStyleEntry.transform.SetParent(_roomStyleList.transform);

			RoomStyleUI roomStyleUI = roomStyleEntry.GetComponent<RoomStyleUI>();
			roomStyleUI.roomStyle = roomStyle;
			roomStyleUI.themeManager = _themeManager;
			roomStyleUI.UpdateValues();
			_roomStyleEntries.Add(roomStyleUI);
		}
	}

    public void GenerateMaze()
    {
        _busyScreen.SetActive(true);
        _gameManager.GenerateMaze(_themeManager.ruleset, () => { _busyScreen.SetActive(false); });
    }

	public void MazeNameChanged(string newName) { _themeManager.ruleset.name = newName; }

	public void MazeWidthChanged(string newWidth)
	{
		int width = Mathf.Max(1, int.Parse(newWidth));
		_mazeWidthField.text = width.ToString();
		_themeManager.ruleset.size.x = width;
	}
	public void MazeHeightChanged(string newHeight)
	{
		int height = Mathf.Max(1, int.Parse(newHeight));
		_mazeHeightField.text = height.ToString();
		_themeManager.ruleset.size.y = height;
	}

	public void RoomStyleNameChanged(int index, string newName)
	{
		_themeManager.ruleset.roomStyles[index].name = newName;
	}
}
