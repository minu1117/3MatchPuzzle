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

        //var boardInfo = Instantiate(stage.StageInfo.boardInfo, holder.boardParentObject.transform);
        holder.board.Init(stage.StageInfo.boardInfo.width, stage.StageInfo.boardInfo.height);
        EffectManager.Instance.CreateEffects(holder.GetEffectPoolParent());
        holder.boardMixButton.onClick.AddListener(() => holder.board.Mix());
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
