using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class UnitController : MonoBehaviour
{
    public static UnitController instance;

    public GameObject ShadowPrefab;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one FarmonController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
