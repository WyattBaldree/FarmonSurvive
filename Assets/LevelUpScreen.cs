using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpScreen : MonoBehaviour, IPointerDownHandler
{
    public static LevelUpScreen instance;

    [SerializeField]
    TextMeshProUGUI gritText, powerText, agilityText, focusText, luckText, pointsText, levelText;

    [SerializeField]
    Image farmonPortrait;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    DynamicText textBox;

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

    public void Popup(Farmon farmon, int gritPrev, int powerPrev, int agilityPrev, int focusPrev, int luckPrev, int pointsPrev)
    {
        targetFarmon = farmon;

        gameObject.SetActive(true);

        audioSource.clip = FarmonController.instance.LevelUpSound;
        audioSource.volume = .7f;
        audioSource.Play();

        gritText.text = StatChangeString(gritPrev, farmon.Grit);
        powerText.text = StatChangeString(powerPrev, farmon.Power);
        agilityText.text = StatChangeString(agilityPrev, farmon.Agility);
        focusText.text = StatChangeString(focusPrev, farmon.Focus);
        luckText.text = StatChangeString(luckPrev, farmon.Luck);
        pointsText.text = StatChangeString(pointsPrev, farmon.attributePoints);

        levelText.text = "Level " + targetFarmon.level;

        textBox.SetText(farmon.nickname + " has reached level <color.yellow>" + farmon.level + "<default>!");
    }

    private string StatChangeString(int statOld, int statNew)
    {
        if (statOld != statNew)
        {
            return statOld + ">" + statNew;
        }
        else
        {
            return "" + statNew;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        targetFarmon.DistributeLevelUpPerks();
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (textBox.reading)
        {
            textBox.SkipReading();
        }
        else
        {
            Close();
        }
    }
}
