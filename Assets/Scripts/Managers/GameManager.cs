public class GameManager : Manager<GameManager>
{
    public PuzzleSceneObjectHolder puzzleSceneHolder;
    public ModeChoiceSceneHolder modeChoiceSceneHolder;
    private StageInfo stageInfo;
    public string prefabSaveFolderName;

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartGame()
    {
        puzzleSceneHolder = FindAnyObjectByType<PuzzleSceneObjectHolder>();

        puzzleSceneHolder.board.SetBoardInfo(stageInfo.boardInfo);
        puzzleSceneHolder.board.Init(stageInfo.boardInfo.width, stageInfo.boardInfo.height);
        EffectManager.Instance.CreateEffects(puzzleSceneHolder.GetEffectPoolParent());

        puzzleSceneHolder.boardMixButton.onClick.AddListener(() => puzzleSceneHolder.board.Mix());

        puzzleSceneHolder.exitButton.onClick.AddListener(() => puzzleSceneHolder.board.StopTask());
        puzzleSceneHolder.exitButton.onClick.AddListener(() => stageInfo.boardInfo.ClearSaveDict());
        puzzleSceneHolder.exitButton.onClick.AddListener(() => stageInfo = null);
        puzzleSceneHolder.exitButton.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        puzzleSceneHolder.exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.menuSceneName));
    }

    public void StartChoiceScene()
    {
        modeChoiceSceneHolder = FindAnyObjectByType<ModeChoiceSceneHolder>();

        modeChoiceSceneHolder.loader.Init();

        modeChoiceSceneHolder.controler.ConnectEventTrigger();
        modeChoiceSceneHolder.controler.Off();

        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.controler.On());
        modeChoiceSceneHolder.customBoardStart.onClick.AddListener(() => modeChoiceSceneHolder.loader.ConnectAllCreateGrid());
        modeChoiceSceneHolder.stageStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.gameSceneName));
        modeChoiceSceneHolder.createStart.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.boardCreateSceneName));
    }

    public void SetStageInfo(StageInfo info)
    {
        stageInfo = info;
    }

    public StageInfo GetStage()
    {
        return stageInfo;
    }
}
