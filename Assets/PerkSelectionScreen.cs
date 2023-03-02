using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkSelectionScreen : MonoBehaviour
{
    public static PerkSelectionScreen instance;

    [SerializeField]
    TextMeshProUGUI titleText;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    InfoBox infoBox;

    Farmon targetFarmon;

    [SerializeField]
    GameObject _perkButtonPrefab;

    List<GameObject> _plates = new List<GameObject>();

    Perk[] _perks;

    [SerializeField]
    Transform PerkGrid;

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

    public void Popup( Farmon farmon, Perk[] perks )
    {
        targetFarmon = farmon;

        _perks = perks;

        gameObject.SetActive(true);

        //audioSource.clip = FarmonController.instance.LevelUpSound;
        //audioSource.volume = .7f;
        //audioSource.Play();

        SetDefaultText();

        CreatePerkButtons();
    }

    void CreatePerkButtons()
    {
        foreach (GameObject g in _plates)
        {
            Destroy(g);
        }

        _plates.Clear();

        foreach (Perk perk in _perks)
        {
            GameObject newPlate = Instantiate(_perkButtonPrefab, PerkGrid);
            _plates.Add(newPlate);
            PerkButton perkButton = newPlate.GetComponent<PerkButton>();
            RectTransform rectTransform = perkButton.transform as RectTransform;

            rectTransform.sizeDelta = new Vector2(170, 47);

            perkButton.Text.text = perk.PerkName;

            targetFarmon.perkList.TryGetValue(perk.PerkName, out int perkLevel);

            perkButton.HoverForInfo.infoBox = infoBox;
            perkButton.HoverForInfo.plateText = perk.PerkName;
            perkButton.HoverForInfo.descriptionText = perk.PerkDescription;

            if (perk.MaxLevel > 1)
            {
                perkButton.Text.text += " " + (perkLevel + 1);
                perkButton.HoverForInfo.plateText += " " + (perkLevel + 1);
            }

            perkButton.farmon = targetFarmon;
            perkButton.perk = perk;
            perkButton.button.onClick.AddListener(perkButton.GivePerk);
            perkButton.button.onClick.AddListener(Close);
        }
    }

    void SetDefaultText()
    {
        if (_perks.Length > 1)
        {
            titleText.text = "Select A Perk";

            infoBox.SetText("", targetFarmon.nickname + " has reached a crossroads in its development!");
        }
        else
        {
            titleText.text = "New Perk!";

            infoBox.SetText("", targetFarmon.nickname + " has developed a greater understanding of its power!");
        }
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
    }
}
