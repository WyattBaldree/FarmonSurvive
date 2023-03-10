using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FarmonController : MonoBehaviour
{
    public static FarmonController instance;

    public static bool Paused
    {
        get
        {
            bool shouldPause = false;

            if (RoundController.Instance)
            {
                shouldPause = shouldPause || !RoundController.Instance.RoundPlaying;
            }

            if (MenuController.instance.MenuOpen)
            {
                shouldPause = true;
            }

            return shouldPause;
        }
    }

    public GameObject ShadowPrefab;
    public GameObject FloatingTextPrefab;
    public GameObject HitEffectPrefab;

    public AudioClip HitSound;
    public AudioClip HitSound2;
    public AudioClip DieSound;
    public AudioClip DodgeSound;
    public AudioClip DashSound;
    public AudioClip LevelUpSound;

    private void Awake()
    {
        Assert.IsNull(instance, "There should only be one FarmonController");
        instance = this;

        Perk.CreatePerks();
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
