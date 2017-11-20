using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecorationEntry : MonoBehaviour
{
	[HideInInspector] public int index = 0;
	[HideInInspector] public DecorationRuleset decorationRuleset = null;
	[HideInInspector] public ThemeManager themeManager = null;

	[SerializeField] private Dropdown _locationDropdown = null;
	[SerializeField] private Dropdown _textureDropdown = null;
	[SerializeField] private ToggleGroup _amountTypeGroup = null;
	[SerializeField] private InputField _chanceField = null;
	[SerializeField] private InputField _countField = null;

	public void UpdateValues()
	{
		_locationDropdown.ClearOptions();
        _locationDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(DecorationRuleset.Location))));
        _locationDropdown.value = (int)decorationRuleset.location;

		_textureDropdown.ClearOptions();
        string[] textures = new string[themeManager.textures.Count];
        themeManager.textures.Keys.CopyTo(textures, 0);
        _textureDropdown.AddOptions(new List<string>(textures));
        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] == decorationRuleset.texture)
            {
                _textureDropdown.value = i;
                break;
            }
        }

		string amountType = decorationRuleset.amountType.ToString();
		for (int i = 0; i < _amountTypeGroup.transform.childCount; i++)
		{
			Toggle toggle = _amountTypeGroup.transform.GetChild(i).GetComponent<Toggle>();
			if (toggle.gameObject.name == amountType)
			{
				toggle.isOn = true;
				break;
			}
		}

		_chanceField.text = _countField.text = decorationRuleset.amount;
	}

	public void LocationChanged(System.Int32 index)
	{
		decorationRuleset.location = (DecorationRuleset.Location)index;
	}

	public void TextureChanged(System.Int32 index)
	{
		decorationRuleset.SetTexture(_textureDropdown.options[index].text, themeManager);
		if (decorationRuleset.texture != _textureDropdown.options[index].text)
            Debug.LogError("Couldn't set texture to " + _textureDropdown.options[index].text);
	}

	public void AmountTypeChanged()
	{
		List<Toggle> toggles = new List<Toggle>(_amountTypeGroup.ActiveToggles());
		DecorationRuleset.AmountType amountType = (DecorationRuleset.AmountType)System.Enum.Parse(typeof(DecorationRuleset.AmountType), toggles[0].name);
		if (amountType != decorationRuleset.amountType)
			decorationRuleset.SetAmountType(amountType);
		
		switch (amountType)
		{
			case DecorationRuleset.AmountType.Chance:
				_chanceField.transform.parent.parent.gameObject.SetActive(true);
				_countField.transform.parent.parent.gameObject.SetActive(false);
				ChanceChanged(decorationRuleset.amount);
				break;
			case DecorationRuleset.AmountType.Count:
				_countField.transform.parent.parent.gameObject.SetActive(true);
				_chanceField.transform.parent.parent.gameObject.SetActive(false);
				CountChanged(decorationRuleset.amount);
				break;
		}
	}

	public void ChanceChanged(string newChance)
	{
		decorationRuleset.SetAmount(newChance);
		_chanceField.text = decorationRuleset.amount;
	}

	public void CountChanged(string newCount)
	{
		decorationRuleset.SetAmount(newCount);
		_countField.text = decorationRuleset.amount;
	}
}