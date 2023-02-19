using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Player.instance.gameObject.SetActive(false);
    }

    public void NewGame()
    {
        Player.instance.StoryProgress = 0;
        Player.instance.FarmonSquadIds = new uint[Player.farmonPerTeam];
        Player.instance.FarmonSquadIds[0] = 1;
        Player.instance.SaveName = "New Player";
        Player.instance.gameObject.SetActive(true);
        SceneManager.LoadScene("Hill");
    }

    public void LoadGame()
    {
        SaveController.LoadPlayer();
        Player.instance.gameObject.SetActive(true);
        SceneManager.LoadScene("Hill");
    }
}
