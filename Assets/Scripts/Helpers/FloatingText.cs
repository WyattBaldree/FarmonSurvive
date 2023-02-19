using Assets.Scripts.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField]
    TextMeshPro tmp;

    DestroyAfterTime destroyAfterTime;

    internal void Setup(string text, Color color, float duration = 1f)
    {
        tmp.text = text;
        tmp.color = color;

        destroyAfterTime = gameObject.AddComponent<DestroyAfterTime>();
        destroyAfterTime.duration = duration;
    }

    private void Update()
    {
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, Mathf.Min(1, destroyAfterTime.FloatTimer.Percent * 1.2f));

        transform.Translate(Vector3.up * 2 * Time.deltaTime);
    }
}
