using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverForInfo : MonoBehaviour, IPointerEnterHandler
{
    public InfoBox infoBox;

    [SerializeField]
    string plateText;

    [SerializeField]
    string descriptionText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoBox.SetText(plateText, descriptionText);
    }
}
