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
