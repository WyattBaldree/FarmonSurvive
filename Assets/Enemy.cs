using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    public static List<Enemy> s_enemyList = new List<Enemy>();

    

    private void Start()
    {
        s_enemyList.Add(this);

        targetTransform = Player.instance.transform;
        targetDistance = 0;
    }

    private void OnDestroy()
    {
        s_enemyList.Remove(this);
    }

    public Vector3 GetUnitVectorToMe(Vector3 origin)
    {
        return (transform.position - origin).normalized;
    }
}
