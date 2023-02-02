using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H
{
    public static Vector3 Flatten(Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3Int Vector3ToGridPosition(Vector3 v, float gridSize)
    {
        int x = Mathf.FloorToInt(v.x / gridSize);
        int y = Mathf.FloorToInt(v.y / gridSize);
        int z = Mathf.FloorToInt(v.z / gridSize);

        return new Vector3Int(x, y, z);
    }

    public static Vector3 GridPositionToVector3(Vector3Int v, float gridSize)
    {
        float x = v.x * gridSize;
        float y = v.y * gridSize;
        float z = v.z * gridSize;

        return new Vector3(x, y, z);
    }
}
