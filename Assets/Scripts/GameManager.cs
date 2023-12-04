using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private Board boardPrefab;
    [SerializeField] private SoundManager soundManagerPrefab;
    [SerializeField] private EffectManager effectManagerPrefab;
    [SerializeField] private UIManager UIManagerPrefab;

    [Header("Parents Objects")]
    //[SerializeField] private AudioListener listener;
    [SerializeField] private GameObject effectPoolParentsObject;
    [SerializeField] private GridLayoutGroup backgroundParentsObject;
    [SerializeField] private GameObject puzzleParentsObject;
    [SerializeField] private GameObject scoreParentsObject;

    private Board board;

    [Header("Board Width, Height")]
    public int width;
    public int height;

    public void Start()
    {
        board = Instantiate(boardPrefab);

        var soundMgr = Instantiate(soundManagerPrefab);
        var effectmgr = Instantiate(effectManagerPrefab);
        var uiMgr = Instantiate(UIManagerPrefab);

        soundMgr.Init();
        effectmgr.Init(effectPoolParentsObject);
        uiMgr.Init(scoreParentsObject);

        // �켱 ���� ���� �ֱ� ������ ��� �����ϰ�, �� �߰� �� �ڵ� �����ؾ���
        // �޴� �� : soundMgr, UIMgr ����
        // ���� �� : board, effectMgr ����

        SoundManager.Instanse.PlayBgm();
        board.Init(backgroundParentsObject, puzzleParentsObject, width, height);

        DontDestroyOnLoad(gameObject);
    }
}
