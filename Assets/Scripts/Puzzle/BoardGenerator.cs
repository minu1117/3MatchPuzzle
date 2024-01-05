using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup elementsGroup;
    [SerializeField] private BoardElements elementPrefab;

    [SerializeField] private TMP_InputField widthInputField;
    [SerializeField] private TMP_InputField heightInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Toggle isInfinityModeToggle;
    [SerializeField] private Toggle isStageCreatedToggle;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button loadButton;

    [SerializeField] private ModeChoiceSceneHolder holder;

    private List<BoardElements> elements = new();

    private int width = 0;
    private int height = 0;

    private int maxWidth = 10;
    private int maxHeight = 10;

    private int clearScore = 0;

    public string prefabSaveFolderName;

    private void Start()
    {
        widthInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        heightInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        widthInputField.characterLimit = 2;
        heightInputField.characterLimit = 2;

        widthInputField.onValueChanged.AddListener(SetWidth);
        heightInputField.onValueChanged.AddListener(SetHeight);

        saveButton.onClick.AddListener(Save);
        loadButton.onClick.AddListener(() => holder.controler.On());
        loadButton.onClick.AddListener(() => holder.loader.ConnectAllCreateGrid());

        holder.loader.Init();
        holder.loader.LoadInGenerator(this);
        holder.controler.ConnectEventTrigger();
        holder.controler.Off();
    }

    private void CreateOrDestroyTiles()
    {
        int count;
        int boardSize = width * height;
        int groupChildCount = elementsGroup.transform.childCount;

        if (groupChildCount > boardSize) // 타일 수가 설정한 보드 크기보다 많을 때 삭제
        {
            count = groupChildCount - (groupChildCount - boardSize);
            for (int i = groupChildCount - 1; i >= count; i--)
            {
                Destroy(elementsGroup.transform.GetChild(i).gameObject);
            }
        }
        else if (groupChildCount < boardSize) // 타일 수가 설정한 보드 크기보다 적을 때 생성
        {
            count = boardSize - groupChildCount;
            for (int i = 0; i < count; i++)
            {
                Instantiate(elementPrefab, elementsGroup.transform);
            }
        }

        UIManager.Instance.FitToCell(elementsGroup, width, height);
    }

    private void SetConstaintType()
    {
        if (width < height)
        {
            elementsGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            elementsGroup.constraintCount = height;
        }
        else if (width >= height)
        {
            elementsGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            elementsGroup.constraintCount = width;
        }
    }

    private void SetSize(ref int size, int maxSize, TMP_InputField inputField)
    {
        if (int.TryParse(inputField.text, out int parseInt))
        {
            if (parseInt == size)
                return;

            size = parseInt;
            if (size > maxSize)
            {
                size = maxSize;
                inputField.text = maxSize.ToString();
            }

            SetConstaintType();
            CreateOrDestroyTiles();
        }
    }

    private void SetWidth(string widthStr)
    {
        if (int.TryParse(widthStr, out int parseInt))
        {
            SetSize(ref width, maxWidth, widthInputField);
        }
    }

    private void SetHeight(string heightStr)
    {
        if (int.TryParse(heightStr, out int parseInt))
        {
            SetSize(ref height, maxHeight, heightInputField);
        }
    }

    private void SetGridNum()
    {
        elements.Clear();

        int x = 0;
        int y = 0;
        for (int i = 0; i < elementsGroup.transform.childCount; i++) 
        {
            var elem = elementsGroup.transform.GetChild(i).GetComponent<BoardElements>();
            elem.gridNum = (y, x);

            x++;
            if (x == width)
            {
                x = 0;
                y++;
            }

            elements.Add(elem);
        }
    }

    private void CreateFolder(string folderPath)
    {
        // 경로에 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        else
        {
            // ui 연결해서 이름 중복되었다고 출력
            // bool형으로 return해서 중복됐을 때 저장 실행 안 하게 추가
        }
    }

    private void CreatePrefab(string name, bool isStageCreated)
    {
        // 프리팹을 저장할 폴더 경로

        string folderPath = Path.Combine(UnityEngine.Application.dataPath, $"{GameManager.Instance.customBoardSaveFolderName}/{name}");
        if (isStageCreated)
            folderPath = Path.Combine(UnityEngine.Application.dataPath, $"{GameManager.Instance.stageSaveFolderName}/{name}");

        CreateFolder(folderPath);

        // 새 프리팹 생성, 폴더에 추가
        var newBoardInfo = new GameObject().AddComponent<BoardInfo>();
        var newStageInfo = new GameObject().AddComponent<StageInfo>();

        SetGridNum();
        newBoardInfo.SetBoardSize(width, height);
        newBoardInfo.SetGridLayoutData(elementsGroup);

        for (int i = 0; i < elements.Count; i++)
        {
            newBoardInfo.SaveGridBlocked(elements[i].isBlocked);
        }

        string stagePrefabPath = $"{folderPath}/{name}_StageInfo.prefab";
        string boardPrefabPath = $"{folderPath}/{name}_BoardInfo.prefab";

        GameObject boardInfoPrefab = PrefabUtility.SaveAsPrefabAsset(newBoardInfo.gameObject, boardPrefabPath);

        newStageInfo.boardInfo = boardInfoPrefab.GetComponent<BoardInfo>();
        newStageInfo.clearScore = clearScore;
        newStageInfo.stageName = Path.GetFileName(folderPath);
        newStageInfo.isInfinityMode = isInfinityModeToggle.isOn;
        newStageInfo.isStageMode = isStageCreatedToggle.isOn;
        PrefabUtility.SaveAsPrefabAsset(newStageInfo.gameObject, stagePrefabPath);

        Destroy(newBoardInfo.gameObject);
        Destroy(newStageInfo.gameObject);
    }

    private void Save()
    {
        CreatePrefab(nameInputField.text, isStageCreatedToggle.isOn);
    }

    public void Load(StageInfo info)
    {
        width = info.boardInfo.width;
        height = info.boardInfo.height;
        widthInputField.text = width.ToString();
        heightInputField.text = height.ToString();
        nameInputField.text = info.stageName;
        isInfinityModeToggle.isOn = info.isInfinityMode;
        if (isStageCreatedToggle != null)
        {
            isStageCreatedToggle.isOn = info.isStageMode;
        }

        CreateOrDestroyTiles();
        elementsGroup.constraint = info.boardInfo.GetConstraintType();
        elementsGroup.constraintCount = info.boardInfo.GetConstaintCount();
        SetGridNum();
        for (int i = 0; i < elements.Count; i++)
        {
            int x = elements[i].gridNum.Item2;
            int y = elements[i].gridNum.Item1;

            bool blocked = info.boardInfo.GetBlockedGrid(x,y);
            elements[i].SetBlocked(blocked);
        }
    }
}
