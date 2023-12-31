using UnityEngine;

public class GameManager : Manager<GameManager>
{
    public PuzzleSceneObjectHolder puzzleSceneHolder;
    public ModeChoiceSceneHolder modeChoiceSceneHolder;
    private StageInfo stageInfo;
    public string customBoardSaveFolderName;
    public string stageSaveFolderName;

    public GameObject unblockedPuzzle;
    public GameObject blockedPuzzle;
    public BoardElements blockedElement;

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
        puzzleSceneHolder.clearUI.Init();
    }

    public void StartChoiceScene()
    {
        modeChoiceSceneHolder = FindAnyObjectByType<ModeChoiceSceneHolder>();

        modeChoiceSceneHolder.loader.LoadCustomBoard();
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
