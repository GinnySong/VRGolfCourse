using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int score;
    public GameObject winScreen;
    public TextMeshPro scoreText;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
    }

    public void incrementScore() {
        score += 1;
    }

    public void displayWinScreen() {
        scoreText.text = "Score: " + score;
        winScreen.SetActive(true);
    }
}
