using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableEntry : MonoBehaviour, IPointerClickHandler
{
	public EventTrigger.TriggerEvent selectEvent { get; private set; }

	private void Awake()
	{
		selectEvent = new EventTrigger.TriggerEvent();
	}

	public void OnPointerClick(PointerEventData eventData)
    {
        selectEvent.Invoke(eventData);
    }
}
