using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public Farmon.TeamEnum team = Farmon.TeamEnum.player;

    public int damage = 1;

    public int pierce = 1;

    public float knockBack = 7.0f;

    public bool undodgeable = false;

    public UnityEvent EventDestroy = new UnityEvent();

    List<Farmon> hitFarmonList = new List<Farmon>();

    private void OnDestroy()
    {
        EventDestroy.Invoke();
    }

    private void OnTriggerEnter(Collider collision)
    {
        Farmon unit = collision.GetComponent<Farmon>();
        if (unit && unit.team != team && !hitFarmonList.Contains(unit))
        {
            unit.TakeDamage(damage, transform.position, knockBack, undodgeable);

            hitFarmonList.Add(unit);

            pierce--;
            if (pierce <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        if(Vector3.Distance(transform.position, Player.instance.transform.position) > 50)
        {
            Destroy(gameObject);
        }
    }
}