using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
	[SerializeField] private Button _removeRoomStyleButton = null;
	[SerializeField] private GameObject _roomStyleEntryPrefab = null;
	private List<RoomStyleEntry> _roomStyleEntries = new List<RoomStyleEntry>();
	private GameObject _selectedRoomStyle = null;

	[SerializeField] private GameObject _roomList = null;
	[SerializeField] private Button _removeRoomButton = null;
	[SerializeField] private GameObject _roomEntryPrefab = null;
	private List<RoomEntry> _roomEntries = new List<RoomEntry>();
	private GameObject _selectedRoom = null;

	private bool _selectedEntry = false;

	private void Start()
	{
		_gameManager = GameManager.instance;
		_themeManager = _gameManager.themeManager;

		_busyScreen.SetActive(false);

		_themeDropdown.AddOptions(_themeManager.themeNames);
        ThemeChanged(0);
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			if (!_selectedEntry)
				UnselectEntries();
			_selectedEntry = false;
		}
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

		foreach (RoomStyleEntry roomStyleEntry in _roomStyleEntries)
			Destroy(roomStyleEntry.gameObject);
		_roomStyleEntries.Clear();
		
		if (ruleset.roomStyles != null)
		for (int i = 0; i < ruleset.roomStyles.Length; i++)
		{
			GameObject roomStyleEntry = Instantiate(_roomStyleEntryPrefab);
			roomStyleEntry.transform.SetParent(_roomStyleList.transform);

			SelectableEntry roomStyleSelectable = roomStyleEntry.GetComponent<SelectableEntry>();
			roomStyleSelectable.selectEvent.AddListener((data) => { RoomStyleSelected(data.selectedObject); } );

			RoomStyleEntry roomStyleUI = roomStyleEntry.GetComponent<RoomStyleEntry>();
			roomStyleUI.index = i;
			roomStyleUI.roomStyle = ruleset.roomStyles[i];
			roomStyleUI.editorUI = this;
			roomStyleUI.themeManager = _themeManager;
			roomStyleUI.UpdateValues();
			_roomStyleEntries.Add(roomStyleUI);
		}

		foreach (RoomEntry roomEntry in _roomEntries)
			Destroy(roomEntry.gameObject);
		_roomEntries.Clear();

		if (ruleset.rooms != null)
		for (int i = 0; i < ruleset.rooms.Length; i++)
		{
			GameObject roomEntry = Instantiate(_roomEntryPrefab);
			roomEntry.transform.SetParent(_roomList.transform);

			SelectableEntry roomSelectable = roomEntry.GetComponent<SelectableEntry>();
			roomSelectable.selectEvent.AddListener((data) => { RoomSelected(data.selectedObject); } );

			RoomEntry roomUI = roomEntry.GetComponent<RoomEntry>();
			roomUI.index = i;
			roomUI.mazeRuleset = ruleset;
			roomUI.roomRuleset = ruleset.rooms[i];
			roomUI.UpdateValues();
			_roomEntries.Add(roomUI);
		}
	}

	public void SaveRuleset()
	{
		_themeManager.SaveThemeRuleset();
	}

    public void GenerateMaze()
    {
        _busyScreen.SetActive(true);
        _gameManager.GenerateMaze(_themeManager.ruleset, () => { _busyScreen.SetActive(false); });
    }

	public void MazeNameChanged(string newName)
	{
		_themeManager.ruleset.name = newName;
	}

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

	public void AddRoomStyle()
	{
		Utils.PushToArray(ref _themeManager.ruleset.roomStyles, new RoomStyle());
		UnselectEntries();
		UpdateValues();
	}

	public void RemoveRoomStyle()
	{
		Utils.RemoveAtIndex(ref _themeManager.ruleset.roomStyles, _selectedRoomStyle.GetComponent<RoomStyleEntry>().index);
		UnselectEntries();
		UpdateValues();
	}

	public void RoomStyleSelected(GameObject selected)
	{
		UnselectEntries();
		_selectedRoomStyle = selected;
		_removeRoomStyleButton.interactable = true;
		EntrySelected();
	}

	public void AddRoom()
	{
		Utils.PushToArray(ref _themeManager.ruleset.rooms, new RoomRuleset());
		UpdateValues();
	}

	public void RemoveRoom()
	{
		Utils.RemoveAtIndex(ref _themeManager.ruleset.rooms, _selectedRoom.GetComponent<RoomEntry>().index);
		UnselectEntries();
		UpdateValues();
	}

	public void RoomSelected(GameObject selected)
	{
		UnselectEntries();
		_selectedRoom = selected;
		_removeRoomButton.interactable = true;
		EntrySelected();
	}

	public void EntrySelected()
	{
		_selectedEntry = true;
	}

	public void UnselectEntries()
	{
		_selectedEntry = false;
		_selectedRoomStyle = null;
		_removeRoomStyleButton.interactable = false;
		_selectedRoom = null;
		_removeRoomButton.interactable = false;
		foreach (RoomStyleEntry roomStyle in _roomStyleEntries)
			roomStyle.EntryDeselected();
	}
}
