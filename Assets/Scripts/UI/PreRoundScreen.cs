using Assets.Scripts.Timer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PreRoundScreen : MonoBehaviour
{
    public static PreRoundScreen instance;

    public GameObject pickUpFarmonDefault;

    [SerializeField]
    List<DropPointPreRoundScreen> playerDropPointsFront;

    [SerializeField]
    List<DropPointPreRoundScreen> playerDropPointsRear;

    [SerializeField]
    List<Image> playerConnectors;

    [SerializeField]
    List<Image> playerConnectorsInner;

    [SerializeField]
    List<DropPointPreRoundScreen> enemyDropPointsFront;

    [SerializeField]
    List<DropPointPreRoundScreen> enemyDropPointsRear;

    [SerializeField]
    List<Image> enemyConnectors;

    [SerializeField]
    List<Image> enemyConnectorsInner;

    [SerializeField]
    Button ContinueButton;

    [SerializeField]
    AudioClip xpSound;
    [SerializeField]
    AudioClip xpCompleteSound;

    [SerializeField]
    AudioSource audioSource;

    public void Awake()
    {
        Assert.IsNull(instance, "There should only be one instance of this object.");
        instance = this;

        gameObject.SetActive(false);
    }
    

    private void OnDestroy()
    {
        instance = null;
    }

    public void Popup(RoundController caller)
    {
        gameObject.SetActive(true);

        List<Farmon> playerTeam = Player.instance.GetFarmon();

        int index = 0;

        foreach(DropPoint dropPoint in playerDropPointsFront)
        {
            dropPoint.PickUpChangedEvent.AddListener(OnDropPointsChange);
        }

        foreach (DropPoint dropPoint in playerDropPointsRear)
        {
            dropPoint.PickUpChangedEvent.AddListener(OnDropPointsChange);
        }

        foreach (Farmon playerFarmon in playerTeam)
        {
            GameObject go = Instantiate(pickUpFarmonDefault);
            PickUpFarmon pickup = go.GetComponent<PickUpFarmon>();
            Image image = go.GetComponent<Image>();

            pickup.Initialize(playerFarmon);

            pickup.Start();

            pickup.SetDropPoint(playerDropPointsFront[index]);

            index ++;
        }

        List<Farmon> enemyTeam = RoundController.Instance.GetEnemyTeam();

        index = 0;

        foreach (Farmon enemyFarmon in enemyTeam)
        {
            GameObject go = Instantiate(pickUpFarmonDefault);
            PickUpFarmon pickup = go.GetComponent<PickUpFarmon>();
            Image image = go.GetComponent<Image>();

            pickup.Initialize(enemyFarmon);

            pickup.Start();

            pickup.SetDropPoint(enemyDropPointsFront[index]);
            index++;
        }

        OnDropPointsChange();
    }

    private void OnDropPointsChange()
    {
        Debug.Log("change");
        for(int i = 0; i < 5; i++)
        {
            if (playerDropPointsFront[i].currentPickUp && !playerDropPointsFront[i].CurrentPickUpGrabbed)
            {
                playerDropPointsRear[i].Enable();
                playerConnectors[i].enabled = true;
            }
            else
            {
                playerDropPointsRear[i].Disable();
                playerConnectors[i].enabled = false;
            }

            if (enemyDropPointsFront[i].currentPickUp && !enemyDropPointsFront[i].CurrentPickUpGrabbed)
            {
                enemyDropPointsRear[i].Enable();
                enemyConnectors[i].enabled = true;
            }
            else
            {
                enemyDropPointsRear[i].Disable();
                enemyConnectors[i].enabled = false;
            }

            if (playerDropPointsFront[i].currentPickUp && !playerDropPointsFront[i].CurrentPickUpGrabbed &&
                playerDropPointsRear[i].currentPickUp && !playerDropPointsRear[i].CurrentPickUpGrabbed)
            {
                playerConnectorsInner[i].enabled = true;
                playerDropPointsFront[i].LockIn();
            }
            else
            {
                playerConnectorsInner[i].enabled = false;
                playerDropPointsFront[i].Enable();
            }

            if (enemyDropPointsFront[i].currentPickUp && !enemyDropPointsFront[i].CurrentPickUpGrabbed &&
                enemyDropPointsRear[i].currentPickUp && !enemyDropPointsRear[i].CurrentPickUpGrabbed)
            {
                enemyConnectorsInner[i].enabled = true;
                enemyDropPointsFront[i].LockIn();
            }
            else
            {
                enemyConnectorsInner[i].enabled = false;
                enemyDropPointsFront[i].Enable();
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {

    }

    public void ContinueButtonClick()
    {
        Close();

        for(int i = 0 ; i < playerDropPointsFront.Count; i++)
        {
            PickUpFarmon pickUp = (PickUpFarmon)playerDropPointsFront[i].currentPickUp;
            if(pickUp) pickUp.farmonInstance.SetFormation(i, Farmon.FormationTypeEnum.frontline);
        }

        for (int i = 0; i < playerDropPointsRear.Count; i++)
        {
            PickUpFarmon pickUp = (PickUpFarmon)playerDropPointsRear[i].currentPickUp;
            if (pickUp) pickUp.farmonInstance.SetFormation(i, Farmon.FormationTypeEnum.backline);
        }

        for (int i = 0; i < enemyDropPointsFront.Count; i++)
        {
            PickUpFarmon pickUp = (PickUpFarmon)enemyDropPointsFront[i].currentPickUp;
            if (pickUp) pickUp.farmonInstance.SetFormation(i, Farmon.FormationTypeEnum.frontline);
        }

        for (int i = 0; i < enemyDropPointsRear.Count; i++)
        {
            PickUpFarmon pickUp = (PickUpFarmon)enemyDropPointsRear[i].currentPickUp;
            if (pickUp) pickUp.farmonInstance.SetFormation(i, Farmon.FormationTypeEnum.backline);
        }

        RoundController.Instance.StartMatch();
    }
}
