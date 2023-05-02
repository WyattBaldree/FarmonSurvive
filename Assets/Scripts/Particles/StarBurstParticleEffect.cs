using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class StarBurstParticleEffect : MonoBehaviour
{
    public ParticleSystem PS;

    public UnityEvent StarEnteredEvent = new UnityEvent();

    private void OnParticleTrigger()
    {
        if(!PS) PS = GetComponent<ParticleSystem>();

        if (!PS) return;

        List<ParticleSystem.Particle> enterList = new List<ParticleSystem.Particle>();
        int numEnter = PS.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList);

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enterList[i];
            p.remainingLifetime = 0;
            enterList[i] = p;
        }

        PS.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterList);

        StarEnteredEvent.Invoke();
    }
}
