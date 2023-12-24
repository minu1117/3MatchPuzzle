using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

        startButton.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stageInfo));
        startButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
    }

    public void CreateGrid(GameObject gridPrefab, GameObject blockGridPrefab)
    {
        if (boardPreviewGrid.transform.childCount > 0)
            return;

        int width = stageInfo.boardInfo.width;
        int height = stageInfo.boardInfo.height;
        StartCoroutine(CoCreateGrid(gridPrefab, blockGridPrefab, width, height));
    }

    private IEnumerator CoCreateGrid(GameObject gridPrefab, GameObject blockGridPrefab, int width, int height)
    {
        stageInfo.boardInfo.LoadGridsBlockData();

        List<GameObject> createdGrids = new();
        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                GameObject obj;
                if (stageInfo.boardInfo.GetBlockedGrid(x, y))
                    obj = Instantiate(blockGridPrefab, boardPreviewGrid.transform);
                else
                    obj = Instantiate(gridPrefab, boardPreviewGrid.transform);

                obj.gameObject.SetActive(false);
                createdGrids.Add(obj);
            }
        }

        yield return null;

        GridLayoutGroup.Constraint constraintType = new();
        if (width >= height) constraintType = GridLayoutGroup.Constraint.FixedColumnCount;
        if (width < height) constraintType = GridLayoutGroup.Constraint.FixedRowCount;

        boardPreviewGrid.constraint = constraintType;
        boardPreviewGrid.constraintCount = stageInfo.boardInfo.GetConstaintCount();
        UIManager.Instance.FitToCell(boardPreviewGrid, width, height);

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
