using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Parents Objects")]
    [SerializeField] private GameObject effectPoolParentsObject;
    [SerializeField] private GridLayoutGroup backgroundParentsObject;
    [SerializeField] private GameObject puzzleParentsObject;

    [Header("Board")]
    [SerializeField] private Board boardPrefab;
    public Button boardMixButton;
    private Board board;
    public int width;
    public int height;

    [Header("UI")]
    public Score score;
    public Button exitButton;

    public void Start()
    {
        board = Instantiate(boardPrefab);
        board.Init(backgroundParentsObject, puzzleParentsObject, width, height);
        EffectManager.Instance.CreateEffects(effectPoolParentsObject);
        boardMixButton.onClick.AddListener(() => board.Mix());
        exitButton.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.menuSceneName));
    }
}
