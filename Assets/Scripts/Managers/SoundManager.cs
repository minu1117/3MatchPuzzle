using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Manager<SoundManager>
{
    //[SerializeField] private AudioListener listener;
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource explodingSound;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource bgmObject;
    private AudioSource explodingSoundObject;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        CreateSoundObjects();

        string sceneName = MySceneManager.Instance.menuSceneName;
        if (sceneName == SceneManager.GetSceneByBuildIndex(0).name)
        {
            PlayBgm();
        }
    }

    public void ChangeSFXVolume(float value)
    {
        sfxGroup.audioMixer.SetFloat(sfxGroup.name, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(bgmGroup.name, value);
    }

    public void ChangeBGMVolume(float value)
    {
        bgmGroup.audioMixer.SetFloat(bgmGroup.name, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(bgmGroup.name, value);
    }

    public void PlayExplodingSound()
    {
        explodingSoundObject.PlayOneShot(explodingSoundObject.clip);
    }

    public void PlayBgm()
    {
        bgmObject.Play();
        bgmObject.loop = true;
    }

    private void CreateSoundObjects()
    {
        bgmObject = Instantiate(bgm, gameObject.transform);
        explodingSoundObject = Instantiate(explodingSound, gameObject.transform);
    }
}
