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

                infoBox.plateText.text = targetUnit.farmonName;
                infoBox.SetText(targetUnit.farmonName, targetUnit.Description);

                portraitHoverForInfo.plateText = targetUnit.farmonName;
                portraitHoverForInfo.descriptionText = targetUnit.Description;
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
    public TextMeshProUGUI agilityText;
    public Button agilityUpgradeButton;
    public TextMeshProUGUI focusText;
    public Button focusUpgradeButton;
    public TextMeshProUGUI luckText;
    public Button luckUpgradeButton;

    public TextMeshProUGUI attributePointsText;

    //Perks
    [Header("Perks")]
    public PerkList perkList;

    //InfoBox
    [Header("InfoBox")]
    [SerializeField]
    InfoBox infoBox;

    [SerializeField]
    HoverForInfo portraitHoverForInfo;

    public void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;

        gritUpgradeButton.onClick.AddListener(UpgradeGrit);
        powerUpgradeButton.onClick.AddListener(UpgradePower);
        agilityUpgradeButton.onClick.AddListener(UpgradeAgility);
        focusUpgradeButton.onClick.AddListener(UpgradeFocus);
        luckUpgradeButton.onClick.AddListener(UpgradeLuck);
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void UpgradeGrit()
    {
        if(targetUnit.attributePoints > 0)
        {
            targetUnit.attributePoints--;

            targetUnit.GritBonus++;
        }
    }

    private void UpgradePower()
    {
        if (targetUnit.attributePoints > 0)
        {
            targetUnit.attributePoints--;
            targetUnit.PowerBonus++;
        }
    }

    private void UpgradeAgility()
    {
        if (targetUnit.attributePoints > 0)
        {
            targetUnit.attributePoints--;
            targetUnit.AgilityBonus++;
        }
    }

    private void UpgradeFocus()
    {
        if (targetUnit.attributePoints > 0)
        {
            targetUnit.attributePoints--;
            targetUnit.FocusBonus++;
        }
    }

    private void UpgradeLuck()
    {
        if (targetUnit.attributePoints > 0)
        {
            targetUnit.attributePoints--;
            targetUnit.LuckBonus++;
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
        if(targetUnit.nickname == "")
        {
            nameText.text = targetUnit.farmonName;
        }
        else
        {
            nameText.text = targetUnit.nickname;
        }

        levelText.text = "Level " + targetUnit.level;

        characterImage.sprite = targetUnit.mySpriteRenderer.sprite;

        gritText.text = targetUnit.GetModifiedGrit().ToString();
        if(targetUnit.GetModifiedGrit() > targetUnit.Grit)
        {
            gritText.color = Color.green;
        }
        else if(targetUnit.GetModifiedGrit() < targetUnit.Grit)
        {
            gritText.color = Color.red;
        }
        else
        {
            gritText.color = Color.white;
        }

        powerText.text = targetUnit.GetModifiedPower().ToString();
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

        agilityText.text = targetUnit.GetModifiedAgility().ToString();
        if (targetUnit.GetModifiedAgility() > targetUnit.Agility)
        {
            agilityText.color = Color.green;
        }
        else if (targetUnit.GetModifiedAgility() < targetUnit.Agility)
        {
            agilityText.color = Color.red;
        }
        else
        {
            agilityText.color = Color.white;
        }

        focusText.text = targetUnit.GetModifiedFocus().ToString();
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

        luckText.text = targetUnit.GetModifiedLuck().ToString();
        if (targetUnit.GetModifiedLuck() > targetUnit.Luck)
        {
            luckText.color = Color.green;
        }
        else if (targetUnit.GetModifiedLuck() < targetUnit.Luck)
        {
            luckText.color = Color.red;
        }
        else
        {
            luckText.color = Color.white;
        }

        attributePointsText.text = targetUnit.attributePoints.ToString();

        bool enableAttributeButtons = targetUnit.attributePoints > 0;
        gritUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        powerUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        agilityUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        focusUpgradeButton.gameObject.SetActive(enableAttributeButtons);
        luckUpgradeButton.gameObject.SetActive(enableAttributeButtons);


        perkList.GeneratePerkList(targetUnit, infoBox);
    }
}
