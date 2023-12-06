using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Parents Objects")]
    //[SerializeField] private AudioListener listener;
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
        EffectManager.Instance.Init(effectPoolParentsObject);
        board.Init(backgroundParentsObject, puzzleParentsObject, width, height);
        boardMixButton.onClick.AddListener(() => board.Mix());
        exitButton.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(MySceneManager.Instance.menuSceneName));
    }
}
