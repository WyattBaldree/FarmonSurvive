using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class StatsScreen : MonoBehaviour
{
    public static StatsScreen instance;

    private Farmon targetUnit = null;
    public Farmon TargetUnit
    {
        get => targetUnit;
        set
        {
            if (targetUnit) targetUnit.statsChangedEvent -= UpdateStatsScreen;
            targetUnit = value;
            if (targetUnit)
            {
                targetUnit.statsChangedEvent += UpdateStatsScreen;
                gameObject.SetActive(true);

                UpdateStatsScreen(targetUnit, EventArgs.Empty);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    public Image characterImage;

    //Attributes
    [Header("Attributes")]
    public TextMeshProUGUI gritText;
    public Button gritUpgradeButton;
    public TextMeshProUGUI powerText;
    public Button powerUpgradeButton;
    public TextMeshProUGUI reflexText;
    public Button reflexUpgradeButton;
    public TextMeshProUGUI focusText;
    public Button focusUpgradeButton;
    public TextMeshProUGUI speedText;
    public Button speedUpgradeButton;

    public TextMeshProUGUI attributePointsText;

    //Perks
    [Header("Perks")]
    public TextMeshProUGUI perkPointsText;



    public void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;

        gritUpgradeButton.onClick.AddListener(UpgradeGrit);
        powerUpgradeButton.onClick.AddListener(UpgradePower);
        reflexUpgradeButton.onClick.AddListener(UpgradeReflex);
        focusUpgradeButton.onClick.AddListener(UpgradeFocus);
        speedUpgradeButton.onClick.AddListener(UpgradeSpeed);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void UpgradeGrit()
    {
        if(targetUnit.attributePoints > 0)
        {
            if (targetUnit.GritBase + targetUnit.GritBonus < 40)
            {
                targetUnit.attributePoints--;
                targetUnit.GritBonus++;
            }
        }
    }

    private void UpgradePower()
    {
        if (targetUnit.attributePoints > 0)
        {
            if (targetUnit.PowerBase + targetUnit.PowerBonus < 40)
            {
                targetUnit.attributePoints--;
                targetUnit.PowerBonus++;
            }
        }
    }

    private void UpgradeReflex()
    {
        if (targetUnit.attributePoints > 0)
        {
            if (targetUnit.ReflexBase + targetUnit.ReflexBonus < 40)
            {
                targetUnit.attributePoints--;
                targetUnit.ReflexBonus++;
            }
        }
    }

    private void UpgradeFocus()
    {
        if (targetUnit.attributePoints > 0)
        {
            if (targetUnit.FocusBase + targetUnit.FocusBonus < 40)
            {
                targetUnit.attributePoints--;
                targetUnit.FocusBonus++;
            }
        }
    }

    private void UpgradeSpeed()
    {
        if (targetUnit.attributePoints > 0)
        {
            if (targetUnit.SpeedBase + targetUnit.SpeedBonus < 40)
            {
                targetUnit.attributePoints--;
                targetUnit.SpeedBonus++;
            }
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            TargetUnit = null;
        }

        if (TargetUnit)
        {
            characterImage.sprite = targetUnit.mySpriteRenderer.sprite;
        }
    }

    private void UpdateStatsScreen(object sender, EventArgs e)
    {
        nameText.text = targetUnit.farmonName;

        levelText.text = "Level " + targetUnit.level;

        characterImage.sprite = targetUnit.mySpriteRenderer.sprite;

        gritText.text = targetUnit.GetModifiedGrit().ToString();
        if(targetUnit.GetModifiedGrit() > targetUnit.GritBonus)
        {
            gritText.color = Color.green;
        }
        else if(targetUnit.GetModifiedGrit() < targetUnit.GritBonus)
        {
            gritText.color = Color.red;
        }
        else
        {
            gritText.color = Color.white;
        }

        powerText.text = targetUnit.Power.ToString();
        if (targetUnit.GetModifiedPower() > targetUnit.Power)
        {
            powerText.color = Color.green;
        }
        else if (targetUnit.GetModifiedPower() < targetUnit.Power)
        {
            powerText.color = Color.red;
        }
        else
        {
            powerText.color = Color.white;
        }

        reflexText.text = targetUnit.Reflex.ToString();
        if (targetUnit.GetModifiedReflex() > targetUnit.Reflex)
        {
            reflexText.color = Color.green;
        }
        else if (targetUnit.GetModifiedReflex() < targetUnit.Reflex)
        {
            reflexText.color = Color.red;
        }
        else
        {
            reflexText.color = Color.white;
        }

        focusText.text = targetUnit.Focus.ToString();
        if (targetUnit.GetModifiedFocus() > targetUnit.Focus)
        {
            focusText.color = Color.green;
        }
        else if (targetUnit.GetModifiedFocus() < targetUnit.Focus)
        {
            focusText.color = Color.red;
        }
        else
        {
            focusText.color = Color.white;
        }

        speedText.text = targetUnit.Speed.ToString();
        if (targetUnit.GetModifiedSpeed() > targetUnit.Speed)
        {
            speedText.color = Color.green;
        }
        else if (targetUnit.GetModifiedSpeed() < targetUnit.Speed)
        {
            speedText.color = Color.red;
        }
        else
        {
            speedText.color = Color.white;
        }

        attributePointsText.text = targetUnit.attributePoints.ToString();

        bool enableAttributeButtons = targetUnit.attributePoints > 0;
        gritUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        powerUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        reflexUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        focusUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        speedUpgradeButton.gameObject.SetActive(enableAttributeButtons);


        perkPointsText.text = targetUnit.perkPoints.ToString();
    }
}
