using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Farmon
{
    public static List<Enemy> s_enemyList = new List<Enemy>();

    protected override void Awake()
    {
        base.Awake();
        s_enemyList.Add(this);
    }

    protected override void Start()
    {
        base.Start();

        targetTransform = Player.instance.transform;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        s_enemyList.Remove(this);
    }
}
