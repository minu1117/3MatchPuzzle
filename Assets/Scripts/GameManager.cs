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

        // �켱 ���� ���� �ֱ� ������ ��� �����ϰ�, �� �߰� �� �ڵ� �����ؾ���
        // �޴� �� : soundMgr, UIMgr ����
        // ���� �� : board, effectMgr ����

        SoundManager.Instanse.PlayBgm();
        board.Init(backgroundParentsObject, puzzleParentsObject, width, height);
        boardMixButton.onClick.AddListener(() => board.Mix());

        DontDestroyOnLoad(gameObject);
    }
}
