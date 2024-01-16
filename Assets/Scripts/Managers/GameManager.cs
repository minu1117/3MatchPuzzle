using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool developMode = false;

    public PuzzleSceneObjectHolder puzzleSceneHolder;
    public ModeChoiceSceneHolder modeChoiceSceneHolder;
    private StageInfo stageInfo;
    public string customBoardSaveFolderName;
    public string stageSaveFolderName;

    public GameObject unblockedGrid;
    public GameObject blockedGrid;

    public SpriteObject[] puzzleSprites { get; set; }

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

        puzzleSceneHolder.board.SetBoardInfo(stageInfo.boardInfo);
        puzzleSceneHolder.board.Init(stageInfo.boardInfo.width, stageInfo.boardInfo.height);
        EffectManager.Instance.CreateEffects(puzzleSceneHolder.GetEffectPoolParent());

        puzzleSceneHolder.boardMixButton.onClick.AddListener(() => puzzleSceneHolder.board.Mix());
        puzzleSceneHolder.clearUI.Init();

        puzzleSceneHolder.modeText.text = stageInfo.isInfinityMode ? "무한 ∞" : "점수 모드";
    }

    public void StartChoiceScene()
    {
        modeChoiceSceneHolder = FindAnyObjectByType<ModeChoiceSceneHolder>();

        modeChoiceSceneHolder.loader.LoadCustomBoard(customBoardSaveFolderName);
        modeChoiceSceneHolder.loader.ConnectUIStartButtonOnClick();

        modeChoiceSceneHolder.controler.ConnectEventTrigger();
        modeChoiceSceneHolder.controler.Off();

        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.controler.On());
        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.loader.ConnectAllCreateGrid());
        modeChoiceSceneHolder.stageStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.stageSceneName));
        modeChoiceSceneHolder.createStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.boardCreateSceneName));
    }

    public void SetStageInfo(StageInfo info)
    {
        stageInfo = info;
    }

    public StageInfo GetStageInfo()
    {
        return stageInfo;
    }
}
