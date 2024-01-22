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
        LoadStage();
    }

    private void LoadStage()
    {
        // 스테이지 폴더들 로딩
        string folderName = GameManager.Instance.stageSaveFolderName;
        if (!Directory.Exists($"{Application.dataPath}/{folderName}"))
            return;

        int nameOrder = 1;
        DirectoryInfo directoryInfo = new($"{Application.dataPath}/{folderName}");
        List<DirectoryInfo> stageFolders = new(directoryInfo.GetDirectories());
        stageFolders.Sort((a, b) => a.CreationTime.CompareTo(b.CreationTime));

        foreach (DirectoryInfo stageFolder in stageFolders)
        {
            FileInfo[] prefabs = stageFolder.GetFiles("*StageInfo.prefab"); // 각 폴더 내의 StageInfo 프리펩 가져오기
            foreach (FileInfo prefabFile in prefabs)
            {
                var stage = Instantiate(stagePrefab, stageObjectParent.transform);

                string prefabPath = prefabFile.FullName;
                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefab != null && prefab.TryGetComponent(out StageInfo stageInfo))
                {
                    stage.stageInfo = stageInfo;
                    stage.OnStars();
                }

                if (stage.stageInfo != null)
                {
                    stage.stageName.text = nameOrder.ToString();
                    stage.button.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
                    stage.button.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stage.stageInfo));
                    stage.button.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
                    stageList.Add(stage);

                    nameOrder++;
                }
            }
        }
    }
}
