using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class EvolutionScreen : MonoBehaviour
{
    public static EvolutionScreen instance;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    InfoBox infoBox;

    FarmonDisplay farmonDisplay;

    [SerializeField]
    RawImage farmonImage;

    [SerializeField]
    PlayableDirector timelineDirector;
    
    [SerializeField]
    PlayableAsset playableAsset;

    Farmon oldFarmon;
    Farmon newFarmon;

    [SerializeField]
    Sprite oldSprite;
    [SerializeField]
    Sprite newSprite;

    bool switchingSprite = false;

    string oldName = "Scrimp";

    string newFarmonName = "Screevil";

    PlayableDirector evolutionAnimation;
    private bool animationComplete;

    public void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        farmonDisplay = FarmonDisplayController.instance.GetFarmonDisplay();
        farmonImage.texture = farmonDisplay.RenderTexture;

        farmonDisplay.DisplayCamera.transform.position = new Vector3(0, 0, -4.7f);
        farmonDisplay.DisplayCamera.fieldOfView = 59.2f;

        //Bind the evolution screens animation to the farmonDisplay and farmonHud
        var outputs = playableAsset.outputs;
        foreach (var itm in outputs)
        {
            Debug.Log(itm.streamName);
            if (itm.streamName == "FarmonDisplayAnimationTrack")
            {
                timelineDirector.SetGenericBinding(itm.sourceObject, farmonDisplay.GetComponent<Animator>());
            }
            else if (itm.streamName == "HudAnimationTrack")
            {
                timelineDirector.SetGenericBinding(itm.sourceObject, farmonDisplay.DisplayFarmonHud.GetComponent<Animator>());
            }
        }


    }

    private void OnEnable()
    {
        infoBox.SetText("", "What's this? " + oldName + " is changing!");
    }

    private void Update()
    {
        if (animationComplete)
        {
            if (Input.anyKeyDown)
            {
                if (infoBox.descriptionText.reading)
                {
                    infoBox.descriptionText.SkipReading();
                }
                else
                {
                    gameObject.SetActive(false);

                    string evolveMessage = newFarmon.nickname + " looks even stronger!";

                    LevelUpScreen.instance.Popup(newFarmon, evolveMessage, oldFarmon.Grit, oldFarmon.Power, oldFarmon.Agility, oldFarmon.Focus, oldFarmon.Luck, oldFarmon.attributePoints);
                }
            }
        }
    }

    private void LateUpdate()
    {
        //While true, one side of the sprite is oldSprite and the other side is newSprite
        if (switchingSprite)
        {
            Vector3 cameraForward = farmonDisplay.DisplayCamera.transform.forward;
            Vector3 spriteForward = farmonDisplay.DisplayFarmonHud.spriteRenderer.transform.forward;
            if (Vector3.Dot(cameraForward, spriteForward) > 0)
            {
                ShowOldFarmon();
            }
            else
            {
                ShowNewFarmon();
            }
        }
    }

    public void Popup(Farmon farmon, string _newFarmonName)
    {
        oldFarmon = farmon;
        oldName = farmon.farmonName;
        oldSprite = oldFarmon.mySpriteRenderer.sprite;

        gameObject.SetActive(true);

        newFarmonName = _newFarmonName;
        newFarmon = oldFarmon.Evolve(newFarmonName);
        newSprite = newFarmon.mySpriteRenderer.sprite;

        audioSource.clip = FarmonController.instance.LevelUpSound;
        audioSource.volume = .7f;
        audioSource.Play();

        infoBox.SetText("", "What's this? " + oldName + " is changing!");

        animationComplete = false;
    }

    public void EvolutionAnimationComplete()
    {
        infoBox.SetText("", "Wow! " + oldName + " has grown into a " + newFarmonName + "!");

        animationComplete = true;
    }

    public void ShowOldFarmon()
    {
        farmonDisplay.DisplayFarmonHud.SetSprite(oldSprite);
    }

    public void ShowNewFarmon()
    {
        farmonDisplay.DisplayFarmonHud.SetSprite(newSprite);
    }

    public void BeginSwitchingSprite()
    {
        switchingSprite = true;
    }

    public void StopSwitchingSprite()
    {
        switchingSprite = false;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        newFarmon.DistributeEvolvePerks();
    }
}