using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlavourTileEntry : MonoBehaviour
{
	[HideInInspector] public int index = 0;
	private FlavourTileRuleset _flavourTileRuleset = null;
	[HideInInspector] public ThemeManager themeManager = null;

	private ToggleGroup _typeGroup = null;
	private Dropdown _textureDropdown = null;
	private Dropdown _locationDropdown = null;
	private Transform _locationTogglesRoot = null;
	private List<Toggle> _locationToggles = new List<Toggle>();
	private ToggleGroup _amountTypeGroup = null;
	private InputField _chanceField = null;
	private InputField _countField = null;
	private TileLocationRuleEntry _tileLocationRule = null;

	private bool _disableCallbacks = false;

	public void Initialise(FlavourTileRuleset flavourTileRuleset)
	{
		_flavourTileRuleset = flavourTileRuleset;

		_typeGroup = transform.Find("Type").GetComponent<ToggleGroup>();
		_textureDropdown = transform.Find("Texture").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
		_locationDropdown = transform.Find("Location").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
        _locationDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(FlavourTileRuleset.Location))));
		_locationTogglesRoot = transform.Find("Locations");
		for (int i = 0; i < _locationTogglesRoot.childCount; i++)
			_locationToggles.Add(_locationTogglesRoot.GetChild(i).GetComponent<Toggle>());
		_amountTypeGroup = transform.Find("AmountType").GetComponent<ToggleGroup>();
		_chanceField = transform.Find("Chance").Find("Value").Find("InputField").GetComponent<InputField>();
		_countField = transform.Find("Count").Find("Value").Find("InputField").GetComponent<InputField>();
		_tileLocationRule = transform.Find("TileLocationRule").GetComponent<TileLocationRuleEntry>();
		_tileLocationRule.Initialise(flavourTileRuleset.validLocations);

		for (int i = 0; i < _typeGroup.transform.childCount; i++)
			_typeGroup.transform.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener( (bool b) => TypeChanged() );
		_textureDropdown.onValueChanged.AddListener(TextureChanged);
		_locationDropdown.onValueChanged.AddListener(LocationChanged);
		foreach (Toggle toggle in _locationToggles)
			toggle.onValueChanged.AddListener( (bool b) => LocationChanged() );
		for (int i = 0; i < _amountTypeGroup.transform.childCount; i++)
			_amountTypeGroup.transform.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener( (bool b) => AmountTypeChanged() );
		_chanceField.onEndEdit.AddListener(ChanceChanged);
		_countField.onEndEdit.AddListener(CountChanged);
	}

	public void UpdateValues()
	{
		_disableCallbacks = true;

		string type = _flavourTileRuleset.type.ToString();
		for (int i = 0; i < _typeGroup.transform.childCount; i++)
		{
			Toggle toggle = _typeGroup.transform.GetChild(i).GetComponent<Toggle>();
			if (toggle.gameObject.name == type)
			{
				toggle.isOn = true;
				break;
			}
		}

		UpdateLocationUI();

		_textureDropdown.ClearOptions();
        string[] textures = new string[themeManager.textures.Count];
        themeManager.textures.Keys.CopyTo(textures, 0);
        _textureDropdown.AddOptions(new List<string>(textures));
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] == _flavourTileRuleset.texture)
            {
                _textureDropdown.value = i;
                break;
            }
        }

		string amountType = _flavourTileRuleset.amountType.ToString();
		for (int i = 0; i < _amountTypeGroup.transform.childCount; i++)
		{
			Toggle toggle = _amountTypeGroup.transform.GetChild(i).GetComponent<Toggle>();
			if (toggle.gameObject.name == amountType)
			{
				toggle.isOn = true;
				break;
			}
		}

		_chanceField.text = _countField.text = _flavourTileRuleset.amount;
	}

	private void UpdateLocationUI()
	{
		_disableCallbacks = true;
		switch (_flavourTileRuleset.type)
		{
			case FlavourTileRuleset.Type.Single:
				_locationDropdown.transform.parent.parent.gameObject.SetActive(true);
				_locationTogglesRoot.gameObject.SetActive(false);

				string[] locations = System.Enum.GetNames(typeof(FlavourTileRuleset.Location));
				for (int i = 0; i < locations.Length; i++)
				{
					if (locations[i] == ((FlavourTileRuleset.Location)_flavourTileRuleset.location).ToString())
					{
						_locationDropdown.value = i;
						break;
					}
				}
				break;
			case FlavourTileRuleset.Type.Tile:
				_locationTogglesRoot.gameObject.SetActive(true);
				_locationDropdown.transform.parent.parent.gameObject.SetActive(false);

				foreach (Toggle toggle in _locationToggles)
				{
					byte toggleValue = (byte)System.Enum.Parse(typeof(FlavourTileRuleset.Location), toggle.name);
					toggle.isOn = Utils.IsBitUp(_flavourTileRuleset.location, toggleValue);
				}
				break;
		}
		_disableCallbacks = false;
	}

	public void TypeChanged()
	{
		if (_disableCallbacks)
			return;
		
		List<Toggle> toggles = new List<Toggle>(_typeGroup.ActiveToggles());
		FlavourTileRuleset.Type type = (FlavourTileRuleset.Type)System.Enum.Parse(typeof(FlavourTileRuleset.Type), toggles[0].name);
		if (type != _flavourTileRuleset.type)
		{
			_flavourTileRuleset.location = 0;
			_flavourTileRuleset.SetType(type);
		}
		
		UpdateLocationUI();
	}

	public void TextureChanged(System.Int32 index)
	{
		_flavourTileRuleset.SetTexture(_textureDropdown.options[index].text, themeManager);
		if (_flavourTileRuleset.texture != _textureDropdown.options[index].text)
            Debug.LogError("Couldn't set texture to " + _textureDropdown.options[index].text);
	}

	public void LocationChanged(System.Int32 index)
	{
		if (_disableCallbacks)
			return;

		string[] locationNames = System.Enum.GetNames(typeof(FlavourTileRuleset.Location));
		byte location = (byte)System.Enum.Parse(typeof(FlavourTileRuleset.Location), locationNames[index]);
		if (location != _flavourTileRuleset.location)
			_flavourTileRuleset.SetLocation(location);
	}

	public void LocationChanged()
	{
		if (_disableCallbacks)
			return;

		byte location = 0;
		foreach (Toggle toggle in _locationToggles)
			if (toggle.isOn)
				location |= (byte)System.Enum.Parse(typeof(FlavourTileRuleset.Location), toggle.name);
		if (location != _flavourTileRuleset.location)
			_flavourTileRuleset.SetLocation(location);
	}

	public void AmountTypeChanged()
	{
		List<Toggle> toggles = new List<Toggle>(_amountTypeGroup.ActiveToggles());
		FlavourTileRuleset.AmountType amountType = (FlavourTileRuleset.AmountType)System.Enum.Parse(typeof(FlavourTileRuleset.AmountType), toggles[0].name);
		if (amountType != _flavourTileRuleset.amountType)
			_flavourTileRuleset.SetAmountType(amountType);
		
		switch (amountType)
		{
			case FlavourTileRuleset.AmountType.Chance:
				_chanceField.transform.parent.parent.gameObject.SetActive(true);
				_countField.transform.parent.parent.gameObject.SetActive(false);
				ChanceChanged(_flavourTileRuleset.amount);
				break;
			case FlavourTileRuleset.AmountType.Count:
				_countField.transform.parent.parent.gameObject.SetActive(true);
				_chanceField.transform.parent.parent.gameObject.SetActive(false);
				CountChanged(_flavourTileRuleset.amount);
				break;
		}
	}

	public void ChanceChanged(string newChance)
	{
		_flavourTileRuleset.SetAmount(newChance);
		_chanceField.text = _flavourTileRuleset.amount;
	}

	public void CountChanged(string newCount)
	{
		_flavourTileRuleset.SetAmount(newCount);
		_countField.text = _flavourTileRuleset.amount;
	}
}