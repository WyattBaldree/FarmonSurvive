using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LevelController : MonoBehaviour
{
    public static LevelController Instance;

    public LevelBlock[,,] BlockArray;

    public float gridSize = 1f;
    public Vector3Int levelSize = new Vector3Int();

    public Transform BoundsObject;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        Instance = this;
        BlockArray = new LevelBlock[levelSize.x, levelSize.y, levelSize.z];
        for (int i = 0; i < BlockArray.GetLength(0); i++)
        {
            for (int j = 0; j < BlockArray.GetLength(1); j++)
            {
                for (int k = 0; k < BlockArray.GetLength(2); k++)
                {
                    BlockArray[i, j, k] = null;
                    
                }
            }
        }

        foreach(LevelBlock block in LevelBlock.levelBlockList)
        {
            block.AddToBlockMatrix(this);
        }
    }

    private void OnValidate()
    {
        if (BoundsObject)
        {
            BoundsObject.position = (Vector3)levelSize * gridSize / 2;
            BoundsObject.localScale = (Vector3)levelSize * gridSize;
        }
    }

    public LevelBlock GetBlockArray(Vector3Int vInt)
    {
        return BlockArray[vInt.x, vInt.y, vInt.z];
    }

    public void SetBlockArray(Vector3Int vInt, LevelBlock levelBlock)
    {
        Vector3Int levelSize = LevelController.Instance.levelSize;
        if (vInt.x >= 0 && vInt.x < levelSize.x &&
            vInt.y >= 0 && vInt.y < levelSize.y &&
            vInt.z >= 0 && vInt.z < levelSize.z)
        {
            //If this is a valid position, add the block to the block array.
            BlockArray[vInt.x, vInt.y, vInt.z] = levelBlock;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)){
            ResetLevel();
        }
    }

    private void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        
    }
}
