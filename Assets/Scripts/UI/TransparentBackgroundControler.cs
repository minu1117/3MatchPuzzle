using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransparentBackgroundControler : MonoBehaviour
{
    [SerializeField] private EventTrigger eventTrigger;

    public void ConnectEventTrigger()
    {
        AddPointerClickEventTrigger(Off);
    }

    private void AddPointerClickEventTrigger(Action action)
    {
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerClick
        };

        entry.callback.AddListener((eventData) => { action(); });
        eventTrigger.triggers.Add(entry);
    }

    public void Off()
    {
        gameObject.SetActive(false);
    }

    public void On()
    {
        gameObject.SetActive(true);
    }
}
