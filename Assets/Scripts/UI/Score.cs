using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int scoreNum;

    private void Start()
    {
        scoreNum = 0;
        SetScore(scoreNum);
    }

    public void SubScore(int score)
    {
        if (scoreNum - score <= 0) 
        {
            scoreNum = 0;
        }
        else
        {
            scoreNum -= score;
        }
        
        SetScore(scoreNum);
    }

    public void AddScore(int score)
    {
        scoreNum += score;
        SetScore(scoreNum);
    }

    public void SetScore(int score)
    {
        scoreNum = score;
        scoreText.text = scoreNum.ToString();
    }
}
