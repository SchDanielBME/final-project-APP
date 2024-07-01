using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Office Scene");
    }

    public void TrainingGame()
    {
        SceneManager.LoadSceneAsync("Training Scene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
