using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickUp : MonoBehaviour, IPointerDownHandler
{
    public DropPointTypes DropPointType;

    public bool Locked = false;

    RectTransform rectTransform;

    Vector3 defaultScale;

    DropPoint _currentDropPoint;

    [SerializeField]
    protected Image clickableImage;

    public DropPoint CurrentDropPoint
    {
        get => _currentDropPoint;
    }

    private void Awake()
    {
        //Fetch the RectTransform from the GameObject
        rectTransform = GetComponent<RectTransform>();
    }

    public void Start()
    {
        defaultScale = rectTransform.localScale;
    }

    public virtual void ReturnToDropPoint()
    {
        Drop();
        SetDropPoint(_currentDropPoint);
    }

    private void Drop()
    {
        rectTransform.localScale = defaultScale;

        clickableImage.raycastTarget = true;
    }

    public virtual void Grab()
    {
        rectTransform.localScale = defaultScale * 1.2f;

        clickableImage.raycastTarget = false;

        //Let the drop point know it's been grabbed.
        CurrentDropPoint.PickUpGrabbed();
    }

    public void SetDropPoint(DropPoint dropPoint)
    {
        if (_currentDropPoint) _currentDropPoint.SetPickUp(null);

        Drop();
        _currentDropPoint = dropPoint;
        _currentDropPoint.SetPickUp(this);
        transform.SetParent(_currentDropPoint.transform);
        transform.localPosition = Vector3.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Locked || _currentDropPoint.Locked) return;
        PickUpper.instance.Grab(this);
    }
}
