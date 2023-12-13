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

        var board = Instantiate(stage.StageInfo.board, holder.boardParentObject.transform);
        board.Init(stage.StageInfo.boardWidth, stage.StageInfo.boardHeight);
        EffectManager.Instance.CreateEffects(holder.GetEffectPoolParent());
        holder.boardMixButton.onClick.AddListener(() => board.Mix());
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
