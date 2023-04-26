using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor;

public class SpriteEffects : MonoBehaviour
{

    [SerializeField, Range(0.0f, 1.0f)]
    float _whiteOut = 0;

    Material _material;

    public bool _white = false;

    public bool isUi;
    RectTransform rectTransform;

    float GetUnitHeight()
    {
        if (rectTransform)
        {
            return rectTransform.rect.height;
        }
        else
        {
            SpriteRenderer r = GetComponent<SpriteRenderer>();
            if (r)
            {
                return transform.lossyScale.y * GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            }
            else
            {
                return 1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Renderer r = GetComponent<Renderer>();

        if (r)
        {
            _material = r.material;
        }
        else
        {
            Image i = GetComponent<Image>();

            //On start, create a material instance to modify at runtime.
            _material = Instantiate(i.material);
            i.material = _material;

            rectTransform = GetComponent<RectTransform>();
        }

        if (_white)
        {
            _whiteOut = Mathf.SmoothStep(_whiteOut, 1, Time.deltaTime * 6.5f);
        }
        else
        {
            _whiteOut = Mathf.SmoothStep(_whiteOut, 0, Time.deltaTime * 6.5f);
        }

        _material.SetFloat("_WhiteOut", _whiteOut);
    }

    public void TurnWhite()
    {
        _white = true;
    }

    public void TurnClear()
    {
        _white = false;
    }

    public void BumpAnimation()
    {
        float h = GetUnitHeight();
        transform.DOPunchScale(Vector3.down * .25f, .35f, 1);
        transform.DOPunchPosition(Vector3.down * h * .1f, .35f, 1);
    }

    public void EatAnimation()
    {
        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(transform.DOPunchPosition(Vector3.down * h * .1f, .45f, 1));
        // Insert a scale tween for the whole duration of the Sequence
        mySequence.Append(transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(transform.DOPunchPosition(Vector3.down * h * .1f, .45f, 1));
    }

    public void JumpAnimation()
    {
        float h = GetUnitHeight();

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOScaleY(.75f, .25f));
        mySequence.Append(transform.DOLocalJump(Vector3.zero, .45f * h, 1, .6f));
        mySequence.Join(transform.DOScaleY(1, .15f));

        mySequence.Append(transform.DOScaleY(.75f, .25f));
        mySequence.Append(transform.DOLocalJump(Vector3.zero, .45f * h, 1, .6f));
        mySequence.Join(transform.DOScaleY(1, .15f));
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
            if (GUILayout.Button("SpriteEffect Jump"))
            {
                se.JumpAnimation();
            }
            if (GUILayout.Button("SpriteEffect Toggle White"))
            {
                se._white = !se._white;
            }
        }
    }
}
