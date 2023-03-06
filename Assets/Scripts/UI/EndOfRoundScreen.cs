using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EndOfRoundScreen : MonoBehaviour
{
    public static EndOfRoundScreen instance;

    List<uint> farmonList = new List<uint>();
    List<Image> farmonImageList = new List<Image>();
    List<Bar> farmonBarList = new List<Bar>();

    [SerializeField]
    Image CharacterImage1;
    [SerializeField]
    Image CharacterImage2;
    [SerializeField]
    Image CharacterImage3;
    [SerializeField]
    Image CharacterImage4;
    [SerializeField]
    Image CharacterImage5;

    [SerializeField]
    Bar XpBar1;
    [SerializeField]
    Bar XpBar2;
    [SerializeField]
    Bar XpBar3;
    [SerializeField]
    Bar XpBar4;
    [SerializeField]
    Bar XpBar5;

    [SerializeField]
    Button ContinueButton;

    [SerializeField]
    AudioClip xpSound;
    [SerializeField]
    AudioClip xpCompleteSound;

    [SerializeField]
    AudioSource audioSource;

    private Timer xpTimer = new Timer();
    private Timer waitForNextFarmonTimer = new Timer();

    bool doneGivingXp = false;

    public void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;
    }
    

    private void OnDestroy()
    {
        instance = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        farmonImageList.Add(CharacterImage1);
        farmonImageList.Add(CharacterImage2);
        farmonImageList.Add(CharacterImage3);
        farmonImageList.Add(CharacterImage4);
        farmonImageList.Add(CharacterImage5);

        farmonBarList.Add(XpBar1);
        farmonBarList.Add(XpBar2);
        farmonBarList.Add(XpBar3);
        farmonBarList.Add(XpBar4);
        farmonBarList.Add(XpBar5);

        gameObject.SetActive(false);

        xpTimer.SetTime(0.06f);
        xpTimer.autoReset = true;

        waitForNextFarmonTimer.SetTime(1f);
        waitForNextFarmonTimer.autoReset = true;
    }

    public void Popup(RoundController caller)
    {
        List<Farmon> playerFarmon = Player.instance.GetFarmon();

        for (int i = 0; i < 5; i++)
        {
            if(i < playerFarmon.Count)
            {
                farmonImageList[i].gameObject.SetActive(true);
                farmonBarList[i].gameObject.SetActive(true);
            }
            else
            {
                farmonImageList[i].gameObject.SetActive(false);
                farmonBarList[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);

        ContinueButton.interactable = false;

        currentXPIndex = 0;
        totalXP = GetTotalXP(caller);
        currentXP = totalXP;

        doneGivingXp = false;
    }

    int currentXPIndex = 0;
    int totalXP = 0;
    int currentXP = 0;

    private void Update()
    {
        if (doneGivingXp)
        {
            return;
        }

        if (LevelUpScreen.instance.gameObject.activeSelf || PerkSelectionScreen.instance.gameObject.activeSelf)
        {
            return;
        }

        List<Farmon> playerFarmon = Player.instance.GetFarmon();

        if (currentXPIndex >= playerFarmon.Count)
        {
            doneGivingXp = true;
            ContinueButton.interactable = true;

            foreach (Farmon f in playerFarmon)
            {
                SaveController.SaveFarmonPlayer(f);
                SaveController.SavePlayer();
            }
            return;
        }

        if(currentXP <= 0)
        {
            if (waitForNextFarmonTimer.Tick(Time.deltaTime))
            {
                currentXP = totalXP;
                currentXPIndex++;
            }

            return;
        }

        if (xpTimer.Tick(Time.deltaTime))
        {
            int xpToGive = Mathf.Max(1, Mathf.Min(currentXP, totalXP/40));

            playerFarmon[currentXPIndex].GiveXp(xpToGive);
            currentXP -= xpToGive;

            if (currentXP <= 0)
            {
                audioSource.clip = xpCompleteSound;
            }
            else
            {
                audioSource.clip = xpSound;
                audioSource.pitch = 1f + 0.8f * (1f - (float)currentXP / (float)totalXP);
            }

            audioSource.volume = 0.2f;
            audioSource.Play();
        }
    }

    private int GetTotalXP(RoundController caller)
    {
        List<Farmon> enemyTeam = caller.GetEnemyTeam();
        int totalXP = 0;
        for (int i = 0; i < enemyTeam.Count; i++)
        {
            totalXP += enemyTeam[i].level * 40;
        }

        return totalXP;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        List<Farmon> playerFarmon = Player.instance.GetFarmon();

        for (int i = 0; i < playerFarmon.Count; i++)
        {
            Farmon farmon = playerFarmon[i];
            farmonImageList[i].sprite = farmon.mySpriteRenderer.sprite;

            farmonBarList[i].SetPercent((float) farmon.experience / (float) farmon.GetXpRequiredForNextLevel());
        }
    }
}
