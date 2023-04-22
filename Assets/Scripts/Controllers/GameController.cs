using Assets.Scripts.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    static Timer _slowMoTimer = new Timer();

    private void Update()
    {
        if (_slowMoTimer.Tick(Time.fixedDeltaTime)){
            Time.timeScale = 1f;
        }
    }

    public static void SlowMo(float seconds, float magnitude = .5f)
    {
        _slowMoTimer.SetTime(seconds);
        Time.timeScale = magnitude;
    }

    public void OpenMainMenu()
    {
        Farmon.UnloadFarmon();

        //mainMenu.gameObject.SetActive(true);
        SceneManager.LoadScene("MainMenu");
    }
}

[CustomEditor(typeof(GameControllerEditor), true)]
public class GameControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        GameController GameController = (GameController)target;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Load Main Menu"))
        {
            GameController.OpenMainMenu();
        }
    }
}
