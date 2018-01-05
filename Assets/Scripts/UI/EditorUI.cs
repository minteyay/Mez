using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class EditorUI : MonoBehaviour
{
	private GameManager _gameManager = null;
	private ThemeManager _themeManager = null;

	private GameObject _busyScreen = null;

	private Dropdown _themeDropdown = null;

	private InputField _mazeNameField = null;
	private InputField _mazeWidthField = null;
	private InputField _mazeHeightField = null;

	private Transform _roomStyleList = null;
	private Button _removeRoomStyleButton = null;
	[SerializeField] private GameObject _roomStyleEntryPrefab = null;
	private List<RoomStyleEntry> _roomStyleEntries = new List<RoomStyleEntry>();
	private GameObject _selectedRoomStyle = null;

	private Transform _roomList = null;
	private Button _removeRoomButton = null;
	[SerializeField] private GameObject _roomEntryPrefab = null;
	private List<RoomEntry> _roomEntries = new List<RoomEntry>();
	private GameObject _selectedRoom = null;

	private bool _selectedEntry = false;

	[SerializeField] private GameObject _addRulesetModal = null;
	[SerializeField] private InputField _addRulesetNameField = null;

	private void Start()
	{
		_gameManager = GameManager.instance;
		_themeManager = _gameManager.themeManager;

		_busyScreen = transform.Find("BusyScreen").gameObject;

		Transform generationPanelRoot = transform.Find("GenerationPanel");
		_themeDropdown = generationPanelRoot.Find("ThemeDropdown").GetComponent<Dropdown>();
		_themeDropdown.onValueChanged.AddListener(ThemeChanged);
		generationPanelRoot.Find("NewRulesetButton").GetComponent<Button>().onClick.AddListener(ShowAddMazeRulesetModal);
		generationPanelRoot.Find("RunMazeButton").GetComponent<Button>().onClick.AddListener(_gameManager.LoadRunningMazeState);
		generationPanelRoot.Find("GenerateMazeButton").GetComponent<Button>().onClick.AddListener(GenerateMaze);

		Transform mazeSettingsRoot = transform.Find("RulesetPanel").Find("MazeSettings");
		mazeSettingsRoot.Find("Header").Find("SaveButton").GetComponent<Button>().onClick.AddListener(SaveRuleset);
		_mazeNameField = mazeSettingsRoot.Find("MazeName").Find("Value").Find("InputField").GetComponent<InputField>();
		_mazeNameField.onEndEdit.AddListener(MazeNameChanged);
		_mazeWidthField = mazeSettingsRoot.Find("MazeSize").Find("Value").Find("WidthField").GetComponent<InputField>();
		_mazeWidthField.onEndEdit.AddListener(MazeWidthChanged);
		_mazeHeightField = mazeSettingsRoot.Find("MazeSize").Find("Value").Find("HeightField").GetComponent<InputField>();
		_mazeHeightField.onEndEdit.AddListener(MazeHeightChanged);

		Transform roomStylesRoot = transform.Find("RulesetPanel").Find("RoomStyles");
		roomStylesRoot.Find("Titlebar").Find("AddButton").GetComponent<Button>().onClick.AddListener(AddRoomStyle);
		_removeRoomStyleButton = roomStylesRoot.Find("Titlebar").Find("RemoveButton").GetComponent<Button>();
		_removeRoomStyleButton.onClick.AddListener(RemoveRoomStyle);
		_roomStyleList = roomStylesRoot.Find("Entries").Find("Viewport").Find("Content");

		Transform roomsRoot = transform.Find("RulesetPanel").Find("Rooms");
		roomsRoot.Find("Titlebar").Find("AddButton").GetComponent<Button>().onClick.AddListener(AddRoom);
		_removeRoomButton = roomsRoot.Find("Titlebar").Find("RemoveButton").GetComponent<Button>();
		_removeRoomButton.onClick.AddListener(RemoveRoom);
		_roomList = roomsRoot.Find("Entries").Find("Viewport").Find("Content");

		_busyScreen.SetActive(false);

		UpdateThemeNames();
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

	private void UpdateThemeNames()
	{
		_themeDropdown.ClearOptions();
		_themeDropdown.AddOptions(_themeManager.themeNames);

		if (_themeManager.ruleset != null)
		for (int i = 0; i < _themeDropdown.options.Count; i++)
		{
			if (_themeDropdown.options[i].text == _themeManager.ruleset.name)
			{
				_themeDropdown.value = i;
				break;
			}
		}
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
			roomStyleEntry.transform.SetParent(_roomStyleList);

			SelectableEntry roomStyleSelectable = roomStyleEntry.GetComponent<SelectableEntry>();
			roomStyleSelectable.selectEvent.AddListener((data) => { RoomStyleSelected(data.selectedObject); } );

			RoomStyleEntry roomStyleUI = roomStyleEntry.GetComponent<RoomStyleEntry>();
			roomStyleUI.Initialise(i, ruleset, this, _themeManager);
			_roomStyleEntries.Add(roomStyleUI);
		}

		foreach (RoomEntry roomEntry in _roomEntries)
			Destroy(roomEntry.gameObject);
		_roomEntries.Clear();

		if (ruleset.rooms != null)
		for (int i = 0; i < ruleset.rooms.Length; i++)
		{
			GameObject roomEntry = Instantiate(_roomEntryPrefab);
			roomEntry.transform.SetParent(_roomList);

			SelectableEntry roomSelectable = roomEntry.GetComponent<SelectableEntry>();
			roomSelectable.selectEvent.AddListener((data) => { RoomSelected(data.selectedObject); } );

			RoomEntry roomUI = roomEntry.GetComponent<RoomEntry>();
			roomUI.Initialise(i, ruleset);
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

	public void ShowAddMazeRulesetModal()
	{
		_busyScreen.SetActive(true);
		_addRulesetModal.SetActive(true);
	}

	public void HideAddMazeRulesetModal()
	{
		_busyScreen.SetActive(false);
		_addRulesetModal.SetActive(false);
	}

	public void AddMazeRuleset()
	{
		string newThemeName = _addRulesetNameField.text;
		// TODO: Error message popups for these.
		if (newThemeName.Length <= 0)
		{
			Debug.LogError("Theme name can't be empty.");
			return;
		}
		if (_themeManager.themeNames.Contains(newThemeName))
		{
			Debug.LogError("A theme with the name \"" + newThemeName + "\" already exists.");
			_addRulesetNameField.text = "";
			return;
		}
		_addRulesetNameField.text = "";
		_themeManager.CreateTheme(newThemeName);
		UpdateThemeNames();
		_themeDropdown.value = _themeManager.themeNames.Count - 1;
		ThemeChanged(_themeDropdown.value);
		HideAddMazeRulesetModal();
	}

	public void MazeNameChanged(string newName)
	{
		if (newName == _themeManager.ruleset.name)
			return;
		
		bool error = false;
		// TODO: Error message popups for these.
		if (newName.Length <= 0)
		{
			Debug.LogError("Theme name can't be empty.");
			error = true;
		}
		if (_themeManager.themeNames.Contains(newName))
		{
			Debug.LogError("A theme with the name \"" + newName + "\" already exists.");
			error = true;
		}
		
		MazeRuleset ruleset = _themeManager.ruleset;
		if (!error)
		{
			_themeManager.RenameTheme(newName);
			UpdateThemeNames();
		}
		_mazeNameField.text = ruleset.name;
	}

	public void MazeWidthChanged(string newWidth)
	{
		Point newSize = new Point(int.Parse(newWidth), _themeManager.ruleset.size.y);
		_themeManager.ruleset.SetSize(newSize);
		_mazeWidthField.text = _themeManager.ruleset.size.x.ToString();
	}

	public void MazeHeightChanged(string newHeight)
	{
		Point newSize = new Point(_themeManager.ruleset.size.x, int.Parse(newHeight));
		_themeManager.ruleset.SetSize(newSize);
		_mazeHeightField.text = _themeManager.ruleset.size.y.ToString();
	}

	public void AddRoomStyle()
	{
		RoomStyle roomStyle = new RoomStyle();
		roomStyle.SetName("default", _themeManager.ruleset);
		Utils.PushToArray(ref _themeManager.ruleset.roomStyles, roomStyle);
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
