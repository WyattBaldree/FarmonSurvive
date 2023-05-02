using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class EvolutionScreen : MonoBehaviour
{
    public static EvolutionScreen instance;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    InfoBox infoBox;

    [SerializeField]
    FarmonDisplay farmonDisplay;

    Farmon oldFarmon;
    Farmon newFarmon;

    [SerializeField]
    Sprite oldSprite;
    [SerializeField]
    Sprite newSprite;

    bool switchingSprite = false;

    string oldName = "Farmon";

    string newFarmonName = "NewFarmon";

    bool evolutionComplete = false;

    PlayableDirector evolutionAnimation;

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
    }

    private void OnEnable()
    {
        infoBox.SetText("", "What's this? " + oldName + " is changing!");
    }

    private void LateUpdate()
    {
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

        evolutionComplete = false;

        infoBox.SetText("", "What's this? " + oldName + " is changing!");
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

    void SetDefaultText()
    {
        infoBox.SetText("", "What's this?" + oldName + " is changing!");
    }

    public void Close()
    {
        gameObject.SetActive(false);
        newFarmon.DistributeEvolvePerks();
    }

    IEnumerator EvolutionPopupCoroutine()
    {
        SetDefaultText();

        //showOldFarmon = true;

        newFarmon = oldFarmon.Evolve(newFarmonName);

        yield return new WaitForSeconds(1.5f);

        //float spriteSwitchTimeCurrent = .75f;

        //spriteSwitchTimer.SetTime(spriteSwitchTimeCurrent);
        float time = 0;
        float timeScale = 1f;
        float timeScaleMax = 7.5f;
        /*while (timeScale < timeScaleMax)
        {
            if (showOldFarmon)
            {
                if (Mathf.Sin(time * timeScale) < 0) showOldFarmon = !showOldFarmon;
            }
            else
            {
                if (Mathf.Sin(time * timeScale) > 0) showOldFarmon = !showOldFarmon;
            }

            time += Time.deltaTime * timeScale;

            timeScale += Time.deltaTime * .8f;

            //if (spriteSwitchTimer.Tick(Time.deltaTime))
            //{
                
            //    spriteSwitchTimeCurrent -= .015f;
            //    spriteSwitchTimer.SetTime(spriteSwitchTimeCurrent);
            //}

            if (Input.anyKeyDown)
            {
                //spriteSwitchTimeCurrent = 0;
                timeScale = timeScaleMax;
            }

            yield return new WaitForEndOfFrame();
        }*/

        //showOldFarmon = false;
        infoBox.SetText("", "Your " + oldName + " evolved into a " + newFarmonName + "!");
        evolutionComplete = true;
    }
}