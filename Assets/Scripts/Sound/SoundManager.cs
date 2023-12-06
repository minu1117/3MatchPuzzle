using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    //[SerializeField] private AudioListener listener;
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource explodingSound;

    public static SoundManager Instance;

    private AudioSource bgmObject;
    private AudioSource explodingSoundObject;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        string sceneName = MySceneManager.Instance.menuSceneName;
        if (sceneName == SceneManager.GetSceneByBuildIndex(0).name)
        {
            PlayBgm();
        }
    }

    public void PlayExplodingSound()
    {
        if (explodingSoundObject == null)
        {
            explodingSoundObject = Instantiate(explodingSound, gameObject.transform);
        }

        explodingSoundObject.PlayOneShot(explodingSoundObject.clip);
    }

    public void PlayBgm()
    {
        if (bgmObject == null)
        {
            bgmObject = Instantiate(bgm, gameObject.transform);
            bgmObject.Play();
            bgmObject.loop = true;
        }
    }
}
