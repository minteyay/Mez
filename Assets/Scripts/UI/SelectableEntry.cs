using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableEntry : MonoBehaviour, ISelectHandler
{
	public EventTrigger.TriggerEvent selectEvent { get; private set; }

	private void Awake()
	{
		selectEvent = new EventTrigger.TriggerEvent();
	}

	public void OnSelect(BaseEventData eventData)
    {
        selectEvent.Invoke(eventData);
    }
}
