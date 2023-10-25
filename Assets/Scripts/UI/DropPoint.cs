using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum DropPointTypes
{
    Default,
    PreRoundScreen
}

public class DropPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DropPointTypes DropPointType;

    public bool Locked = false;

    public PickUp currentPickUp;

    public UnityEvent PickUpChangedEvent;

    public bool CurrentPickUpGrabbed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PickUpper.instance.EnterDropPoint(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PickUpper.instance.ExitDropPoint(this);
    }

    public virtual void SetPickUp(PickUp newPickUp)
    {
        currentPickUp = newPickUp;
        PickUpChangedEvent.Invoke();

        CurrentPickUpGrabbed = false;
    }

    internal void PickUpGrabbed()
    {
        CurrentPickUpGrabbed = true;
        PickUpChangedEvent.Invoke();
    }
}
