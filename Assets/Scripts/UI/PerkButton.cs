using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkButton : MonoBehaviour
{
    public HoverForInfo HoverForInfo;
    public TextMeshProUGUI Text;
    public Button button;
    public Farmon farmon;
    public Perk perk;

    public void GivePerk()
    {
        farmon.AddPerk(perk);
    }
}
