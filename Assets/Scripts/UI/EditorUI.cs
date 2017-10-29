﻿using UnityEngine;
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

	[SerializeField] private GameObject _roomList = null;
	[SerializeField] private GameObject _roomEntryPrefab = null;
	private List<RoomUI> _roomEntries = new List<RoomUI>();

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
		
		if (ruleset.roomStyles != null)
		for (int i = 0; i < ruleset.roomStyles.Length; i++)
		{
			GameObject roomStyleEntry = Instantiate(_roomStyleEntryPrefab);
			roomStyleEntry.transform.SetParent(_roomStyleList.transform);

			RoomStyleUI roomStyleUI = roomStyleEntry.GetComponent<RoomStyleUI>();
			roomStyleUI.index = i;
			roomStyleUI.roomStyle = ruleset.roomStyles[i];
			roomStyleUI.themeManager = _themeManager;
			roomStyleUI.removeCallback = RemoveRoomStyle;
			roomStyleUI.UpdateValues();
			_roomStyleEntries.Add(roomStyleUI);
		}

		foreach (RoomUI roomEntry in _roomEntries)
			Destroy(roomEntry.gameObject);
		_roomEntries.Clear();

		if (ruleset.rooms != null)
		for (int i = 0; i < ruleset.rooms.Length; i++)
		{
			GameObject roomEntry = Instantiate(_roomEntryPrefab);
			roomEntry.transform.SetParent(_roomList.transform);

			RoomUI roomUI = roomEntry.GetComponent<RoomUI>();
			roomUI.index = i;
			roomUI.mazeRuleset = ruleset;
			roomUI.roomRuleset = ruleset.rooms[i];
			roomUI.removeCallback = RemoveRoom;
			roomUI.UpdateValues();
			_roomEntries.Add(roomUI);
		}
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
		MazeRuleset ruleset = _themeManager.ruleset;

		int roomStyleCount = 0;
		if (ruleset.roomStyles != null)
			roomStyleCount = ruleset.roomStyles.Length;
		
		RoomStyle[] roomStyles = new RoomStyle[roomStyleCount + 1];
		for (int i = 0; i < roomStyleCount; i++)
			roomStyles[i] = ruleset.roomStyles[i];
		roomStyles[roomStyleCount] = new RoomStyle();

		ruleset.roomStyles = roomStyles;

		UpdateValues();
	}

	public void RemoveRoomStyle(int index)
	{
		MazeRuleset ruleset = _themeManager.ruleset;
		
		if (ruleset.roomStyles == null)
			return;
		
		if ((ruleset.roomStyles.Length - 1) <= 0)
		{
			ruleset.roomStyles = null;
		}
		else
		{
			RoomStyle[] roomStyles = new RoomStyle[ruleset.roomStyles.Length - 1];
			for (int i = 0; i < ruleset.roomStyles.Length; i++)
			{
				if (i < index)
					roomStyles[i] = ruleset.roomStyles[i];
				else if (i > index)
					roomStyles[i - 1] = ruleset.roomStyles[i];
			}
			ruleset.roomStyles = roomStyles;
		}

		UpdateValues();
	}

	public void AddRoom()
	{
		MazeRuleset ruleset = _themeManager.ruleset;

		int roomCount = 0;
		if (ruleset.rooms != null)
			roomCount = ruleset.rooms.Length;
		
		RoomRuleset[] rooms = new RoomRuleset[roomCount + 1];
		for (int i = 0; i < roomCount; i++)
			rooms[i] = ruleset.rooms[i];
		rooms[roomCount] = new RoomRuleset();

		ruleset.rooms = rooms;

		UpdateValues();
	}

	public void RemoveRoom(int index)
	{
		MazeRuleset ruleset = _themeManager.ruleset;
		
		if (ruleset.rooms == null)
			return;
		
		if ((ruleset.rooms.Length - 1) <= 0)
		{
			ruleset.rooms = null;
		}
		else
		{
			RoomRuleset[] rooms = new RoomRuleset[ruleset.rooms.Length - 1];
			for (int i = 0; i < ruleset.rooms.Length; i++)
			{
				if (i < index)
					rooms[i] = ruleset.rooms[i];
				else if (i > index)
					rooms[i - 1] = ruleset.rooms[i];
			}
			ruleset.rooms = rooms;
		}

		UpdateValues();
	}
}
