using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomBoardLoader : MonoBehaviour
{
    [SerializeField] private LoadingBoardUI loadingUIPrefab;
    [SerializeField] private VerticalLayoutGroup content;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject blockGridPrefab;
    private List<LoadingBoardUI> loadingUIList = new();

    public void Init()
    {
        LoadCustomBoard();
    }

    private void LoadCustomBoard()
    {
        // 스테이지 폴더들 로딩
        //string[] stageFolders = Directory.GetDirectories(Application.dataPath, $"{GameManager.Instance.prefabSaveFolderName}");
        if (!Directory.Exists($"{Application.dataPath}/Prefabs/Stage"))
            return;

        string[] stageFolders = Directory.GetDirectories(Application.dataPath, $"Prefabs/Stage/");
        foreach (string stageFolder in stageFolders)
        {
            string[] prefabs = Directory.GetFiles(stageFolder, "*StageInfo.prefab"); // 각 폴더 내의 StageInfo 프리펩 가져오기
            foreach (string prefabPath in prefabs)
            {
                GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefab != null && prefab.TryGetComponent(out StageInfo info))
                {
                    var loadingUI = Instantiate(loadingUIPrefab, content.transform);
                    loadingUI.Init(info);

                    string name = Path.GetFileName(stageFolder);
                    loadingUI.SetStageName(name);
                    loadingUIList.Add(loadingUI);
                }
            }
        }
    }

    public void ConnectAllCreateGrid()
    {
        foreach (var ui in loadingUIList)
        {
            ui.CreateGrid(gridPrefab, blockGridPrefab);
        }
    }
}
