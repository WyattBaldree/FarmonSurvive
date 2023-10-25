using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmonDisplayController : MonoBehaviour
{
    public static FarmonDisplayController instance;

    [SerializeField]
    private GameObject FarmonDisplayPrefab;

    [SerializeField]
    private float rendererDistance = 4;

    [SerializeField]
    private float rendererOffset = 50;

    List<FarmonDisplay> farmonDisplayList = new List<FarmonDisplay>();

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one FarmonDisplayController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public FarmonDisplay GetFarmonDisplay()
    {
        GameObject go = Instantiate(FarmonDisplayPrefab, transform);
        FarmonDisplay fd = go.GetComponent<FarmonDisplay>();

        farmonDisplayList.Add(fd);

        fd.Destroyed.AddListener(RemoveFromList);

        UpdateTransforms();

        return fd;
    }

    private void RemoveFromList(FarmonDisplay caller)
    {
        farmonDisplayList.Remove(caller);
    }

    private void UpdateTransforms()
    {
        for(int i = 0; i < farmonDisplayList.Count; i++)
        {
            farmonDisplayList[i].transform.position = new Vector3(rendererOffset + rendererDistance * i, 0, 0);
        }
    }
}
