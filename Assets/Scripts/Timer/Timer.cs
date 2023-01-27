using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Timer
{
    public class Timer
    {
        float currentTime = 0;

        private float maxTime = 0;

        private bool running = false;

        public bool autoReset = false;

        public void SetTime( float time )
        {
            maxTime = time;

            currentTime = time;

            running = true;
        }

        public bool Tick( float timePassed )
        {
            currentTime -= timePassed;
            if (currentTime <= 0 && running == true)
            {
                if (autoReset)
                {
                    currentTime += maxTime;
                }
                else
                {
                    running = false;
                }

                return true;
            }
            return false;
        }

        public float Percent
        {
            get{
                if(maxTime == 0)
                {
                    return 0;
                }
                return currentTime / maxTime; 
            }
        }

        public float GetTime()
        {
            return Mathf.Max(0, currentTime);
        }
    }
}
