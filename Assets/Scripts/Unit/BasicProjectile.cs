using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This class represents a projectile basic attack from a farmon.
//Once basic attacks are released, they fly straight at the target and always collide.
//If the target no longer exists the projectile will continue in the direction it is moving
public class BasicProjectile : MonoBehaviour
{
    public uint TargetFarmonId;

    private Rigidbody rb;
    private Projectile projectileComponent;

    public float Velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileComponent = GetComponent<Projectile>();

        projectileComponent.HitEvent.AddListener(OnHitFarmon);
    }

    private void OnHitFarmon(Farmon hitFarmon)
    {
        Farmon targetFarmon = Farmon.GetFarmonInstanceFromLoadedID(TargetFarmonId, true);
        if (hitFarmon == targetFarmon)
        {
            //If we hit the farmon we are target,
            //destroy this component so the projectile can continue on as normal.
            Destroy(this);
            return;
        }
    }

    protected void Update()
    {
        //Get the target farmon from the loaded list, if it exists.
        Farmon targetFarmon = Farmon.GetFarmonInstanceFromLoadedID(TargetFarmonId, true);

        if (!targetFarmon)
        {
            //If at any point this BasicProjectile loses its target,
            //destroy this component so the projectile behaves as a normal projectile.
            Destroy(this);
            return;
        }

        //rb.AddForce(Velocity * targetFarmon.GetUnitVectorToMe(transform.position), ForceMode.VelocityChange);

        rb.velocity = Velocity * targetFarmon.GetUnitVectorToMe(transform.position);
    }
}
