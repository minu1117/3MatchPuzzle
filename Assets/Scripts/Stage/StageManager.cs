using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

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
        BoardType boardType = BoardType.Stage;
        string folderPath = MyJsonUtility.GetSaveFolderPath(boardType);
        if (!Directory.Exists(folderPath))
        {
            Debug.Log($"{folderPath}, None");
            return;
        }

        var folders = Directory.GetDirectories(folderPath);
        Array.Sort(folders, (a, b) => Directory.GetCreationTime(a).CompareTo(Directory.GetCreationTime(b)));

        int nameOrder = 1;
        foreach (var folder in folders)
        {
            var boardFolder = Directory.GetFiles(folder);
            string folderName = Path.GetFileName(folder);

            StageInfoData stageInfoData = MyJsonUtility.LoadJson<StageInfoData>(folderName, InfoType.Stage, boardType);
            BoardInfoData boardInfoData = MyJsonUtility.LoadJson<BoardInfoData>(folderName, InfoType.Board, boardType);
            StageInfo stageInfo = new StageInfo(stageInfoData);
            BoardInfo boardInfo = new BoardInfo(boardInfoData);

            if (stageInfo != null && boardInfo != null)
            {
                var stage = Instantiate(stagePrefab, stageObjectParent.transform);
                stage.stageInfo = stageInfo;
                stage.boardInfo = boardInfo;
                stage.OnStars();

                stage.stageName.text = nameOrder.ToString();
                stage.button.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
                stage.button.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stage.stageInfo));
                stage.button.onClick.AddListener(() => GameManager.Instance.SetBoardInfo(stage.boardInfo));
                stage.button.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
                stageList.Add(stage);

                nameOrder++;
            }
        }
    }
}
