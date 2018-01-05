using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RoomStyleEntry : MonoBehaviour
{
    public int index { get; private set; }
    private MazeRuleset _mazeRuleset = null;
    private RoomStyle _roomStyle = null;
    private EditorUI _editorUI = null;
    private ThemeManager _themeManager = null;

    private InputField _nameField = null;
    private Dropdown _tilesetDropdown = null;

    private Transform _decorationList = null;
	private Button _removeDecorationButton = null;
	[SerializeField] private GameObject _decorationEntryPrefab = null;
	private List<DecorationEntry> _decorationEntries = new List<DecorationEntry>();
	private GameObject _selectedDecoration = null;

    private Transform _flavourTileList = null;
    private Button _removeFlavourTileButton = null;
    [SerializeField] private GameObject _flavourTileEntryPrefab = null;
    private List<FlavourTileEntry> _flavourTileEntries = new List<FlavourTileEntry>();
    private GameObject _selectedFlavourTile = null;

    public void Initialise(int index, MazeRuleset mazeRuleset, EditorUI editorUI, ThemeManager themeManager)
    {
        this.index = index;
        _mazeRuleset = mazeRuleset;
        _roomStyle = _mazeRuleset.roomStyles[index];
        _editorUI = editorUI;
        _themeManager = themeManager;

        _nameField = transform.Find("Name").Find("Value").Find("InputField").GetComponent<InputField>();
        _tilesetDropdown = transform.Find("Tileset").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
        _decorationList = transform.Find("Decorations").Find("Entries");
        _removeDecorationButton = transform.Find("Decorations").Find("Titlebar").Find("RemoveButton").GetComponent<Button>();
        _flavourTileList = transform.Find("FlavourTiles").Find("Entries");
        _removeFlavourTileButton = transform.Find("FlavourTiles").Find("Titlebar").Find("RemoveButton").GetComponent<Button>();

        _nameField.onEndEdit.AddListener(NameChanged);
        _tilesetDropdown.onValueChanged.AddListener(TilesetChanged);
        transform.Find("Decorations").Find("Titlebar").Find("AddButton").GetComponent<Button>().onClick.AddListener(AddDecoration);
        _removeDecorationButton.onClick.AddListener(RemoveDecoration);
        transform.Find("FlavourTiles").Find("Titlebar").Find("AddButton").GetComponent<Button>().onClick.AddListener(AddFlavourTile);
        _removeFlavourTileButton.onClick.AddListener(RemoveFlavourTile);

        UpdateValues();
    }

    public void UpdateValues()
    {
        _nameField.text = _roomStyle.name;

        _tilesetDropdown.ClearOptions();
        string[] tilesets = new string[_themeManager.textures.Count];
        _themeManager.textures.Keys.CopyTo(tilesets, 0);
        _tilesetDropdown.AddOptions(new List<string>(tilesets));
        for (int i = 0; i < tilesets.Length; i++)
        {
            if (tilesets[i] == _roomStyle.tileset)
            {
                _tilesetDropdown.value = i;
                break;
            }
        }

        foreach (DecorationEntry decorationEntry in _decorationEntries)
			Destroy(decorationEntry.gameObject);
		_decorationEntries.Clear();
		
		if (_roomStyle.decorations != null)
		for (int i = 0; i < _roomStyle.decorations.Length; i++)
		{
			GameObject decorationEntry = Instantiate(_decorationEntryPrefab);
			decorationEntry.transform.SetParent(_decorationList);

            SelectableEntry decorationSelectable = decorationEntry.GetComponent<SelectableEntry>();
			decorationSelectable.selectEvent.AddListener((data) => { DecorationSelected(data.selectedObject); } );

			DecorationEntry decorationUI = decorationEntry.GetComponent<DecorationEntry>();
            decorationUI.Initialise(i, _roomStyle.decorations[i], _themeManager);
			_decorationEntries.Add(decorationUI);
		}

        foreach (FlavourTileEntry flavourTileEntry in _flavourTileEntries)
            Destroy(flavourTileEntry.gameObject);
        _flavourTileEntries.Clear();

        if (_roomStyle.flavourTiles != null)
		for (int i = 0; i < _roomStyle.flavourTiles.Length; i++)
		{
			GameObject flavourTileEntry = Instantiate(_flavourTileEntryPrefab);
			flavourTileEntry.transform.SetParent(_flavourTileList);

            SelectableEntry flavourTileSelectable = flavourTileEntry.GetComponent<SelectableEntry>();
			flavourTileSelectable.selectEvent.AddListener((data) => { FlavourTileSelected(data.selectedObject); } );

			FlavourTileEntry flavourTileUI = flavourTileEntry.GetComponent<FlavourTileEntry>();
            flavourTileUI.Initialise(i, _roomStyle.flavourTiles[i], _themeManager);
			_flavourTileEntries.Add(flavourTileUI);
		}
    }

    private void NameChanged(string newName)
    {
        _roomStyle.SetName(newName, _mazeRuleset);
        _nameField.text = _roomStyle.name;
    }

    private void TilesetChanged(System.Int32 index)
    {
        _roomStyle.SetTileset(_tilesetDropdown.options[index].text, _themeManager);
        if (_roomStyle.tileset != _tilesetDropdown.options[index].text)
            Debug.LogError("Couldn't set tileset to " + _tilesetDropdown.options[index].text);
    }

    private void AddDecoration()
	{
		Utils.PushToArray(ref _roomStyle.decorations, new DecorationRuleset());
		_editorUI.UnselectEntries();
		UpdateValues();
	}

    private void AddFlavourTile()
    {
        Utils.PushToArray(ref _roomStyle.flavourTiles, new FlavourTileRuleset());
        _editorUI.UnselectEntries();
        UpdateValues();
    }

	private void RemoveDecoration()
	{
		Utils.RemoveAtIndex(ref _roomStyle.decorations, _selectedDecoration.GetComponent<DecorationEntry>().index);
		_editorUI.UnselectEntries();
		UpdateValues();
	}

    private void RemoveFlavourTile()
    {
        Utils.RemoveAtIndex(ref _roomStyle.flavourTiles, _selectedFlavourTile.GetComponent<FlavourTileEntry>().index);
		_editorUI.UnselectEntries();
		UpdateValues();
    }

    private void DecorationSelected(GameObject selected)
	{
        _editorUI.UnselectEntries();
		_selectedDecoration = selected;
		_removeDecorationButton.interactable = true;
        _editorUI.EntrySelected();
	}

    private void FlavourTileSelected(GameObject selected)
	{
        _editorUI.UnselectEntries();
		_selectedFlavourTile = selected;
		_removeFlavourTileButton.interactable = true;
        _editorUI.EntrySelected();
	}

	public void EntryDeselected()
	{
		_selectedDecoration = _selectedFlavourTile = null;
        _removeDecorationButton.interactable = _removeFlavourTileButton.interactable = false;
	}
}
