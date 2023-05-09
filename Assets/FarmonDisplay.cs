using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmonDisplay : MonoBehaviour
{
    public FarmonHud DisplayFarmonHud;

    public Camera DisplayCamera;

    public GameObject LightFlash;

    public void Flash()
    {
        LightFlash.SetActive(true);
    }
}
