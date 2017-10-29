using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomUI : MonoBehaviour
{
    [HideInInspector] public MazeRuleset mazeRuleset = null;
    [HideInInspector] public RoomRuleset roomRuleset = null;

    [SerializeField] private Dropdown _styleDropdown = null;

    public void UpdateValues()
    {
        _styleDropdown.ClearOptions();
        List<string> roomStyles = new List<string>();
        foreach (RoomStyle roomStyle in mazeRuleset.roomStyles)
            roomStyles.Add(roomStyle.name);
        _styleDropdown.AddOptions(roomStyles);
    }
}
