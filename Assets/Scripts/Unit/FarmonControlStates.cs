using Assets.Scripts.States;
using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainState : StateMachineState
{
    Farmon farmon;

    public MainState(Farmon thisFarmon)
    {
        farmon = thisFarmon;
    }

    public override void Tick()
    {
        base.Tick();
        farmon.UpdateFarmonStateMachine();
        farmon.UpdateEffects();
        farmon.UpdateTimers();
    }
}

public class HitStopState : StateMachineState
{
    readonly Farmon farmon;
    readonly Timer hitStopTimer = new Timer();
    readonly Timer flashTimer = new Timer();

    private Vector3 spriteQuadOrigin;
    private Vector3 spriteHealthBarOrigin;

    HitStopState2 hitStopState2;
    SpriteRenderer spriteRenderer;

    AttackData _attackData;
    Vector3 _damageOrigin;

    bool flashFlag;

    public HitStopState(Farmon thisUnit, float hitStopTime, AttackData attackData, Vector3 damageOrigin, Vector3 knockbackDirection)
    {
        farmon = thisUnit;

        _damageOrigin = damageOrigin;

        hitStopTimer.SetTime(hitStopTime);

        flashTimer.SetTime(.1f);
        flashTimer.autoReset = true;

        spriteRenderer = farmon.Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();

        if (attackData != null)
        {
            _attackData = attackData;

            Vector3 bounceVector = knockbackDirection * attackData.Knockback;

            if (bounceVector.magnitude > .01f)
            {
                hitStopState2 = new HitStopState2(farmon, attackData, damageOrigin, bounceVector);
            }
        }
    }

    public override void Enter()
    {
        base.Enter();

        farmon.rb.isKinematic = true;

        farmon.Hud.Animator.speed = 0;
        farmon.Hud.PositionQuad.enabled = false;

        spriteQuadOrigin = farmon.Hud.SpriteQuad.transform.position;
        spriteHealthBarOrigin = farmon.Hud.HealthBar.transform.position;

        spriteRenderer.color = new Color(.6f, .6f, .6f, 1);

        farmon.Hud.AudioSource.clip = FarmonController.instance.HitSound;
        farmon.Hud.AudioSource.volume = .3f;
        farmon.Hud.AudioSource.Play();
    }

    public override void Exit()
    {
        base.Exit();

        farmon.rb.isKinematic = false;

        farmon.Hud.Animator.speed = 1;
        farmon.Hud.PositionQuad.enabled = true;

        spriteRenderer.color = new Color(1, 1, 1, 1);

        farmon.Hud.SpriteQuad.transform.position = spriteQuadOrigin;
        farmon.Hud.HealthBar.transform.position = spriteHealthBarOrigin;

        //Take damae upon exiting this state
        if (_attackData != null) farmon.TakeDamage(_attackData, _damageOrigin);
    }

    public override void Tick()
    {
        base.Tick();

        Vector3 cameraRight = Camera.main.transform.right;
        Vector3 cameraUp = Camera.main.transform.up;

        float noise1X = Mathf.PerlinNoise(Time.time * 20, 0) * 2 - 1;
        float noise1Y = Mathf.PerlinNoise(0, Time.time * 20) * 2 - 1;
        float noise2X = Mathf.PerlinNoise(100 + Time.time * 20, 0) * 2 - 1;
        float noise2Y = Mathf.PerlinNoise(0, 100 + Time.time * 20) * 2 - 1;

        float diminish = .75f * (1 - hitStopTimer.Percent);

        farmon.Hud.SpriteQuad.transform.position = spriteQuadOrigin + (0.3f - diminish) * noise1X * cameraRight + (0.2f - diminish) * noise1Y * cameraUp;
        farmon.Hud.HealthBar.transform.position = spriteHealthBarOrigin + (0.3f - diminish) * noise2X * cameraRight + (0.2f - diminish) * noise2Y * cameraUp;

        if (flashTimer.Tick(Time.deltaTime))
        {
            if (flashFlag)
            {
                spriteRenderer.color = new Color(.6f, .6f, .6f, 1);
            }
            else
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
            flashFlag = !flashFlag;
        }

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            if (hitStopState2 != null)
            {
                farmon.SetControlState(hitStopState2);
            }
            else
            {
                farmon.SetControlState(new MainState(farmon));
            }
        }
    }
}

public class HitStopState2 : StateMachineState
{
    readonly Farmon farmon;
    readonly Vector3 _damageOrigin;
    readonly Vector3 _bounceVector;
    readonly AttackData _attackData;

    readonly Timer hitStopTimer = new Timer();


    public HitStopState2(Farmon thisUnit, AttackData attackData, Vector3 damageOrigin, Vector3 bounceVector)
    {
        farmon = thisUnit;
        _bounceVector = bounceVector;
        _damageOrigin = damageOrigin;

        hitStopTimer.SetTime(0.5f);

        _attackData = attackData;
    }

    public override void Enter()
    {
        base.Enter();

        farmon.TakeDamage(_attackData, _damageOrigin);

        farmon.rb.AddForce(farmon.dead ? _bounceVector * 3 : _bounceVector, ForceMode.Impulse);
    }

    public override void Tick()
    {
        base.Tick();

        if (hitStopTimer.Tick(Time.deltaTime))
        {
            farmon.SetControlState(new MainState(farmon));
        }

    }
}

public class DieState : StateMachineState
{
    Farmon farmon;

    SpriteRenderer spriteRenderer;

    readonly Timer dieTimer = new Timer();

    readonly Timer flashTimer = new Timer();
    bool flashFlag;

    public DieState(Farmon _farmon)
    {
        farmon = _farmon;

        dieTimer.SetTime(1f);

        flashTimer.SetTime(0.1f);
        flashTimer.autoReset = true;

        spriteRenderer = farmon.Hud.SpriteQuad.GetComponentInChildren<SpriteRenderer>();
    }

    public override void Enter()
    {
        farmon.dead = true;

        spriteRenderer.color = new Color(.6f, .6f, .6f, 1);

        foreach (LookAtCamera lac in farmon.GetComponentsInChildren<LookAtCamera>())
        {
            lac.enabled = false;
            lac.transform.localEulerAngles = new Vector3(5, lac.transform.localEulerAngles.y, 0);
        }

        farmon.Hud.AudioSource.clip = FarmonController.instance.DieSound;
        farmon.Hud.AudioSource.volume = .3f;
        farmon.Hud.AudioSource.Play();

        farmon.rb.AddForce(Vector3.up * 3, ForceMode.Impulse);
    }

    public override void Tick()
    {
        base.Tick();

        farmon.Hud.SpriteQuad.transform.Rotate(new Vector3(0, 1, 0), 400f * Time.deltaTime);

        farmon.maxSpeed = 0;
        farmon.MovementIdle();

        farmon.transform.localScale -= .35f * Time.deltaTime * Vector3.one;


        if (dieTimer.Tick(Time.deltaTime))
        {
            farmon.gameObject.SetActive(false);
            return;
        }

        if (flashTimer.Tick(Time.deltaTime))
        {
            if (flashFlag)
            {
                spriteRenderer.color = new Color(.6f, .6f, .6f, 1);
            }
            else
            {
                spriteRenderer.color = new Color(1, 1, 1, 1);
            }
            flashFlag = !flashFlag;
        }
    }
}
