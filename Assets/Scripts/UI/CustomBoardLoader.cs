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

    public void Init()
    {
        LoadCustomBoard();
    }

    private void LoadCustomBoard()
    {
        // 스테이지 폴더들 로딩
        string folderName = GameManager.Instance.customBoardSaveFolderName;
        if (!Directory.Exists($"{Application.dataPath}/{folderName}"))
            return;

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

    // 보드 생성기에 로딩해서 옮겨주기 (이름 못 정함)
    public void ConnectUIStartButtonOnClick(GridLayoutGroup gridLayoutGroup, GameObject unBlockedPuzzle)
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.ConnectCreateGrid(gridLayoutGroup, unBlockedPuzzle);
        }
    }

    public void ConnectStartButtonsAction(UnityAction action)
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.ConnectStartButtonAction(action);
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
            ui.CreateGrid(layoutGroup, gridPrefab);
        }
    }
}
