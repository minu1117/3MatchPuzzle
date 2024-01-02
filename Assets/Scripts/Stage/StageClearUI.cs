using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Sprite emptyStarSprite;
    [SerializeField] private List<Image> starImages;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI clickCountText;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject backgroundObject;

    public void Init()
    {
        for (int i = 0; i < starImages.Count; i++)
        {
            starImages[i].sprite = emptyStarSprite;
        }

        exitButton.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.stageSceneName));
        SetClearTimeText(0);
        SetClickCountText(0);
    }

    public void SetClickCountText(int count)
    {
        clickCountText.text = $"Ŭ�� Ƚ�� : {count}ȸ";
    }

    public void SetClearTimeText(int time)
    {
        int saveTime = time;
        string timeStr = string.Empty;
        if (saveTime >= 3600)
        {
            int hour = saveTime / 3600;
            timeStr += $"{hour}�ð�";
            saveTime = saveTime - (hour*3600);
        }
        if (saveTime >= 60)
        {
            int minute = saveTime / 60;
            timeStr += $"{minute}��";
            saveTime = saveTime - (minute*60);
        }
        if (saveTime >= 0)
        {
            int minute = saveTime;
            timeStr += $"{minute}��";
        }

        clearTimeText.text = $"�ɸ� �ð� : {timeStr}";
    }

    public void OnActive()
    {
        backgroundObject.SetActive(true);
    }
}
