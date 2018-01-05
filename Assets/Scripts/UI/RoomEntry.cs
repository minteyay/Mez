using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// TODO: Hide the count field when it's useless (e.g. positions for maze start and end).

public class RoomEntry : MonoBehaviour
{
    public int index { get; private set; }
    private MazeRuleset _mazeRuleset = null;
    private RoomRuleset _roomRuleset = null;

    private Dropdown _styleDropdown = null;
    private Dropdown _startDropdown = null;
    private InputField _sizeField = null;
    private InputField _countField = null;

    public void Initialise(int index, MazeRuleset mazeRuleset)
    {
        this.index = index;
        _mazeRuleset = mazeRuleset;
        _roomRuleset = _mazeRuleset.rooms[index];

        _styleDropdown = transform.Find("Style").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
        _startDropdown = transform.Find("Start").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
        _sizeField = transform.Find("Size").Find("Value").Find("InputField").GetComponent<InputField>();
        _countField = transform.Find("Count").Find("Value").Find("InputField").GetComponent<InputField>();

        _styleDropdown.onValueChanged.AddListener(StyleChanged);
        _startDropdown.onValueChanged.AddListener(StartChanged);
        _sizeField.onEndEdit.AddListener(SizeChanged);
        _countField.onEndEdit.AddListener(CountChanged);

        UpdateValues();
    }

    private void UpdateValues()
    {
        _styleDropdown.ClearOptions();
        int selectedIndex = 0;
        List<string> roomStyles = new List<string>();
        for (int i = 0; i < _mazeRuleset.roomStyles.Length; i++)
        {
            string roomStyleName = _mazeRuleset.roomStyles[i].name;
            roomStyles.Add(roomStyleName);
            if (roomStyleName == _roomRuleset.style)
                selectedIndex = i;
        }
        _styleDropdown.AddOptions(roomStyles);
        _styleDropdown.value = selectedIndex;

        _startDropdown.ClearOptions();
        _startDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(RoomRuleset.Start))));
        _startDropdown.value = (int)_roomRuleset.start;

        _sizeField.text = _roomRuleset.size;
        
        _countField.text = _roomRuleset.count;
    }

    private void StyleChanged(System.Int32 index)
    {
        _roomRuleset.SetStyle(_styleDropdown.options[index].text, _mazeRuleset);
        if (_roomRuleset.style != _styleDropdown.options[index].text)
            Debug.LogError("Couldn't set style to " + _styleDropdown.options[index].text);
    }

    private void StartChanged(System.Int32 index)
    {
        _roomRuleset.start = (RoomRuleset.Start)index;
    }

    private void SizeChanged(string newSize)
    {
        _roomRuleset.SetSize(newSize);
        _sizeField.text = _roomRuleset.size;
    }

    private void CountChanged(string newCount)
    {
        _roomRuleset.SetCount(newCount);
        _countField.text = _roomRuleset.count;
    }
}
