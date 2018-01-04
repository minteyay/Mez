using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RoomStyleEntry : MonoBehaviour
{
    [HideInInspector] public int index = 0;
    [HideInInspector] public RoomStyle roomStyle = null;
    [HideInInspector] public MazeRuleset mazeRuleset = null;
    [HideInInspector] public EditorUI editorUI = null;
    [HideInInspector] public ThemeManager themeManager = null;

    [SerializeField] private InputField _nameField = null;
    [SerializeField] private Dropdown _tilesetDropdown = null;

    [SerializeField] private GameObject _decorationList = null;
	[SerializeField] private Button _removeDecorationButton = null;
	[SerializeField] private GameObject _decorationEntryPrefab = null;
	private List<DecorationEntry> _decorationEntries = new List<DecorationEntry>();
	private GameObject _selectedDecoration = null;

    [SerializeField] private GameObject _flavourTileList = null;
    [SerializeField] private Button _removeFlavourTileButton = null;
    [SerializeField] private GameObject _flavourTileEntryPrefab = null;
    private List<FlavourTileEntry> _flavourTileEntries = new List<FlavourTileEntry>();
    private GameObject _selectedFlavourTile = null;

    public void UpdateValues()
    {
        _nameField.text = roomStyle.name;

        _tilesetDropdown.ClearOptions();
        string[] tilesets = new string[themeManager.textures.Count];
        themeManager.textures.Keys.CopyTo(tilesets, 0);
        _tilesetDropdown.AddOptions(new List<string>(tilesets));
        for (int i = 0; i < tilesets.Length; i++)
        {
            if (tilesets[i] == roomStyle.tileset)
            {
                _tilesetDropdown.value = i;
                break;
            }
        }

        foreach (DecorationEntry decorationEntry in _decorationEntries)
			Destroy(decorationEntry.gameObject);
		_decorationEntries.Clear();
		
		if (roomStyle.decorations != null)
		for (int i = 0; i < roomStyle.decorations.Length; i++)
		{
			GameObject decorationEntry = Instantiate(_decorationEntryPrefab);
			decorationEntry.transform.SetParent(_decorationList.transform);

            SelectableEntry decorationSelectable = decorationEntry.GetComponent<SelectableEntry>();
			decorationSelectable.selectEvent.AddListener((data) => { DecorationSelected(data.selectedObject); } );

			DecorationEntry decorationUI = decorationEntry.GetComponent<DecorationEntry>();
            decorationUI.index = i;
            decorationUI.themeManager = themeManager;
            decorationUI.Initialise(roomStyle.decorations[i]);
            decorationUI.UpdateValues();
			_decorationEntries.Add(decorationUI);
		}

        foreach (FlavourTileEntry flavourTileEntry in _flavourTileEntries)
            Destroy(flavourTileEntry.gameObject);
        _flavourTileEntries.Clear();

        if (roomStyle.flavourTiles != null)
		for (int i = 0; i < roomStyle.flavourTiles.Length; i++)
		{
			GameObject flavourTileEntry = Instantiate(_flavourTileEntryPrefab);
			flavourTileEntry.transform.SetParent(_flavourTileList.transform);

            SelectableEntry flavourTileSelectable = flavourTileEntry.GetComponent<SelectableEntry>();
			flavourTileSelectable.selectEvent.AddListener((data) => { FlavourTileSelected(data.selectedObject); } );

			FlavourTileEntry flavourTileUI = flavourTileEntry.GetComponent<FlavourTileEntry>();
            flavourTileUI.index = i;
            flavourTileUI.themeManager = themeManager;
            flavourTileUI.Initialise(roomStyle.flavourTiles[i]);
            flavourTileUI.UpdateValues();
			_flavourTileEntries.Add(flavourTileUI);
		}
    }

    public void NameChanged(string newName)
    {
        roomStyle.SetName(newName, mazeRuleset);
        _nameField.text = roomStyle.name;
    }

    public void TilesetChanged(System.Int32 index)
    {
        roomStyle.SetTileset(_tilesetDropdown.options[index].text, themeManager);
        if (roomStyle.tileset != _tilesetDropdown.options[index].text)
            Debug.LogError("Couldn't set tileset to " + _tilesetDropdown.options[index].text);
    }

    public void AddDecoration()
	{
		Utils.PushToArray(ref roomStyle.decorations, new DecorationRuleset());
		editorUI.UnselectEntries();
		UpdateValues();
	}

    public void AddFlavourTile()
    {
        Utils.PushToArray(ref roomStyle.flavourTiles, new FlavourTileRuleset());
        editorUI.UnselectEntries();
        UpdateValues();
    }

	public void RemoveDecoration()
	{
		Utils.RemoveAtIndex(ref roomStyle.decorations, _selectedDecoration.GetComponent<DecorationEntry>().index);
		editorUI.UnselectEntries();
		UpdateValues();
	}

    public void RemoveFlavourTile()
    {
        Utils.RemoveAtIndex(ref roomStyle.flavourTiles, _selectedFlavourTile.GetComponent<FlavourTileEntry>().index);
		editorUI.UnselectEntries();
		UpdateValues();
    }

    public void DecorationSelected(GameObject selected)
	{
        editorUI.UnselectEntries();
		_selectedDecoration = selected;
		_removeDecorationButton.interactable = true;
        editorUI.EntrySelected();
	}

    public void FlavourTileSelected(GameObject selected)
	{
        editorUI.UnselectEntries();
		_selectedFlavourTile = selected;
		_removeFlavourTileButton.interactable = true;
        editorUI.EntrySelected();
	}

	public void EntryDeselected()
	{
		_selectedDecoration = _selectedFlavourTile = null;
        _removeDecorationButton.interactable = _removeFlavourTileButton.interactable = false;
	}
}
