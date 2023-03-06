using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
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
