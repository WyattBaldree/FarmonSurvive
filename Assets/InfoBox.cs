using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoBox : MonoBehaviour
{
    public TextMeshProUGUI plateText;
    public TextMeshProUGUI descriptionText;

    [SerializeField]
    string startingText;

    internal void SetText(string newPlateText, string newDescriptionText)
    {
        plateText.text = newPlateText;
        descriptionText.text = startingText + newDescriptionText;
    }
}
