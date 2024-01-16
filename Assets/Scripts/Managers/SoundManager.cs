using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource puzzlePopSoundSFX;
    [SerializeField] private AudioSource doorCloseSoundSFX;
    [SerializeField] private AudioSource doorOpenSoundSFX;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource bgmObject;
    private AudioSource puzzlePopSoundSfxObject;
    private AudioSource doorCloseSoundSfxObject;
    private AudioSource doorOpenSoundSfxObject;

    private bool muteBGM;
    private bool muteSFX;

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

    public void LoadMuteData()
    {
        muteBGM = System.Convert.ToBoolean(PlayerPrefs.GetInt("MuteBGM"));
        muteSFX = System.Convert.ToBoolean(PlayerPrefs.GetInt("MuteSFX"));
    }

    public bool GetIsMuteBGM()
    {
        return muteBGM;
    }

    public bool GetIsMuteSFX()
    {
        return muteSFX;
    }

    public void ControlMuteBGM()
    {
        muteBGM = !muteBGM;
        PlayerPrefs.SetInt("MuteBGM", System.Convert.ToInt16(muteBGM));

        bgmObject.mute = muteBGM;
    }

    public void ControlMuteSFX()
    {
        muteSFX = !muteSFX;
        PlayerPrefs.SetInt("MuteSFX", System.Convert.ToInt16(muteSFX));

        puzzlePopSoundSfxObject.mute = muteSFX;
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
        puzzlePopSoundSfxObject.PlayOneShot(puzzlePopSoundSfxObject.clip);
    }

    public void PlayFullDoorSound()
    {
        doorCloseSoundSfxObject.PlayOneShot(doorCloseSoundSfxObject.clip);
    }
    
    public void StopSlideDoorSound()
    {
        doorOpenSoundSfxObject.Stop();
    }

    public void PlayDoorSlideSound()
    {
        doorOpenSoundSfxObject.Play();
    }

    public void PlayBgm()
    {
        bgmObject.Play();
        bgmObject.loop = true;
    }

    private void CreateSoundObjects()
    {
        bgmObject = Instantiate(bgm, gameObject.transform);
        puzzlePopSoundSfxObject = Instantiate(puzzlePopSoundSFX, gameObject.transform);
        doorCloseSoundSfxObject = Instantiate(doorCloseSoundSFX, gameObject.transform);
        doorOpenSoundSfxObject = Instantiate(doorOpenSoundSFX, gameObject.transform);
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
