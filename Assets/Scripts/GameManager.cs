using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private Board boardPrefab;
    [SerializeField] private SoundManager soundManagerPrefab;
    [SerializeField] private EffectManager effectManagerPrefab;

    [Header("Parents Objects")]
    //[SerializeField] private AudioListener listener;
    [SerializeField] private GameObject effectPoolParentsObject;
    [SerializeField] private GridLayoutGroup backgroundParentsObject;
    [SerializeField] private GameObject puzzleParentsObject;

    public Button boardMixButton;
    private Board board;

    [Header("Board Width, Height")]
    public int width;
    public int height;

    public static GameManager Instanse;

    public void Start()
    {
        if (Instanse == null)
        {
            Instanse = this;
        }

        board = Instantiate(boardPrefab);

        var soundMgr = Instantiate(soundManagerPrefab);
        var effectmgr = Instantiate(effectManagerPrefab);

        soundMgr.Init();
        effectmgr.Init(effectPoolParentsObject);

        // 우선 게임 씬만 있기 때문에 모두 생성하고, 씬 추가 후 코드 변경해야함
        // 메뉴 씬 : soundMgr, UIMgr 생성
        // 게임 씬 : board, effectMgr 생성

        SoundManager.Instanse.PlayBgm();
        board.Init(backgroundParentsObject, puzzleParentsObject, width, height);
        boardMixButton.onClick.AddListener(() => board.Mix());

        DontDestroyOnLoad(gameObject);
    }
}
