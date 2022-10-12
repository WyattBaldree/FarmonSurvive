using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyController : MonoBehaviour
{
    public GameObject BatPrefab;

    [HideInInspector]
    public Enemy ClosestEnemy = null;

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

    private void Update()
    {
        GetEnemyClosestToPlayer();
    }

    public void GetEnemyClosestToPlayer()
    {
        ClosestEnemy = null;
        float closestEnemyDistance = float.MaxValue;
        foreach (Enemy e in Enemy.s_enemyList)
        {
            float distanceToPlayer = Vector3.Distance(e.transform.position, Player.instance.transform.position);

            if (distanceToPlayer < closestEnemyDistance)
            {
                ClosestEnemy = e;
                closestEnemyDistance = distanceToPlayer;
            }
        }
    }

    IEnumerator SpawnBatsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);

            Vector3 spawnLocation = Quaternion.Euler(0, 0, Random.Range(0, 359)) * new Vector3(0, 15, 0);

            Instantiate(BatPrefab, spawnLocation, Quaternion.identity);
        }
    }
}
