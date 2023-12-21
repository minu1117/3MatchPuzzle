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

    private List<BoardElements> elements = new();

    private int width = 0;
    private int height = 0;

    private int maxWidth = 10;
    private int maxHeight = 10;

    private int clearScore = 0;
    private bool isInfinityMode = false;

    public string prefabSaveFolderName;

    private void Start()
    {
        widthInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        heightInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        widthInputField.characterLimit = 2;
        heightInputField.characterLimit = 2;

        widthInputField.onValueChanged.AddListener(SetWidth);
        heightInputField.onValueChanged.AddListener(SetHeight);
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
        else if (width > height) 
        {
            elementsGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            elementsGroup.constraintCount = width;
        }
        else if (width == height)
        {
            elementsGroup.constraint = GridLayoutGroup.Constraint.Flexible;
        }
    }

    private void SetSize(ref int size, int maxSize, TMP_InputField inputField, string sizeStr)
    {
        if (int.TryParse(sizeStr, out int parseInt))
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
            inputField.text = sizeStr;
            CreateOrDestroyTiles();
        }
    }

    private void SetWidth(string widthStr)
    {
        SetSize(ref width, maxWidth, widthInputField, widthStr);
    }

    private void SetHeight(string heightStr)
    {
        SetSize(ref height, maxHeight, heightInputField, heightStr);
    }

    private void SetGridNum()
    {
        for (int i = 0; i < elementsGroup.transform.childCount; i++) 
        {
            var elem = elementsGroup.transform.GetChild(i).GetComponent<BoardElements>();
            elements.Add(elem);
        }

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                elements[index].gridNum = (y, x);
                index++;
            }
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

    private void CreatePrefab(string name)
    {
        // �������� ������ ���� ���
        string folderPath = Path.Combine(Application.dataPath, $"{prefabSaveFolderName}/{name}");
        CreateFolder(folderPath);

        // �� ������ ����, ������ �߰�
        var newBoardInfo = new GameObject().AddComponent<BoardInfo>();
        var newStageInfo = new GameObject().AddComponent<StageInfo>();

        SetGridNum();
        newBoardInfo.SetBoardSize(width, height);
        newBoardInfo.CreateGrids();

        for (int i = 0; i < elements.Count; i++)
        {
            newBoardInfo.SetGridBlocked(elements[i].gridNum, true);
        }

        string stagePrefabPath = $"{folderPath}/{name}_StageInfo.prefab";
        string boardPrefabPath = $"{folderPath}/{name}_BoardInfo.prefab";

        GameObject boardInfoPrefab = PrefabUtility.SaveAsPrefabAsset(newBoardInfo.gameObject, boardPrefabPath);

        newStageInfo.boardInfo = boardInfoPrefab.GetComponent<BoardInfo>();
        newStageInfo.clearScore = clearScore;
        newStageInfo.infinityMode = isInfinityMode;
        PrefabUtility.SaveAsPrefabAsset(newStageInfo.gameObject, stagePrefabPath);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            CreatePrefab("Test");
        }
    }
}
