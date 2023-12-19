using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Manager<SoundManager>
{
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
        PlayerPrefs.SetFloat(sfxGroup.name, value);
    }

    public void ChangeSFXVolume(string value)
    {
        if (int.TryParse(value, out int parseInt)) 
        {
            float parseFloatValue = parseInt / 100f;
            sfxGroup.audioMixer.SetFloat(sfxGroup.name, Mathf.Log10(parseFloatValue) * 20);
            PlayerPrefs.SetFloat(sfxGroup.name, parseFloatValue);
        }
    }

    public void ChangeBGMVolume(float value)
    {
        bgmGroup.audioMixer.SetFloat(bgmGroup.name, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(bgmGroup.name, value);
    }

    public void ChangeBGMVolume(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;
            bgmGroup.audioMixer.SetFloat(bgmGroup.name, Mathf.Log10(parseFloatValue) * 20);
            PlayerPrefs.SetFloat(bgmGroup.name, parseFloatValue);
        }
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

    public string GetBGMGroupName()
    {
        return bgmGroup.name;
    }
    public string GetSFXGroupName()
    {
        return sfxGroup.name;
    }
}
