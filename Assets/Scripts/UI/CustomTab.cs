using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomTab : MonoBehaviour
{
    [SerializeField]
    GameObject activeGameObject;

    Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        activeGameObject.SetActive(toggle.isOn);
    }

    public void OnToggleChanged()
    {
        activeGameObject.SetActive(toggle.isOn);
    }
}
