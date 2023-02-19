using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public void OpenMainMenu()
    {
        foreach(Farmon f in Farmon.farmonList)
        {
            Destroy(f.gameObject);
        }

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
