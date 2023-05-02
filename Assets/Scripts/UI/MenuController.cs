using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    public UnityEvent MenuStateChanged = new UnityEvent();

    public GameObject StartingMenu;

    bool _menuOpen = false;
    public bool MenuOpen
    {
        get
        {
            return _menuOpen;
        }
        set
        {
            if (_menuOpen != value)
            {
                _menuOpen = value;

                MenuStateChanged.Invoke();
            }
        }
    }

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Start()
    {
        MenuStateChanged.Invoke();

        if (StartingMenu) StartingMenu.SetActive(true);
    }

    private void Update()
    {
        // If any of this object's children are active, set menuOpen to true
        bool atLeastOneMenuOpen = false;
        for ( int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
            {
                atLeastOneMenuOpen = true;
                break;
            }
        }

        MenuOpen = atLeastOneMenuOpen;
    }
}
