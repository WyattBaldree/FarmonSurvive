using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartingTab : MonoBehaviour
{
    Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();

        toggle.Select();
    }
}
