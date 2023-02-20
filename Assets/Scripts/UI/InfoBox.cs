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
        plateText.text = newPlateText;
        descriptionText.SetText(startingText + newDescriptionText);
    }
}
