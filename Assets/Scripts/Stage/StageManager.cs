using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup stageObjectParent;
    [SerializeField] private List<StageInfo> stageInfoList;
    [SerializeField] private Stage stagePrefab;
    private List<Stage> stageList = new();

    private void Start()
    {
        //for (int i = 0; i < stageInfoList.Count; i++) 
        //{ 
        //    var stage = Instantiate(stagePrefab, stageObjectParent.transform);
        //    stage.StageInfo = stageInfoList[i];
        //    stage.textMeshPro.text = $"{i+1}";

        //    stage.button.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stage.StageInfo));
        //    stage.button.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
        //}
        LoadStage();
    }

    private void LoadStage()
    {
        // 스테이지 폴더들 로딩
        string folderName = GameManager.Instance.stageSaveFolderName;
        if (!Directory.Exists($"{Application.dataPath}/{folderName}"))
            return;

        string[] stageFolders = Directory.GetDirectories(Application.dataPath, $"{folderName}/");
        foreach (string stageFolder in stageFolders)
        {
            string[] prefabs = Directory.GetFiles(stageFolder, "*StageInfo.prefab"); // 각 폴더 내의 StageInfo 프리펩 가져오기
            foreach (string prefabPath in prefabs)
            {
                var stage = Instantiate(stagePrefab, stageObjectParent.transform);
                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefab != null && prefab.TryGetComponent(out StageInfo stageInfo))
                {
                    stage.StageInfo = stageInfo;
                }

                if (stage.StageInfo != null)
                {
                    stage.textMeshPro.text = Path.GetFileName(stageFolder);
                    stage.button.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stage.StageInfo));
                    stage.button.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
                    stageList.Add(stage);
                }
            }
        }
    }
}
