using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class BoardGenerator : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup elementsGroup;
    [SerializeField] private BoardElements elementPrefab;

    [SerializeField] private TMP_InputField widthInputField;
    [SerializeField] private TMP_InputField heightInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField scoreInputField;
    [SerializeField] private TMP_InputField maxPlayTimeInputField;
    [SerializeField] private Toggle infinityModeToggle;
    [SerializeField] private Toggle stageCreatedToggle;
    [SerializeField] private GameObject stageModeGameObject;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    [SerializeField] private Image saveImage;
    [SerializeField] private TextMeshProUGUI saveText;
    private float startAlphaValue = 1f;
    private float alphaDuration = 0.5f;
    private Coroutine saveImageRoutine;
    [SerializeField] Sprite spriteSaveSuccess;
    [SerializeField] Sprite spriteSaveFail;

    [SerializeField] private LoadUIHolder holder;

    private List<BoardElements> elements = new();

    private int width = 0;
    private int height = 0;

    private int maxWidth = 15;
    private int maxHeight = 15;

    public string prefabSaveFolderName;

    private void Start()
    {
        widthInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        heightInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        scoreInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        maxPlayTimeInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        nameInputField.characterLimit = 10;

        widthInputField.characterLimit = 2;
        heightInputField.characterLimit = 2;

        widthInputField.onValueChanged.AddListener(SetWidth);
        heightInputField.onValueChanged.AddListener(SetHeight);

        saveButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        saveButton.onClick.AddListener(Save);

        holder.controler.ConnectEventTrigger();
        holder.controler.Off();

        ConnectLoadButtonOnClick(GameManager.Instance.customBoardSaveFolderName, BoardType.Custom);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ControlDevelopMode();
        }
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

    private bool CreateCustomBoard(string name)
    {
        if (nameInputField.text == string.Empty || 
            widthInputField.text == string.Empty ||
            heightInputField.text == string.Empty ||
            (!infinityModeToggle.isOn && maxPlayTimeInputField.text == string.Empty) ||
            (!infinityModeToggle.isOn && scoreInputField.text == string.Empty))
            return false;

        if (!GameManager.Instance.developMode)
            stageCreatedToggle.isOn = false;

        BoardType boardType = (GameManager.Instance.developMode && stageCreatedToggle.isOn) ? BoardType.Stage : BoardType.Custom;

        /********************* Board Info Save *********************/
        BoardInfo boardInfo = new BoardInfo(new BoardInfoData());

        // 설정 후 저장
        SetGridNum();
        boardInfo.SetBoardSize(width, height);
        boardInfo.SetGridLayoutData(elementsGroup);

        for (int i = 0; i < elements.Count; i++)
            boardInfo.SaveGridBlocked(elements[i].isBlocked);

        boardInfo.Save(name, boardType);

        /********************* Stage Info Save *********************/
        StageInfo stageInfo = new StageInfo(new StageInfoData());

        if (!infinityModeToggle.isOn)
        {
            stageInfo.data.clearScore = int.Parse(scoreInputField.text);
            stageInfo.data.maxPlayTime = int.Parse(maxPlayTimeInputField.text);
        }

        stageInfo.data.isInfinityMode = infinityModeToggle.isOn;
        stageInfo.data.isStageMode = stageCreatedToggle.isOn;
        stageInfo.Save(name, boardType);

        return true;
    }

    private void Save()
    {
        if (CreateCustomBoard(nameInputField.text))
        {
            saveImage.sprite = spriteSaveSuccess;
            saveText.text = "저장 성공";
        }
        else
        {
            saveImage.sprite = spriteSaveFail;
            saveText.text = "저장 실패";
        }

        if (saveImageRoutine != null)
            StopCoroutine(saveImageRoutine);

        saveImageRoutine = StartCoroutine(CoFadeSaveImage());
    }

    public void Load(StageInfo stageInfo, BoardInfo boardInfo)
    {
        width = boardInfo.data.width;
        height = boardInfo.data.height;
        widthInputField.text = width.ToString();
        heightInputField.text = height.ToString();
        scoreInputField.text = stageInfo.data.clearScore.ToString();
        maxPlayTimeInputField.text = stageInfo.data.maxPlayTime.ToString();
        nameInputField.text = stageInfo.data.stageName;
        infinityModeToggle.isOn = stageInfo.data.isInfinityMode;
        if (stageModeGameObject.activeSelf)
        {
            stageCreatedToggle.isOn = stageInfo.data.isStageMode;
        }

        CreateOrDestroyTiles();
        elementsGroup.constraint = boardInfo.GetConstraintType();
        elementsGroup.constraintCount = boardInfo.GetConstaintCount();
        SetGridNum();
        for (int i = 0; i < elements.Count; i++)
        {
            int x = elements[i].gridNum.Item2;
            int y = elements[i].gridNum.Item1;

            bool blocked = boardInfo.GetBlockedGrid(x,y);
            elements[i].SetBlocked(blocked);
        }
    }

    public void ControlDevelopMode()
    {
        loadButton.onClick.RemoveAllListeners();
        holder.loader.RemoveAllButtonsAction();

        bool set = GameManager.Instance.developMode = !GameManager.Instance.developMode;
        stageModeGameObject.SetActive(set);

        string folderName = string.Empty;
        BoardType type;
        if (set)
        {
            folderName = GameManager.Instance.stageSaveFolderName;
            type = BoardType.Stage;
        }
        else
        {
            folderName = GameManager.Instance.customBoardSaveFolderName;
            type = BoardType.Custom;
        }

        ConnectLoadButtonOnClick(folderName, type);
    }

    private void ConnectLoadButtonOnClick(string folderName, BoardType type)
    {
        loadButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        loadButton.onClick.AddListener(() => holder.loader.LoadCustomBoard(type));
        loadButton.onClick.AddListener(() => holder.controler.On());
        // 로딩
        loadButton.onClick.AddListener(() => holder.loader.LoadAllBoardData(type));
        loadButton.onClick.AddListener(() => holder.loader.ConnectAllCreateGrid());
        loadButton.onClick.AddListener(() => holder.loader.LoadInGenerator(this));
    }

    private IEnumerator CoFadeSaveImage()
    {
        Color startImageColor = new Color(saveImage.color.r, saveImage.color.g, saveImage.color.b, startAlphaValue);
        Color startTextColor = new Color(saveImage.color.r, saveImage.color.g, saveImage.color.b, startAlphaValue);

        float elapsedTime = 0f;
        var waitForSecond = new WaitForSeconds(Time.deltaTime);
        while (elapsedTime < alphaDuration)
        {
            float t = elapsedTime / alphaDuration;

            startImageColor.a = Mathf.Lerp(startAlphaValue, 0f, t);
            startTextColor.a = Mathf.Lerp(startAlphaValue, 0f, t);

            saveImage.color = startImageColor;
            saveText.color = startTextColor;

            elapsedTime += Time.deltaTime;
            yield return waitForSecond;
        }

        saveImage.color = new Color(saveImage.color.r, saveImage.color.g, saveImage.color.b, 0f);
        saveText.color = new Color(saveText.color.r, saveText.color.g, saveText.color.b, 0f);

        saveImageRoutine = null;
    }
}
