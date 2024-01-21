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

        ConnectLoadButtonOnClick(GameManager.Instance.customBoardSaveFolderName);
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

    private bool CreatePrefab(string folderName, string name)
    {
        if (nameInputField.text == string.Empty || 
            widthInputField.text == string.Empty ||
            heightInputField.text == string.Empty ||
            (!infinityModeToggle.isOn && maxPlayTimeInputField.text == string.Empty) ||
            (!infinityModeToggle.isOn && scoreInputField.text == string.Empty))
            return false;

        if (!GameManager.Instance.developMode)
            stageCreatedToggle.isOn = false;

        /********************* Board Info Save *********************/

        // 프리펩을 저장할 폴더 경로
        string folderPath = Path.Combine(UnityEngine.Application.dataPath, $"{folderName}/{name}");
        CreateFolder(folderPath);

        // 새 프리팹 생성, 폴더에 추가
        var newBoardInfo = new GameObject().AddComponent<BoardInfo>();

        // 설정 후 저장
        SetGridNum();
        newBoardInfo.SetBoardSize(width, height);
        newBoardInfo.SetGridLayoutData(elementsGroup);

        for (int i = 0; i < elements.Count; i++)
            newBoardInfo.SaveGridBlocked(elements[i].isBlocked);

        // StageInfo에 사용될 프리펩
        var boardInfoPrefab = newBoardInfo.SavePrefab(folderPath, name);



        /********************* Stage Info Save *********************/

        var newStageInfo = new GameObject().AddComponent<StageInfo>();
        newStageInfo.boardInfo = boardInfoPrefab.GetComponent<BoardInfo>();
        //newStageInfo.stageName = Path.GetFileName(folderPath);

        if (!infinityModeToggle.isOn)
        {
            newStageInfo.clearScore = int.Parse(scoreInputField.text);
            newStageInfo.maxPlayTime = int.Parse(maxPlayTimeInputField.text);
        }
        newStageInfo.isInfinityMode = infinityModeToggle.isOn;
        newStageInfo.isStageMode = stageCreatedToggle.isOn;
        newStageInfo.SavePrefab(folderPath, name);

        // 생성된 오브젝트들 삭제
        Destroy(newBoardInfo.gameObject);
        Destroy(newStageInfo.gameObject);
        return true;
    }

    private void Save()
    {
        string folderName = string.Empty;

        if (stageCreatedToggle.gameObject.activeSelf && stageCreatedToggle.isOn && GameManager.Instance.developMode)
            folderName = GameManager.Instance.stageSaveFolderName;
        else
            folderName = GameManager.Instance.customBoardSaveFolderName;

        if (CreatePrefab(folderName, nameInputField.text))
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

    public void Load(StageInfo info)
    {
        width = info.boardInfo.width;
        height = info.boardInfo.height;
        widthInputField.text = width.ToString();
        heightInputField.text = height.ToString();
        scoreInputField.text = info.clearScore.ToString();
        maxPlayTimeInputField.text = info.maxPlayTime.ToString();
        nameInputField.text = info.stageName;
        infinityModeToggle.isOn = info.isInfinityMode;
        if (stageModeGameObject.activeSelf)
        {
            stageCreatedToggle.isOn = info.isStageMode;
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

    public void ControlDevelopMode()
    {
        loadButton.onClick.RemoveAllListeners();
        holder.loader.RemoveAllButtonsAction();

        bool set = GameManager.Instance.developMode = !GameManager.Instance.developMode;
        stageModeGameObject.SetActive(set);

        string folderName = string.Empty;
        if (set)
        {
            folderName = GameManager.Instance.stageSaveFolderName;
        }
        else
        {
            folderName = GameManager.Instance.customBoardSaveFolderName;
        }

        ConnectLoadButtonOnClick(folderName);
    }

    private void ConnectLoadButtonOnClick(string folderName)
    {
        loadButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        loadButton.onClick.AddListener(() => holder.loader.LoadCustomBoard(folderName));
        loadButton.onClick.AddListener(() => holder.controler.On());
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
