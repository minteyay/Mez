using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        roomRuleset.style = _styleDropdown.options[index].text;
    }

    public void StartChanged(System.Int32 index)
    {
        roomRuleset.start = (RoomRuleset.Start)index;
    }

    public void SizeChanged(string newSize)
    {
        Point newSizeRange;
        if (!Utils.TryParseRange(newSize, out newSizeRange))
            roomRuleset.size = _sizeField.text = 1.ToString();
        else
            roomRuleset.size = _sizeField.text = newSize;
    }

    public void CountChanged(string newCount)
    {
        Point newCountRange;
        if (!Utils.TryParseRange(newCount, out newCountRange))
            roomRuleset.count = _countField.text = 1.ToString();
        else
            roomRuleset.count = _countField.text = newCount;
    }
}
