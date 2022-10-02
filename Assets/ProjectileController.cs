using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ProjectileController : MonoBehaviour
{
    public GameObject defaultProjectilePrefab;

    public static ProjectileController instance;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one ProjectileController");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

}
