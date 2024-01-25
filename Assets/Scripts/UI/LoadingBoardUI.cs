using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class LoadingBoardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private GridLayoutGroup boardPreviewGrid;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI modeText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private TextMeshProUGUI startButtonText;
    private StageInfo stageInfo;
    private BoardInfo boardInfo;

    public BoardInfo GetBoardInfo()
    {
        return boardInfo;
    }

    public void Init(StageInfo stageInfo, BoardInfo boardInfo)
    {
        SetStageInfo(stageInfo);
        this.boardInfo = boardInfo;

        int width = boardInfo.data.width;
        int height = boardInfo.data.height;

        sizeText.text = $"{width} X {height}";
        modeText.text = stageInfo.data.isInfinityMode ? "무한 모드" : "점수 모드";

        if (SceneManager.GetActiveScene().name == MySceneManager.Instance.boardCreateSceneName)
        {
            SetStartButtonText("로드");

            if (removeButton != null)
                removeButton.gameObject.SetActive(true);
        }
        else
        {
            if (removeButton != null)
                removeButton.gameObject.SetActive(false);
        }

        startButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        removeButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
    }

    public void ConnectChangeSceneStartButtonOnClick()
    {
        startButton.onClick.AddListener(() => GameManager.Instance.SetStageInfo(stageInfo));
        startButton.onClick.AddListener(() => GameManager.Instance.SetBoardInfo(boardInfo));
        startButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
    }

    public void AddOnClickStartButton(UnityAction action)
    {
        startButton.onClick.AddListener(action);
    }

    public void AddOnClickRemoveButton(UnityAction action)
    {
        removeButton.onClick.AddListener(action);
    }

    public void AddOnClickRemoveFolder(BoardType boardType)
    {
        if (removeButton.gameObject.activeSelf)
            removeButton.onClick.AddListener(() => RemoveBoard(boardType));
    }

    public void RemoveBoard(BoardType boardType)
    {
        string folderPath = MyJsonUtility.GetSaveFolderPath(boardType);
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        var folders = Directory.GetDirectories(folderPath);
        foreach (var folder in folders)
        {
            string folderName = Path.GetFileName(folder);
            if (folderName == nameText.text)
            {
                Directory.Delete(folder, true);
                Destroy(gameObject);
                break;
            }
        }
    }

    public GridLayoutGroup GetGridLayoutGroup()
    {
        return boardPreviewGrid;
    }

    public void CreateGrid(GridLayoutGroup gridLayoutGroup, GameObject blockGrid, GameObject unblockGrid)
    {
        StartCoroutine(CoCreateGrid(gridLayoutGroup, blockGrid, unblockGrid));
    }

    public IEnumerator CoCreateGrid(GridLayoutGroup gridLayoutGroup, GameObject blockedPuzzle, GameObject unblockedPuzzle)
    {
        if (gridLayoutGroup.transform.childCount > 0)
            yield break;
        
        int width = boardInfo.data.width;
        int height = boardInfo.data.height;
        boardInfo.LoadGridsBlockData();

        List<GameObject> createdGrids = new();
        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                GameObject obj;
                if (boardInfo.GetBlockedGrid(x, y))
                    obj = Instantiate(blockedPuzzle, gridLayoutGroup.transform);
                else
                    obj = Instantiate(unblockedPuzzle, gridLayoutGroup.transform);

                obj.gameObject.SetActive(false);
                createdGrids.Add(obj);
            }
        }

        yield return null;

        GridLayoutGroup.Constraint constraintType = new();
        if (width >= height) constraintType = GridLayoutGroup.Constraint.FixedColumnCount;
        if (width < height) constraintType = GridLayoutGroup.Constraint.FixedRowCount;

        gridLayoutGroup.constraint = constraintType;
        gridLayoutGroup.constraintCount = boardInfo.GetConstaintCount();

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

    public void SetStartButtonText(string text)
    {
        startButtonText.text = text;
    }

    public StageInfo GetStageInfo()
    {
        return stageInfo;
    }

    public void RemoveButtonAction()
    {
        startButton.onClick.RemoveAllListeners();
    }
}
