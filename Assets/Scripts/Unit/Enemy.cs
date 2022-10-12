using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    public static List<Enemy> s_enemyList = new List<Enemy>();

    private int health = 3;

    private void Awake()
    {
        s_enemyList.Add(this);
    }

    protected override void Start()
    {
        base.Start();

        targetTransform = Player.instance.transform;
        targetDistance = 0;
    }

    private void OnDestroy()
    {
        s_enemyList.Remove(this);
    }

    protected override void Update()
    {
        base.Update();

        MoveToTarget(speed);
    }

    public Vector3 GetUnitVectorToMe(Vector3 origin)
    {
        return (transform.position - origin).normalized;
    }

    public void ChangeHeath(int amount)
    {
        health += amount;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
