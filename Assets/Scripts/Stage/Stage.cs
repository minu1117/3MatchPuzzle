using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    public StageInfo stageInfo;
    public TextMeshProUGUI stageName;
    public Button button;

    [SerializeField] private List<Image> stars;

    public void OnStars()
    {
        if (stageInfo == null)
            return;

        for (int i = 0; i < stageInfo.data.clearScore; i++)
        {
            if (stars.Count >= i)
                stars[i].gameObject.SetActive(true);
        }
    }
}
