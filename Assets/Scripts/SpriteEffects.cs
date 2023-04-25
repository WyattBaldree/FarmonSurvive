using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpriteEffects : MonoBehaviour
{

    [SerializeField, Range(0.0f, 1.0f)]
    float _whiteOut = 0;

    Material _material;

    bool _white = false;

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
        transform.DOPunchScale(Vector3.down * .25f, .35f, 1);
        transform.DOPunchPosition(Vector3.down * 15, .35f, 1);
    }

    public void EatAnimation()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(transform.DOPunchPosition(Vector3.down * 15, .45f, 1));
        // Insert a scale tween for the whole duration of the Sequence
        mySequence.Append(transform.DOPunchScale(Vector3.down * .25f, .45f, 1));
        mySequence.Join(transform.DOPunchPosition(Vector3.down * 15, .45f, 1));

        //transform.DOPunchScale(Vector3.down * .25f, .35f, 1);
        //transform.DOPunchPosition(Vector3.down * 15, .35f, 1);
        
    }

    public void SquashAnimation()
    {
        
    }
}
