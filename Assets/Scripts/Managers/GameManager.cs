public class GameManager : Manager<GameManager>
{
    public PuzzleSceneObjectHolder holder;
    private Stage stage;

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartGame()
    {
        holder = FindAnyObjectByType<PuzzleSceneObjectHolder>();

        holder.board.SetBoardInfo(stage.StageInfo.boardInfo);
        holder.board.Init(stage.StageInfo.boardInfo.width, stage.StageInfo.boardInfo.height);
        EffectManager.Instance.CreateEffects(holder.GetEffectPoolParent());

        holder.boardMixButton.onClick.AddListener(() => holder.board.Mix());

        holder.exitButton.onClick.AddListener(() => holder.board.StopTask());
        holder.exitButton.onClick.AddListener(() => stage.StageInfo.boardInfo.ClearSaveDict());
        holder.exitButton.onClick.AddListener(() => stage = null);
        holder.exitButton.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        holder.exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.menuSceneName));
    }

    public void SetStage(Stage st)
    {
        stage = st;
    }

    public Stage GetStage()
    {
        return stage;
    }
}
