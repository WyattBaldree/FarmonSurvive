using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour
{
    Timer DisableTimer = new Timer();

    [SerializeField]
    public float disableTime = 1f;

    private void OnEnable()
    {
        DisableTimer.SetTime(disableTime);
    }

    private void Update()
    {
        if (DisableTimer.Tick(Time.deltaTime))
        {
            gameObject.SetActive(false);
        }
    }
}
