using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("PersistentScene", LoadSceneMode.Single);
        SceneManager.LoadScene("Hill", LoadSceneMode.Single);
    }
}
