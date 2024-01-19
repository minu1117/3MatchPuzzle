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
    [SerializeField] private Image noneContentUI;
    private List<LoadingBoardUI> loadingUIList = new();

    public void LoadCustomBoard(string folderName)
    {
        // 스테이지 폴더들 로딩
        if (!Directory.Exists($"{Application.dataPath}/{folderName}"))
            return;

        for (int i = loadingUIList.Count-1; i >= 0; i--)
        {
            if (loadingUIList[i] != null)
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
                    loadingUI.ConnectRemoveFolder(folderName);

                    string name = Path.GetFileName(stageFolder);
                    loadingUI.SetStageName(name);
                    loadingUIList.Add(loadingUI);
                }

                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        OnContentUI(loadingUIList.Count > 0);
    }

    private void OnContentUI(bool set)
    {
        if (set)
        {
            content.gameObject.SetActive(true);
            noneContentUI.gameObject.SetActive(false);
        }
        else
        {
            content.gameObject.SetActive(false);
            noneContentUI.gameObject.SetActive(true);
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
            var info = loadingUI.GetStageInfo();
            loadingUI.ConnectStartButtonAction(() => generator.Load(info));
        }
    }

    public void RemoveAllButtonsAction()
    {
        foreach (var loadingUI in loadingUIList)
        {
            loadingUI.RemoveButtonAction();
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
            ui.CreateGrid(layoutGroup, GameManager.Instance.blockedGrid, GameManager.Instance.unblockedGrid);
        }
    }

    public List<LoadingBoardUI> GetLoadingUIList()
    {
        return loadingUIList;
    }
}
