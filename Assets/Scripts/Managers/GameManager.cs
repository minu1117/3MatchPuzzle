using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool developMode = false;

    public PuzzleSceneObjectHolder puzzleSceneHolder;
    public ModeChoiceSceneHolder modeChoiceSceneHolder;

    private StageInfo stageInfo;
    private BoardInfo boardInfo;

    public string customBoardSaveFolderName;
    public string stageSaveFolderName;

    public GameObject unblockedGrid;
    public GameObject blockedGrid;

    public SpriteObject[] puzzleSprites { get; set; }
    public bool isPaused = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // Stage 파일이 없을 시 PlayerPrefs 초기화
        string filePath = Path.Combine(Application.persistentDataPath, "Stage");
        if (!File.Exists(filePath))
        {
            PlayerPrefs.DeleteAll();
        }

        bool isFirstStart = System.Convert.ToBoolean(PlayerPrefs.GetInt("FirstStart"));
        if (!isFirstStart)
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, "DefaultStages");
            string destinationPath = Path.Combine(Application.persistentDataPath, "Stage");
            MoveFolder(sourcePath, destinationPath);

            int convertTrue = Convert.ToInt32(true);
            PlayerPrefs.SetInt("FirstStart", convertTrue);

            SoundManager.Instance.ChangeBGMVolume("100");
            SoundManager.Instance.ChangeSFXVolume("100");

            PlayerPrefs.Save();
        }
    }

    private void MoveFolder(string sourcePath, string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var folders = Directory.GetFiles(sourcePath);
        foreach (string filePath in folders)
        {
            // .meta 파일 무시
            if (!filePath.EndsWith(".meta"))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(path, fileName);
                File.Copy(filePath, destFilePath, true);
            }
        }

        var directories = Directory.GetDirectories(sourcePath);
        Array.Sort(directories, (x, y) => ExtractNumber(x).CompareTo(ExtractNumber(y)));
        foreach (string subfolderPath in directories)
        {
            string folderName = Path.GetFileName(subfolderPath);
            string destSubfolderPath = Path.Combine(path, folderName);

            // 폴더 이동
            MoveFolder(subfolderPath, destSubfolderPath);
        }
    }

    private int ExtractNumber(string text)
    {
        // 문자열에서 숫자를 추출하여 반환
        string numberString = string.Concat(text.Where(char.IsDigit));
        return int.Parse(numberString);
    }

    private void LoadPuzzleSprites()
    {
        if (puzzleSprites != null)
            return;

        puzzleSprites = Resources.LoadAll<SpriteObject>("PuzzleSpriteObjects/");
    }

    public void StartGame()
    {
        LoadPuzzleSprites();

        puzzleSceneHolder = FindAnyObjectByType<PuzzleSceneObjectHolder>();

        puzzleSceneHolder.board.SetBoardInfo(boardInfo);
        puzzleSceneHolder.board.Init(boardInfo.data.width, boardInfo.data.height);
        EffectManager.Instance.CreateEffects(puzzleSceneHolder.GetEffectPoolParent(), puzzleSceneHolder.board.GetGridSize());

        puzzleSceneHolder.boardMixButton.onClick.AddListener(() => puzzleSceneHolder.board.Mix());
        puzzleSceneHolder.clearUI.Init();

        puzzleSceneHolder.modeText.text = stageInfo.data.isInfinityMode ? "무한 ∞" : $"목표 점수 : {stageInfo.data.clearScore}";
    }

    public void StartChoiceScene()
    {
        modeChoiceSceneHolder = FindAnyObjectByType<ModeChoiceSceneHolder>();

        modeChoiceSceneHolder.loadUIHolder.loader.LoadCustomBoard(BoardType.Custom);
        modeChoiceSceneHolder.loadUIHolder.loader.ConnectUIStartButtonOnClick();

        modeChoiceSceneHolder.loadUIHolder.controler.ConnectEventTrigger();
        modeChoiceSceneHolder.loadUIHolder.controler.Off();

        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        modeChoiceSceneHolder.stageStart.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        modeChoiceSceneHolder.createStart.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());

        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.loadUIHolder.controler.On());
        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.loadUIHolder.loader.ConnectAllCreateGrid());
        modeChoiceSceneHolder.stageStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.stageSceneName));
        modeChoiceSceneHolder.createStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.boardCreateSceneName));
    }

    public void SetBoardInfo(BoardInfo info)
    {
        boardInfo = info;
    }

    public void SetStageInfo(StageInfo info)
    {
        stageInfo = info;
    }

    public BoardInfo GetBoardInfo()
    {
        return boardInfo;
    }

    public StageInfo GetStageInfo()
    {
        return stageInfo;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
}
