using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmonController : MonoBehaviour
{
    public static FarmonController instance;

    public static bool paused = false;

    public GameObject ShadowPrefab;

    public GameObject FloatingTextPrefab;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one FarmonController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
    }
}
