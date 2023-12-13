using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup stageObjectParent;
    [SerializeField] private List<StageInfo> stageInfoList;
    [SerializeField] private Stage stagePrefab;
    private List<Stage> stageList = new();

    private void Start()
    {
        for (int i = 0; i < stageInfoList.Count; i++) 
        { 
            var stage = Instantiate(stagePrefab, stageObjectParent.transform);
            stage.StageInfo = stageInfoList[i];
            stage.textMeshPro.text = $"{i+1}";

            stage.button.onClick.AddListener(() => GameManager.Instance.SetStage(stage));
            stage.button.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));

            stageList.Add(stage);
        }
    }
}
