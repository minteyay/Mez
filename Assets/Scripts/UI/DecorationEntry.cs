using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationEntry : MonoBehaviour
{
	public int index { get; private set; }
	private DecorationRuleset _decorationRuleset = null;
	private ThemeManager _themeManager = null;

	private Dropdown _locationDropdown = null;
	private Dropdown _textureDropdown = null;
	private ToggleGroup _amountTypeGroup = null;
	private InputField _chanceField = null;
	private InputField _countField = null;
	private TileLocationRuleEntry _tileLocationRule = null;

	public void Initialise(int index, DecorationRuleset decorationRuleset, ThemeManager themeManager)
	{
		this.index = index;
		_decorationRuleset = decorationRuleset;
		_themeManager = themeManager;

		_locationDropdown = transform.Find("Location").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
        _locationDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(DecorationRuleset.Location))));
		_textureDropdown = transform.Find("Texture").Find("Value").Find("Dropdown").GetComponent<Dropdown>();
		_amountTypeGroup = transform.Find("AmountType").GetComponent<ToggleGroup>();
		_chanceField = transform.Find("Chance").Find("Value").Find("InputField").GetComponent<InputField>();
		_countField = transform.Find("Count").Find("Value").Find("InputField").GetComponent<InputField>();
		_tileLocationRule = transform.Find("TileLocationRule").GetComponent<TileLocationRuleEntry>();
		_tileLocationRule.Initialise(_decorationRuleset.validLocations);

		_locationDropdown.onValueChanged.AddListener(LocationChanged);
		_textureDropdown.onValueChanged.AddListener(TextureChanged);
		Transform amountTypeRoot = _amountTypeGroup.transform;
		for (int i = 0; i < amountTypeRoot.childCount; i++)
			amountTypeRoot.GetChild(i).GetComponent<Toggle>().onValueChanged.AddListener(AmountTypeChanged);
		_chanceField.onEndEdit.AddListener(ChanceChanged);
		_countField.onEndEdit.AddListener(CountChanged);

		UpdateValues();
	}

	private void UpdateValues()
	{
        _locationDropdown.value = (int)_decorationRuleset.location;

		_textureDropdown.ClearOptions();
        string[] textures = new string[_themeManager.textures.Count];
        _themeManager.textures.Keys.CopyTo(textures, 0);
        _textureDropdown.AddOptions(new List<string>(textures));
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] == _decorationRuleset.texture)
            {
                _textureDropdown.value = i;
                break;
            }
        }

		string amountType = _decorationRuleset.amountType.ToString();
		for (int i = 0; i < _amountTypeGroup.transform.childCount; i++)
		{
			Toggle toggle = _amountTypeGroup.transform.GetChild(i).GetComponent<Toggle>();
			if (toggle.gameObject.name == amountType)
			{
				toggle.isOn = true;
				break;
			}
		}

		_chanceField.text = _countField.text = _decorationRuleset.amount;
	}

	private void LocationChanged(System.Int32 index)
	{
		_decorationRuleset.location = (DecorationRuleset.Location)index;
	}

	private void TextureChanged(System.Int32 index)
	{
		_decorationRuleset.SetTexture(_textureDropdown.options[index].text, _themeManager);
		if (_decorationRuleset.texture != _textureDropdown.options[index].text)
            Debug.LogError("Couldn't set texture to " + _textureDropdown.options[index].text);
	}

	private void AmountTypeChanged(bool newValue)
	{
		List<Toggle> toggles = new List<Toggle>(_amountTypeGroup.ActiveToggles());
		DecorationRuleset.AmountType amountType = (DecorationRuleset.AmountType)System.Enum.Parse(typeof(DecorationRuleset.AmountType), toggles[0].name);
		if (amountType != _decorationRuleset.amountType)
			_decorationRuleset.SetAmountType(amountType);
		
		switch (amountType)
		{
			case DecorationRuleset.AmountType.Chance:
				_chanceField.transform.parent.parent.gameObject.SetActive(true);
				_countField.transform.parent.parent.gameObject.SetActive(false);
				ChanceChanged(_decorationRuleset.amount);
				break;
			case DecorationRuleset.AmountType.Count:
				_countField.transform.parent.parent.gameObject.SetActive(true);
				_chanceField.transform.parent.parent.gameObject.SetActive(false);
				CountChanged(_decorationRuleset.amount);
				break;
		}
	}

	private void ChanceChanged(string newChance)
	{
		_decorationRuleset.SetAmount(newChance);
		_chanceField.text = _decorationRuleset.amount;
	}

	private void CountChanged(string newCount)
	{
		_decorationRuleset.SetAmount(newCount);
		_countField.text = _decorationRuleset.amount;
	}
}