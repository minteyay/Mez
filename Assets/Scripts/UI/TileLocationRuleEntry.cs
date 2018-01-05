using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TileLocationRuleEntry : MonoBehaviour
{
	private TileLocationRule _tileLocationRule = null;

	[SerializeField] private GameObject _togglePrefab = null;
	private List<Toggle> _toggles = new List<Toggle>();
	private bool _disableCallbacks = false;

	public void Initialise(TileLocationRule tileLocationRule)
	{
		_tileLocationRule = tileLocationRule;

		transform.Find("Buttons").Find("All").GetComponent<Button>().onClick.AddListener(EnableAll);
		transform.Find("Buttons").Find("None").GetComponent<Button>().onClick.AddListener(DisableAll);

		Transform toggleRoot = transform.Find("Rules");
		foreach (TileLocationRule.Option option in System.Enum.GetValues(typeof(TileLocationRule.Option)))
		{
			Toggle toggle = Instantiate(_togglePrefab, toggleRoot).GetComponent<Toggle>();
			toggle.name = toggle.transform.Find("Label").GetComponent<Text>().text = option.ToString();
			toggle.onValueChanged.AddListener(ToggleChanged);
			_toggles.Add(toggle);
		}

		UpdateValues();
	}

	private void UpdateValues()
	{
		_disableCallbacks = true;
		uint tileLocationValue = _tileLocationRule.value;
		TileLocationRule.Option[] _options = (TileLocationRule.Option[])System.Enum.GetValues(typeof(TileLocationRule.Option));
		for (int i = 0; i < _toggles.Count; i++)
			_toggles[i].isOn = Utils.IsBitUp(tileLocationValue, (uint)_options[i]);
		_disableCallbacks = false;
	}

	private void ToggleChanged(bool newValue)
	{
		if (_disableCallbacks)
			return;
		
		uint newRuleValue = 0;
		TileLocationRule.Option[] _options = (TileLocationRule.Option[])System.Enum.GetValues(typeof(TileLocationRule.Option));
		for (int i = 0; i < _toggles.Count; i++)
			if (_toggles[i].isOn)
				newRuleValue |= (uint)_options[i];
		_tileLocationRule.value = newRuleValue;
	}

	private void EnableAll()
	{
		_tileLocationRule.value = uint.MaxValue;
		UpdateValues();
	}

	private void DisableAll()
	{
		_tileLocationRule.value = 0;
		UpdateValues();
	}
}