using Assets.Scripts.Timer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public Farmon.TeamEnum Team = Farmon.TeamEnum.team1;

    public Farmon Owner = null;

    public AttackData AttackData = null;

    public int Pierce = 1;

    public float LifeTime = 5f;

    public AudioClip DestroySound;

    public Farmon SpecificTarget = null;

    public UnityEvent EventDestroy = new UnityEvent();

    List<Farmon> hitFarmonList = new List<Farmon>();

    Timer hitStopTimer = new Timer();

    Rigidbody rb;
    Collider col;
    AudioSource audioSource;

    bool destroyed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(AttackData attackData, Farmon owner, Farmon.TeamEnum team)
    {
        AttackData = attackData;

        if (audioSource && AttackData.InitialSound)
        {
            audioSource.clip = AttackData.InitialSound;
            audioSource.volume = .3f;
            audioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (destroyed) return;

        Farmon farmon = collision.GetComponent<Farmon>();

        if (farmon && !farmon.dead && farmon.team != Team && !hitFarmonList.Contains(farmon))
        {
            if(SpecificTarget != null && SpecificTarget != farmon)
            {
                return;
            }

            Vector3 center = col.bounds.center;

            bool hit = false;
            if (rb)
            {
                hit = farmon.TakeDamage(AttackData, center, H.Flatten(rb.velocity).normalized, Owner);
            }
            else
            {
                Vector3 awayFromProjectile = H.Flatten(farmon.transform.position - transform.position).normalized;
                hit = farmon.TakeDamage(AttackData, center, awayFromProjectile, Owner);
            }

            if (hit)
            {
                


                OnHitDelegate(farmon);

                if (AttackData.HitStopTime > 0)
                {
                    HitStop();
                }

                Pierce--;
                if (Pierce <= 0)
                {
                    BeginDestroy();
                }

                if (audioSource)
                {
                    if (destroyed && DestroySound)
                    {
                        audioSource.clip = DestroySound;
                        audioSource.volume = .3f;
                        audioSource.Play();
                    }
                    else if (AttackData.HitSound)
                    {
                        audioSource.clip = AttackData.HitSound;
                        audioSource.volume = .3f;
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

        LifeTime -= Time.deltaTime;

        if (LifeTime <= 0 || Vector3.Distance(transform.position, Player.instance.transform.position) > 50)
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

        LifeTime += AttackData.HitStopTime - hitStopTimer.GetTime();

        hitStopTimer.SetTime(AttackData.HitStopTime);
    }

    public delegate void OnHit(Farmon hitFarmon);

    public OnHit OnHitDelegate = (unit) => { };
}