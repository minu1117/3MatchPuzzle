using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup tileGroup;
    [SerializeField] private GameObject backgroundTilePrefab;

    [SerializeField] private TMP_InputField widthInputField;
    [SerializeField] private TMP_InputField heightInputField;

    private int width = 0;
    private int height = 0;

    private int maxWidth = 10;
    private int maxHeight = 10;

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
        int groupChildCount = tileGroup.transform.childCount;

        if (groupChildCount > boardSize) // 타일 수가 설정한 보드 크기보다 많을 때 삭제
        {
            count = groupChildCount - (groupChildCount - boardSize);
            for (int i = groupChildCount - 1; i >= count; i--)
            {
                Destroy(tileGroup.transform.GetChild(i).gameObject);
            }
        }
        else if (groupChildCount < boardSize) // 타일 수가 설정한 보드 크기보다 적을 때 생성
        {
            count = boardSize - groupChildCount;
            for (int i = 0; i < count; i++)
            {
                Instantiate(backgroundTilePrefab, tileGroup.transform);
            }
        }

        UIManager.Instance.FitToCell(tileGroup, width, height);
    }

    private void SetConstaintType()
    {
        if (width < height)
        {
            tileGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            tileGroup.constraintCount = height;
        }
        else if (width > height) 
        {
            tileGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            tileGroup.constraintCount = width;
        }
        else if (width == height)
        {
            tileGroup.constraint = GridLayoutGroup.Constraint.Flexible;
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
}
