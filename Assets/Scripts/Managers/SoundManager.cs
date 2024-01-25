using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource puzzlePopSoundSFX;
    [SerializeField] private AudioSource doorOpenSoundSFX;
    [SerializeField] private AudioSource buttonClickSoundSFX;
    [SerializeField] private AudioSource stageClearSoundSFX;
    [SerializeField] private AudioSource starSparkSoundSFX;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource bgmObject;
    private AudioSource puzzlePopSoundSfxObject;
    private AudioSource doorOpenSoundSfxObject;
    private AudioSource buttonClickSoundSfxObject;
    private AudioSource stageClearSoundSfxObject;
    private AudioSource starSparkSoundSfxObject;

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
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume(string value)
    {
        if (int.TryParse(value, out int parseInt)) 
        {
            float parseFloatValue = parseInt / 100f;
            sfxGroup.audioMixer.SetFloat(sfxGroup.name, Mathf.Log10(parseFloatValue) * 20);
            PlayerPrefs.SetFloat(sfxGroup.name, parseFloatValue);
            PlayerPrefs.Save();
        }
    }

    public void ChangeBGMVolume(float value)
    {
        bgmGroup.audioMixer.SetFloat(bgmGroup.name, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(bgmGroup.name, value);
        PlayerPrefs.Save();
    }

    public void ChangeBGMVolume(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;
            bgmGroup.audioMixer.SetFloat(bgmGroup.name, Mathf.Log10(parseFloatValue) * 20);
            PlayerPrefs.SetFloat(bgmGroup.name, parseFloatValue);
            PlayerPrefs.Save();
        }
    }

    private void PlaySound(AudioSource source, bool playOneShot)
    {
        if (playOneShot)
            source.PlayOneShot(source.clip);
        else
            source.Play();
    }

    private void StopSound(AudioSource source)
    {
        source.Stop();
    }

    public void PlayExplodingSound()
    {
        PlaySound(puzzlePopSoundSfxObject, true);
    }
    
    public void StopSlideDoorSound()
    {
        StopSound(doorOpenSoundSfxObject);
    }

    public void PlayDoorSlideSound()
    {
        doorOpenSoundSfxObject.Play();
    }

    public void PlayStarSparkSound()
    {
        PlaySound(starSparkSoundSfxObject, true);
    }

    public void PlayStageClearSound()
    {
        PlaySound(stageClearSoundSfxObject, true);
    }

    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSoundSfxObject, true);
    }

    public void PlayBgm()
    {
        PlaySound(bgmObject, false);
        bgmObject.loop = true;
    }

    private void CreateSoundObjects()
    {
        bgmObject = Instantiate(bgm, gameObject.transform);
        puzzlePopSoundSfxObject = Instantiate(puzzlePopSoundSFX, gameObject.transform);
        buttonClickSoundSfxObject = Instantiate(buttonClickSoundSFX, gameObject.transform);
        stageClearSoundSfxObject = Instantiate(stageClearSoundSFX, gameObject.transform);
        starSparkSoundSfxObject = Instantiate(starSparkSoundSFX, gameObject.transform);
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
