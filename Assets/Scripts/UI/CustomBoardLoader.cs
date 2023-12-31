using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomBoardLoader : MonoBehaviour
{
    [SerializeField] private LoadingBoardUI loadingUIPrefab;
    [SerializeField] private VerticalLayoutGroup content;
    [SerializeField] private GameObject gridPrefab;
    [SerializeField] private GameObject blockGridPrefab;
    private List<LoadingBoardUI> loadingUIList = new();

    public void LoadCustomBoard()
    {
        // 스테이지 폴더들 로딩
        string folderName = GameManager.Instance.customBoardSaveFolderName;
        if (!Directory.Exists($"{Application.dataPath}/{folderName}"))
            return;

        for (int i = loadingUIList.Count-1; i >= 0; i--)
        {
            Destroy(loadingUIList[i].gameObject);
        }
        loadingUIList.Clear();

        string[] stageFolders = Directory.GetDirectories(Application.dataPath, $"{folderName}/");
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

    public void ConnectStartButtonsAction(UnityAction action)
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.ConnectStartButtonAction(action);
        }
    }

    public void LoadInGenerator(BoardGenerator generator)
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.ConnectStartButtonAction(() => generator.Load(loadingUI.GetStageInfo()));
        }
    }

    public void ConnectUIStartButtonOnClick()
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.ConnectChangeSceneStartButtonOnClick();
        }
    }

    public void ConnectAllCreateGrid()
    {
        foreach (var ui in loadingUIList)
        {
            var layoutGroup = ui.GetGridLayoutGroup();
            ui.CreateGrid(layoutGroup, GameManager.Instance.blockedPuzzle, gridPrefab);
        }
    }

    public List<LoadingBoardUI> GetLoadingUIList()
    {
        return loadingUIList;
    }
}
