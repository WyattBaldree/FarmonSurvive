using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [HideInInspector]
    public Timer FloatTimer = new Timer();

    public float duration = 1f;
    // Start is called before the first frame update
    void Start()
    {
        FloatTimer.SetTime(duration);
    }

    // Update is called once per frame
    void Update()
    {
        if (FloatTimer.Tick(Time.deltaTime)) 
        {
            Destroy(gameObject);
        }
    }
}
