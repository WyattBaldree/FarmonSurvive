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

    internal void Setup(string text, float duration = 1f)
    {
        tmp.text = text;

        floatTimer.SetTime(duration);
    }

    private void Update()
    {
        if (floatTimer.Tick(Time.deltaTime)) 
        {
            Destroy(gameObject);
        }

        tmp.color = new Color(1, 1, 1, floatTimer.Percent);

        transform.Translate(Vector3.up * 3 * Time.deltaTime);
    }
}
