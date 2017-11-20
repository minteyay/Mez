using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// TODO: Hide the count field when it's useless (e.g. positions for maze start and end).

public class RoomEntry : MonoBehaviour
{
    [HideInInspector] public int index = 0;
    [HideInInspector] public MazeRuleset mazeRuleset = null;
    [HideInInspector] public RoomRuleset roomRuleset = null;

    [SerializeField] private Dropdown _styleDropdown = null;
    [SerializeField] private Dropdown _startDropdown = null;
    [SerializeField] private InputField _sizeField = null;
    [SerializeField] private InputField _countField = null;

    public void UpdateValues()
    {
        _styleDropdown.ClearOptions();
        int selectedIndex = 0;
        List<string> roomStyles = new List<string>();
        for (int i = 0; i < mazeRuleset.roomStyles.Length; i++)
        {
            string roomStyleName = mazeRuleset.roomStyles[i].name;
            roomStyles.Add(roomStyleName);
            if (roomStyleName == roomRuleset.style)
                selectedIndex = i;
        }
        _styleDropdown.AddOptions(roomStyles);
        _styleDropdown.value = selectedIndex;

        _startDropdown.ClearOptions();
        _startDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(RoomRuleset.Start))));
        _startDropdown.value = (int)roomRuleset.start;

        _sizeField.text = roomRuleset.size;
        
        _countField.text = roomRuleset.count;
    }

    public void StyleChanged(System.Int32 index)
    {
        roomRuleset.SetStyle(_styleDropdown.options[index].text, mazeRuleset);
        if (roomRuleset.style != _styleDropdown.options[index].text)
            Debug.LogError("Couldn't set style to " + _styleDropdown.options[index].text);
    }

    public void StartChanged(System.Int32 index)
    {
        roomRuleset.start = (RoomRuleset.Start)index;
    }

    public void SizeChanged(string newSize)
    {
        roomRuleset.SetSize(newSize);
        _sizeField.text = roomRuleset.size;
    }

    public void CountChanged(string newCount)
    {
        roomRuleset.SetCount(newCount);
        _countField.text = roomRuleset.count;
    }
}
