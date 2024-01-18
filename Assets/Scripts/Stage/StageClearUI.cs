using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private List<Image> starImages;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI clickCountText;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject backgroundObject;

    private Dictionary<Image, Animator> starDict = new();

    public void Init()
    {
        for (int i = 0; i < starImages.Count; i++)
        {
            if (starImages[i].TryGetComponent(out Animator anim))
            {
                starDict.Add(starImages[i], anim);
            }
        }

        exitButton.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.stageSceneName));
        SetClearTimeText(0);
        SetClickCountText(0);
    }

    public void SetClickCountText(int count)
    {
        clickCountText.text = $"클릭 횟수 : {count}회";
    }

    public void SetClearTimeText(int time)
    {
        int saveTime = time;
        string timeStr = string.Empty;
        if (saveTime >= 3600)
        {
            int hour = saveTime / 3600;
            timeStr += $"{hour}시간";
            saveTime = saveTime - (hour*3600);
        }
        if (saveTime >= 60)
        {
            int minute = saveTime / 60;
            timeStr += $"{minute}분";
            saveTime = saveTime - (minute*60);
        }
        if (saveTime >= 0)
        {
            int minute = saveTime;
            timeStr += $"{minute}초";
        }

        clearTimeText.text = $"걸린 시간 : {timeStr}";
    }

    public void StartFillStars(int count)
    {
        StartCoroutine(FillStars(count));
    }

    private IEnumerator FillStars(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (starImages.Count > i)
            {
                starImages[i].gameObject.SetActive(true);
                starDict[starImages[i]].SetTrigger("Start");
                SoundManager.Instance.PlayStarSparkSound();
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    public void OnActive()
    {
        backgroundObject.SetActive(true);
        SoundManager.Instance.PlayStageClearSound();
    }
}
