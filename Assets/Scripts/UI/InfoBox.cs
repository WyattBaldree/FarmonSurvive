using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoBox : MonoBehaviour
{
    public TextMeshProUGUI plateText;
    public DynamicText descriptionText;

    [SerializeField]
    string startingText;

    internal void SetText(string newPlateText, string newDescriptionText)
    {
        if(plateText) plateText.text = newPlateText;
        descriptionText.SetText(startingText + newDescriptionText);
    }
}
