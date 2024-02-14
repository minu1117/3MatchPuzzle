using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomBoardLoader : MonoBehaviour
{
    [SerializeField] private LoadingBoardUI loadingUIPrefab;
    [SerializeField] private VerticalLayoutGroup content;
    [SerializeField] private Image noneContentUI;
    private List<LoadingBoardUI> loadingUIList = new();

    public void LoadCustomBoard(BoardType boardType)
    {
        if (!MyJsonUtility.Exists(boardType))
            return;

        string folderPath = MyJsonUtility.GetSaveFolderPath(boardType);
        if (!Directory.Exists(folderPath))
        {
            Debug.Log($"{folderPath}, None");
            return;
        }

        for (int i = loadingUIList.Count - 1; i >= 0; i--)
        {
            if (loadingUIList[i] != null)
                Destroy(loadingUIList[i].gameObject);
        }
        loadingUIList.Clear();

        // 폴더 로딩, 생성 시간 순 대로 정렬
        var folders = Directory.GetDirectories(folderPath);
        Array.Sort(folders, (a, b) => Directory.GetCreationTime(a).CompareTo(Directory.GetCreationTime(b)));
        foreach (var folder in folders)
        {
            string folderName = Path.GetFileName(folder);

            var stageInfoDataLoad = MyJsonUtility.LoadJson<StageInfoData>(folderName, InfoType.Stage, boardType);
            var boardInfoDataLoad = MyJsonUtility.LoadJson<BoardInfoData>(folderName, InfoType.Board, boardType);
            if (!stageInfoDataLoad.Item2 || !boardInfoDataLoad.Item2)
                return;

            StageInfo stageInfo = new StageInfo(stageInfoDataLoad.Item1);
            BoardInfo boardInfo = new BoardInfo(boardInfoDataLoad.Item1);

            //StageInfoData stageInfoData = MyJsonUtility.LoadJson<StageInfoData>(folderName, InfoType.Stage, boardType);
            //BoardInfoData boardInfoData = MyJsonUtility.LoadJson<BoardInfoData>(folderName, InfoType.Board, boardType);
            //StageInfo stageInfo = new StageInfo(stageInfoData);
            //BoardInfo boardInfo = new BoardInfo(boardInfoData);

            if (stageInfo != null && boardInfo != null)
            {
                var loadingUI = Instantiate(loadingUIPrefab, content.transform);
                loadingUI.Init(stageInfo, boardInfo);
                loadingUI.AddOnClickRemoveFolder(boardType);
                loadingUI.AddOnClickRemoveButton(() => loadingUIList.Remove(loadingUI));
                loadingUI.AddOnClickRemoveButton(() => OnContentUI());
                loadingUI.SetStageName(folderName);
                loadingUIList.Add(loadingUI);
            }
        }

        OnContentUI();
    }

    private void OnContentUI()
    {
        loadingUIList.RemoveAll(ui => ui == null);

        if (loadingUIList.Count > 0)
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
            loadingUI.AddOnClickStartButton(action);
        }
    }

    public void LoadInGenerator(BoardGenerator generator)
    {
        foreach (var loadingUI in loadingUIList)
        {
            var stageInfo = loadingUI.GetStageInfo();
            var boardInfo = loadingUI.GetBoardInfo();
            loadingUI.AddOnClickStartButton(() => generator.Load(stageInfo, boardInfo));
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

    public void LoadAllBoardData(BoardType type)
    {
        foreach (var loadingUI in loadingUIList)
        {
            string name = loadingUI.GetStageInfo().data.stageName;
            loadingUI.GetBoardInfo().LoadData(name, type);
        }
    }

    public void ConnectAllCreateGrid()
    {
        foreach (var loadingUI in loadingUIList)
        {
            var layoutGroup = loadingUI.GetGridLayoutGroup();
            loadingUI.CreateGrid(layoutGroup, GameManager.Instance.blockedGrid, GameManager.Instance.unblockedGrid);
        }
    }

    public List<LoadingBoardUI> GetLoadingUIList()
    {
        return loadingUIList;
    }
}
