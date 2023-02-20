using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverForInfo : MonoBehaviour, IPointerEnterHandler
{
    public InfoBox infoBox;

    public string plateText;

    public string descriptionText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoBox.SetText(plateText, descriptionText);
    }
}
