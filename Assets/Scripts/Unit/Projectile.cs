using Assets.Scripts.Timer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public Farmon.TeamEnum team = Farmon.TeamEnum.player;

    public Farmon owner = null;

    public int damage = 1;

    public int pierce = 1;

    public float hitStunTime = .3f;

    public float knockBack = 7.0f;

    public float lifeTime = 5f;

    public bool undodgeable = false;

    public Farmon specificTarget = null;

    public UnityEvent EventDestroy = new UnityEvent();

    List<Farmon> hitFarmonList = new List<Farmon>();

    Timer hitStopTimer = new Timer();

    Rigidbody rb;

    private void OnDestroy()
    {
        EventDestroy.Invoke();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        Farmon farmon = collision.GetComponent<Farmon>();

        if (farmon && farmon.team != team && !hitFarmonList.Contains(farmon))
        {
            if(specificTarget != null && specificTarget != farmon)
            {
                return;
            }

            bool hit = false;
            if (rb)
            {
                hit = farmon.TakeDamage(damage, rb.velocity.normalized, owner, hitStunTime, knockBack, undodgeable);
            }
            else
            {
                Vector3 awayFromProjectile = (farmon.transform.position - transform.position).normalized;
                hit = farmon.TakeDamage(damage, awayFromProjectile, owner, hitStunTime, knockBack, undodgeable);
            }

            //Spawn a hit effect.
            Vector3 farmonToMe = transform.position + GetComponent<Collider>().bounds.center - farmon.transform.position;
            GameObject hitEffect = Instantiate(FarmonController.instance.HitEffectPrefab, farmon.transform);
            hitEffect.transform.position = farmon.transform.position + farmonToMe.normalized * farmon.sphereCollider.radius;
            hitEffect.transform.localScale = (.2f + 2.5f * hitStunTime) * Vector3.one;


            OnHitDelegate(farmon);

            hitFarmonList.Add(farmon);

            pierce--;
            if (pierce <= 0)
            {
                Destroy(gameObject);
                return;
            }

            if (rb && hit && hitStunTime > 0)
            {
                HitStop();
            }
        }
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0 || Vector3.Distance(transform.position, Player.instance.transform.position) > 50)
        {
            Destroy(gameObject);
            return;
        }

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    public void HitStop()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;

        lifeTime += hitStunTime - hitStopTimer.GetTime();

        hitStopTimer.SetTime(hitStunTime);
    }

    public delegate void OnHit(Farmon hitFarmon);

    public OnHit OnHitDelegate = (unit) => { };
}