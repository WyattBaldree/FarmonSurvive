using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBlock : MonoBehaviour
{
    public static List<LevelBlock> levelBlockList = new List<LevelBlock>();

    private void Awake()
    {
        levelBlockList.Add(this);
    }

    private void OnDestroy()
    {
        levelBlockList.Remove(this);
    }

    public void AddToBlockMatrix(LevelController levelController)
    {
        levelController.SetBlockArray(H.Vector3ToGridPosition(transform.position, levelController.gridSize), this);
    }
}
