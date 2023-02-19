using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LevelUpScreen : MonoBehaviour
{
    public static LevelUpScreen instance;

    [SerializeField]
    TextMeshProUGUI gritText, powerText, reflexText, focusText, speedText, pointsText, levelText;

    [SerializeField]
    Image farmonPortrait;

    [SerializeField]
    AudioSource audioSource;

    Farmon targetFarmon;

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

    public void Popup(Farmon farmon, int gritPrev, int powerPrev, int reflexPrev, int focusPrev, int speedPrev, int pointsPrev)
    {
        targetFarmon = farmon;

        gameObject.SetActive(true);

        audioSource.clip = FarmonController.instance.LevelUpSound;
        audioSource.volume = .7f;
        audioSource.Play();

        gritText.text = StatChangeString(gritPrev, farmon.Grit);
        powerText.text = StatChangeString(powerPrev, farmon.Power);
        reflexText.text = StatChangeString(reflexPrev, farmon.Reflex);
        focusText.text = StatChangeString(focusPrev, farmon.Focus);
        speedText.text = StatChangeString(speedPrev, farmon.Speed);
        pointsText.text = StatChangeString(pointsPrev, farmon.attributePoints);

        levelText.text = "Level " + targetFarmon.level;
    }

    private string StatChangeString(int statOld, int statNew)
    {
        if (statOld != statNew)
        {
            return statOld + "->" + statNew;
        }
        else
        {
            return "" + statNew;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        farmonPortrait.sprite = targetFarmon.mySpriteRenderer.sprite;
    }

    private string IntToPluses(int num)
    {
        string s = "";

        for(int i = 0; i < num; i++)
        {
            s += "+";
        }

        return s;
    }
}
