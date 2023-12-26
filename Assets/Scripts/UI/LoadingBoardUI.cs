using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingBoardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private GridLayoutGroup boardPreviewGrid;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI modeText;

    [SerializeField] private Button startButton;

    private StageInfo stageInfo;

    public void Init(StageInfo info)
    {
        SetStageInfo(info);

        int width = stageInfo.boardInfo.width;
        int height = stageInfo.boardInfo.height;

        sizeText.text = $"{width} X {height}";
        modeText.text = stageInfo.infinityMode ? "무한 모드" : "점수 모드";
    }

    public void ConnectChangeSceneStartButtonOnClick()
    {
        startButton.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stageInfo));
        startButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
    }

    public void ConnectCreateGrid(GridLayoutGroup gridLayoutGroup, GameObject unBlockedPuzzle)
    {
        startButton.onClick.AddListener(() => CreateGrid(gridLayoutGroup, unBlockedPuzzle));
    }

    public void ConnectStartButtonAction(UnityAction action)
    {
        startButton.onClick.AddListener(action);
    }

    public GridLayoutGroup GetGridLayoutGroup()
    {
        return boardPreviewGrid;
    }

    public void CreateGrid(GridLayoutGroup gridLayoutGroup, GameObject unBlockedPuzzle)
    {
        if (gridLayoutGroup.transform.childCount > 0)
            return;

        Debug.Log("2");

        int width = stageInfo.boardInfo.width;
        int height = stageInfo.boardInfo.height;
        StartCoroutine(CoCreateGrid(gridLayoutGroup, unBlockedPuzzle, GameManager.Instance.blockedPuzzle, width, height));
    }

    private IEnumerator CoCreateGrid(GridLayoutGroup gridLayoutGroup, GameObject gridPrefab, GameObject blockGridPrefab, int width, int height)
    {
        stageInfo.boardInfo.LoadGridsBlockData();

        List<GameObject> createdGrids = new();
        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                GameObject obj;
                if (stageInfo.boardInfo.GetBlockedGrid(x, y))
                    obj = Instantiate(blockGridPrefab, gridLayoutGroup.transform);
                else
                    obj = Instantiate(gridPrefab, gridLayoutGroup.transform);

                obj.gameObject.SetActive(false);
                createdGrids.Add(obj);
            }
        }

        yield return null;

        GridLayoutGroup.Constraint constraintType = new();
        if (width >= height) constraintType = GridLayoutGroup.Constraint.FixedColumnCount;
        if (width < height) constraintType = GridLayoutGroup.Constraint.FixedRowCount;

        gridLayoutGroup.constraint = constraintType;
        gridLayoutGroup.constraintCount = stageInfo.boardInfo.GetConstaintCount();
        UIManager.Instance.FitToCell(gridLayoutGroup, width, height);

        for (int i = 0; i < createdGrids.Count; i++)
        {
            createdGrids[i].gameObject.SetActive(true);
        }
    }

    public void SetStageName(string name)
    {
        nameText.text = name;
    }

    public void SetStageInfo(StageInfo info)
    {
        stageInfo = info;
    }
}
