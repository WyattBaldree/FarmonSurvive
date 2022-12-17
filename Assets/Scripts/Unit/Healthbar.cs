using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bar : MonoBehaviour
{
    public SpriteRenderer barSpriteRenderer;
    public SpriteRenderer barDelayedSpriteRenderer;

    public float delayedBarDelay = 2.0f;
    private float delayedBarDelayCurrent;

    public float delayedBarSpeed = .01f;

    [Range(0f, 1f)] public float percentController = 1f;

    private float percent = 1f;

    private float delayedPercent = 1f;

    private void OnValidate()
    {
        SetPercent(percentController);
    }

    private void Update()
    {
        delayedBarDelayCurrent -= Time.deltaTime;

        if(delayedBarDelayCurrent <= 0)
        {
            SetDelayedPercent(Mathf.SmoothStep(delayedPercent, percent, Time.deltaTime * delayedBarSpeed));
        }
    }

    public void SetPercent(float newPercent)
    {
        percent = newPercent;

        barSpriteRenderer.transform.localScale = new Vector2(percent, 1);

        if (percent < delayedPercent)
        {
            delayedBarDelayCurrent = delayedBarDelay;
        }
        else
        {
            SetDelayedPercent(percent);
        }
    }

    void SetDelayedPercent(float newPercent)
    {
        delayedPercent = newPercent;

        barDelayedSpriteRenderer.transform.localScale = new Vector2(delayedPercent, 1);
    }
}
