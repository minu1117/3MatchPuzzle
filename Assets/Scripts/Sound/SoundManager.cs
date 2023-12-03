using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioListener listener;
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource explodingSound;

    public static SoundManager Instanse;

    private AudioSource bgmObject;
    private AudioSource explodingSoundObject;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (Instanse == null)
        {
            Instanse = this;
        }

        DontDestroyOnLoad(gameObject);
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
