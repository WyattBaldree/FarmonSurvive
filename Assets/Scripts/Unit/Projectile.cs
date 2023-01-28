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

    public AudioClip CreateSound;
    public AudioClip HitSound;
    public AudioClip DestroySound;

    private AudioSource audioSource;

    public Farmon specificTarget = null;

    public UnityEvent EventDestroy = new UnityEvent();

    List<Farmon> hitFarmonList = new List<Farmon>();

    Timer hitStopTimer = new Timer();

    Rigidbody rb;

    bool destroyed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource && CreateSound)
        {
            audioSource.clip = CreateSound;
            audioSource.volume = .4f;
            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (destroyed) return;

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
                hit = farmon.TakeDamage(damage, H.Flatten(rb.velocity).normalized, owner, hitStunTime, knockBack, undodgeable);
            }
            else
            {
                Vector3 awayFromProjectile = H.Flatten(farmon.transform.position - transform.position).normalized;
                hit = farmon.TakeDamage(damage, awayFromProjectile, owner, hitStunTime, knockBack, undodgeable);
            }

            if (hit)
            {
                //Spawn a hit effect.
                Vector3 farmonToMe = transform.position + GetComponent<Collider>().bounds.center - farmon.transform.position;
                GameObject hitEffect = Instantiate(FarmonController.instance.HitEffectPrefab, farmon.transform);
                hitEffect.transform.position = farmon.transform.position + farmonToMe.normalized * farmon.sphereCollider.radius;
                hitEffect.transform.localScale = (.2f + 2.5f * hitStunTime) * Vector3.one;


                OnHitDelegate(farmon);

                if (hitStunTime > 0)
                {
                    HitStop();
                }

                pierce--;
                if (pierce <= 0)
                {
                    BeginDestroy();
                }

                if (audioSource)
                {
                    if (destroyed && DestroySound)
                    {
                        audioSource.clip = HitSound;
                        audioSource.volume = .4f;
                        audioSource.Play();
                    }
                    else if (HitSound)
                    {
                        audioSource.clip = HitSound;
                        audioSource.volume = .4f;
                        audioSource.Play();
                    }
                }
                
            }


            hitFarmonList.Add(farmon);
        }
    }

    private void BeginDestroy()
    {
        destroyed = true;

        foreach(SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }

        Collider collider = GetComponent<Collider>();

        if (collider)
        {
            collider.enabled = false;
        }

        if (rb)
        {
            rb.isKinematic = true;
        }

        EventDestroy.Invoke();
    }

    private void Update()
    {
        if (destroyed)
        {
            if (!audioSource.isPlaying)
            {
                Destroy(gameObject);
            }
            return;
        };

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0 || Vector3.Distance(transform.position, Player.instance.transform.position) > 50)
        {
            BeginDestroy();
            return;
        }

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    public void HitStop()
    {
        if(rb) rb.constraints = RigidbodyConstraints.FreezeAll;

        lifeTime += hitStunTime - hitStopTimer.GetTime();

        hitStopTimer.SetTime(hitStunTime);
    }

    public delegate void OnHit(Farmon hitFarmon);

    public OnHit OnHitDelegate = (unit) => { };
}