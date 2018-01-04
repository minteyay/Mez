using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationEntry : MonoBehaviour
{
	[HideInInspector] public int index = 0;
	private DecorationRuleset _decorationRuleset = null;
	[HideInInspector] public ThemeManager themeManager = null;

	[SerializeField] private Dropdown _locationDropdown = null;
	[SerializeField] private Dropdown _textureDropdown = null;
	[SerializeField] private ToggleGroup _amountTypeGroup = null;
	[SerializeField] private InputField _chanceField = null;
	[SerializeField] private InputField _countField = null;
	private TileLocationRuleEntry _tileLocationRule = null;

	public void Initialise(DecorationRuleset decorationRuleset)
	{
		_decorationRuleset = decorationRuleset;

		_tileLocationRule = transform.Find("TileLocationRule").GetComponent<TileLocationRuleEntry>();
		_tileLocationRule.Initialise(_decorationRuleset.validLocations);
	}

	public void UpdateValues()
	{
		_locationDropdown.ClearOptions();
        _locationDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(DecorationRuleset.Location))));
        _locationDropdown.value = (int)_decorationRuleset.location;

		_textureDropdown.ClearOptions();
        string[] textures = new string[themeManager.textures.Count];
        themeManager.textures.Keys.CopyTo(textures, 0);
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

	public void LocationChanged(System.Int32 index)
	{
		_decorationRuleset.location = (DecorationRuleset.Location)index;
	}

	public void TextureChanged(System.Int32 index)
	{
		_decorationRuleset.SetTexture(_textureDropdown.options[index].text, themeManager);
		if (_decorationRuleset.texture != _textureDropdown.options[index].text)
            Debug.LogError("Couldn't set texture to " + _textureDropdown.options[index].text);
	}

	public void AmountTypeChanged()
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

	public void ChanceChanged(string newChance)
	{
		_decorationRuleset.SetAmount(newChance);
		_chanceField.text = _decorationRuleset.amount;
	}

	public void CountChanged(string newCount)
	{
		_decorationRuleset.SetAmount(newCount);
		_countField.text = _decorationRuleset.amount;
	}
}