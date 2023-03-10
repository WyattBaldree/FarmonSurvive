using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PerkList : MonoBehaviour
{
    [SerializeField]
    GameObject _perkPlatePrefab;

    List<GameObject> _plates = new List<GameObject>();

    internal void GeneratePerkList(Farmon farmon, InfoBox infoBox)
    {
        foreach (GameObject g in _plates)
        {
            Destroy(g);
        }

        _plates.Clear();

        foreach (var perk in farmon.perkList)
        {
            GameObject newPlate = Instantiate(_perkPlatePrefab, transform);
            _plates.Add(newPlate);
            PerkPlate perkPlate = newPlate.GetComponent<PerkPlate>();

            Perk perkInfo = Perk.perkDictionary[perk.Key];

            perkPlate.Text.text = perkInfo.PerkName;

            perkPlate.HoverForInfo.infoBox = infoBox;
            perkPlate.HoverForInfo.plateText = perkInfo.PerkName;
            perkPlate.HoverForInfo.descriptionText = perkInfo.PerkDescription[perk.Value - 1];

            if (perkInfo.MaxLevel > 1)
            {
                perkPlate.Text.text += " " + perk.Value;
                perkPlate.HoverForInfo.plateText += " " + perk.Value;
            }
        }
    }
}
