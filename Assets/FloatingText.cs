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

    Timer floatTimer = new Timer();

    internal void Setup(string text, Color color, float duration = 1f)
    {
        tmp.text = text;
        tmp.color = color;

        floatTimer.SetTime(duration);
    }

    private void Update()
    {
        if (floatTimer.Tick(Time.deltaTime)) 
        {
            Destroy(gameObject);
        }

        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, floatTimer.Percent);

        transform.Translate(Vector3.up * 3 * Time.deltaTime);
    }
}
