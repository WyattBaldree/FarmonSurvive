using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor;
using Assets.Scripts.Timer;

public class SpriteEffects : MonoBehaviour
{

    [SerializeField, Range(0.0f, 1.0f)]
    float _whiteOut = 0;

    Material _material;

    public bool White = false;
    public float ShakeAmount = 0f;

    SpriteRenderer spriteRenderer;

    [SerializeField]
    AudioSource audioSource;

    Timer ShakeTimer = new Timer();

    [Header("Starburst")]
    [SerializeField]
    StarBurstParticleEffect StarBurstEffect;

    [SerializeField]
    AudioClip starTwinkleAudio;


    private void Start()
    {
        spriteRenderer = GetComponent<FarmonHud>().SpriteQuad.GetComponentInChildren<SpriteRenderer>();

        StarBurstEffect.StarEnteredEvent.AddListener(StarBurstParticleEntered);

        ShakeTimer.SetTime(.1f);
        ShakeTimer.autoReset = true;
    }

    float GetUnitHeight()
    {
        return transform.lossyScale.y * spriteRenderer.sprite.bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        //White
        _material = spriteRenderer.material;

        if (White)
        {
            _whiteOut = Mathf.Lerp(_whiteOut, 1, Time.deltaTime * 6.5f);
        }
        else
        {
            _whiteOut = Mathf.Lerp(_whiteOut, 0, Time.deltaTime * 6.5f);
        }

        _material.SetFloat("_WhiteOut", _whiteOut);

        //Shaking
        if (ShakeAmount > 0)
        {
            if (ShakeTimer.Tick(Time.deltaTime))
            {
                spriteRenderer.transform.DOComplete();

                float h = GetUnitHeight();

                spriteRenderer.transform.DOPunchPosition(Random.insideUnitSphere.normalized * h * .05f * ShakeAmount, .2f, 1);
            }
        }
    }

    public void TurnWhite()
    {
        White = true;
    }

    public void TurnClear()
    {
        White = false;
    }

    public void BumpAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();
        spriteRenderer.transform.DOPunchScale(Vector3.down * .25f, .35f, 1);
        spriteRenderer.transform.DOPunchPosition(Vector3.down * h * .1f, .35f, 1);
    }

    public void BumpAnimationRandom()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();
        //spriteRenderer.transform.DOPunchScale(Random.insideUnitSphere.normalized * .25f, .35f, 1);
        spriteRenderer.transform.DOPunchPosition(Random.insideUnitSphere.normalized * h * .1f, .2f, 1);
    }

    public void EatAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(spriteRenderer.transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(spriteRenderer.transform.DOPunchPosition(Vector3.down * h * .1f, .45f, 1));
        // Insert a scale tween for the whole duration of the Sequence
        mySequence.Append(spriteRenderer.transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(spriteRenderer.transform.DOPunchPosition(Vector3.down * h * .1f, .45f, 1));
    }

    public void JumpJoyAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(spriteRenderer.transform.DOScaleY(.75f, .25f));
        mySequence.Append(spriteRenderer.transform.DOLocalJump(Vector3.zero, .45f * h, 1, .6f));
        mySequence.Join(spriteRenderer.transform.DOScaleY(1, .15f));

        mySequence.Append(spriteRenderer.transform.DOScaleY(.75f, .25f));
        mySequence.Append(spriteRenderer.transform.DOLocalJump(Vector3.zero, .45f * h, 1, .6f));
        mySequence.Join(spriteRenderer.transform.DOScaleY(1, .15f));
    }

    public void JumpScaredAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        //mySequence.Append(spriteRenderer.transform.DOScaleY(.75f, .1f));
        mySequence.Append(spriteRenderer.transform.DOPunchScale(Vector3.up * .2f, .3f, 1));
        mySequence.Join(spriteRenderer.transform.DOLocalJump(Vector3.zero, .2f * h, 1, .3f)); 
        //mySequence.Join(spriteRenderer.transform.DOScaleY(1, .15f));
        mySequence.Join(spriteRenderer.transform.DOPunchRotation(Vector3.back * 7f, .3f, 1, 1));
    }

    public void SquashAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(spriteRenderer.transform.DOScaleY(.7f, .4f));
        mySequence.Append(spriteRenderer.transform.DOScaleY(1, .25f));
    }

    public void SneezeAnimation()
    {
        spriteRenderer.transform.DOComplete();

        float h = GetUnitHeight();

        float breathTime = .2f, sneezeTime = .1f;

        Sequence mySequence = DOTween.Sequence();
        //Breath in
        mySequence.Append(spriteRenderer.transform.DOScaleY(1.1f, breathTime));
        mySequence.Join(spriteRenderer.transform.DOScaleX(.9f, breathTime));
        //mySequence.Join(spriteRenderer.transform.DORotate(Vector3.back * 7f, breathTime, RotateMode.LocalAxisAdd));
        //Quick pause
        mySequence.AppendInterval(.2f);
        // Sneeze
        mySequence.Append(spriteRenderer.transform.DOScaleY(.8f, sneezeTime));
        mySequence.Join(spriteRenderer.transform.DOScaleX(1.2f, sneezeTime));
        //mySequence.Join(spriteRenderer.transform.DORotate(Vector3.forward * 14f, sneezeTime, RotateMode.LocalAxisAdd));
        //Quick pause
        mySequence.AppendInterval(.1f);
        //Breath in
        mySequence.Append(spriteRenderer.transform.DOScaleY(1f, breathTime));
        mySequence.Join(spriteRenderer.transform.DOScaleX(1f, breathTime));
        //mySequence.Join(spriteRenderer.transform.DORotate(Vector3.back * 7f, breathTime, RotateMode.LocalAxisAdd));
    }

    public void StarBurst()
    {
        StarBurstEffect.PS.Play();
    }

    int starburstEnteredCount = 0;
    private void StarBurstParticleEntered()
    {
        BumpAnimationRandom();

        if (starburstEnteredCount % 4 == 0)
        {
            audioSource.clip = starTwinkleAudio;
            audioSource.Play();
        }

        starburstEnteredCount++;
    }
}

[CustomEditor(typeof(SpriteEffects), true)]
public class SpriteEffectsEditor : Editor
{
    public override void OnInspectorGUI()
    {

        SpriteEffects se = (SpriteEffects)target;

        base.DrawDefaultInspector();

        if (Application.isPlaying)
        {
            // Misc controls.
            GUILayout.Label("Sprite Effects:");
            if (GUILayout.Button("SpriteEffect Eat"))
            {
                se.EatAnimation();
            }
            if (GUILayout.Button("SpriteEffect Bump"))
            {
                se.BumpAnimation();
            }
            if (GUILayout.Button("SpriteEffect Bump Random"))
            {
                se.BumpAnimationRandom();
            }
            if (GUILayout.Button("SpriteEffect Jump Joy"))
            {
                se.JumpJoyAnimation();
            }
            if (GUILayout.Button("SpriteEffect Jump Scared"))
            {
                se.JumpScaredAnimation();
            }
            if (GUILayout.Button("SpriteEffect Sneeze"))
            {
                se.SneezeAnimation();
            }
            if (GUILayout.Button("SpriteEffect Toggle White"))
            {
                se.White = !se.White;
            }
        }
    }
}
