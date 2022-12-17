using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyController : MonoBehaviour
{
    public GameObject BatPrefab;


    public static EnemyController instance;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one EnemyController");
        instance = this;
    }

    private void Start()
    {
        StartCoroutine( SpawnBatsCoroutine() );
    }

    private void OnDestroy()
    {
        instance = null;
    }

    IEnumerator SpawnBatsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);

            Vector3 spawnLocation = Quaternion.Euler(0, 0, Random.Range(0, 359)) * new Vector3(0, 15, 0);

            Instantiate(BatPrefab, spawnLocation, Quaternion.identity);
        }
    }
}
