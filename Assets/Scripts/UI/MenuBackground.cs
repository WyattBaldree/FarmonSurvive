using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackground : MonoBehaviour
{
    private void Start()
    {
        MenuController.instance.MenuStateChanged.AddListener(UpdateActive);
    }

    private void UpdateActive()
    {
        gameObject.SetActive(MenuController.instance.MenuOpen);
    }
}
