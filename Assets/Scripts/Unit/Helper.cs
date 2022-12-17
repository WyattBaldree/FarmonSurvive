using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H
{
    public static Vector3 Flatten(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }
}
