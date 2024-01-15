using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int scoreNum;
    private Animator scoreAnim;

    private void Start()
    {
        scoreAnim = scoreText.GetComponent<Animator>();
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
        scoreAnim.SetTrigger("Start");

        scoreNum = score;
        scoreText.text = scoreNum.ToString();
    }

    public int GetScore()
    {
        return scoreNum;
    }
}
