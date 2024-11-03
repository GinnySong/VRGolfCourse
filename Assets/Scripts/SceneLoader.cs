using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour
{
    public void loadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void NextScene() {
        int index = SceneManager.GetActiveScene().buildIndex;
        index++;
        if (index < SceneManager.sceneCountInBuildSettings) {
            SceneManager.LoadScene(index);
        }
    }

    public void PrevScene() {
        int index = SceneManager.GetActiveScene().buildIndex;
        index--;
        if (index >= 0) {
            SceneManager.LoadScene(index);
        }
    }

    public void RestartScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame() {
        Application.Quit();
    }
}
