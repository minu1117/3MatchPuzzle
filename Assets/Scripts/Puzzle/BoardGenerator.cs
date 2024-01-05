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

        if (groupChildCount > boardSize) // Ÿ�� ���� ������ ���� ũ�⺸�� ���� �� ����
        {
            count = groupChildCount - (groupChildCount - boardSize);
            for (int i = groupChildCount - 1; i >= count; i--)
            {
                Destroy(elementsGroup.transform.GetChild(i).gameObject);
            }
        }
        else if (groupChildCount < boardSize) // Ÿ�� ���� ������ ���� ũ�⺸�� ���� �� ����
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
        // ��ο� ������ ������ ����
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        else
        {
            // ui �����ؼ� �̸� �ߺ��Ǿ��ٰ� ���
            // bool������ return�ؼ� �ߺ����� �� ���� ���� �� �ϰ� �߰�
        }
    }

    private void CreatePrefab(string name, bool isStageCreated)
    {
        // �������� ������ ���� ���

        string folderPath = Path.Combine(UnityEngine.Application.dataPath, $"{GameManager.Instance.customBoardSaveFolderName}/{name}");
        if (isStageCreated)
            folderPath = Path.Combine(UnityEngine.Application.dataPath, $"{GameManager.Instance.stageSaveFolderName}/{name}");

        CreateFolder(folderPath);

        // �� ������ ����, ������ �߰�
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
