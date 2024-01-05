using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] private Button optionOnButton;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private SoundOption soundOption;
    [SerializeField] private Button soundOptionButton;
    [SerializeField] private Button menuExitButton;
    [SerializeField] private Button modeChoiceExitButton;
    [SerializeField] private Button stageExitButton;

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) 
        {
            EscapeControl();
        }
    }

    public void Start()
    {
        if (optionOnButton != null)
        {
            optionOnButton.onClick.AddListener(() => OnOptionPanel());
        }
        soundOptionButton.onClick.AddListener(() => soundOption.gameObject.SetActive(true));
        InitSoundOption();
        AddOnClickAllButtons();
    }

    public void EscapeControl()
    {
        if (soundOption.gameObject.activeSelf)
        {
            soundOption.gameObject.SetActive(false);
            return;
        }

        if (optionOnButton != null)
        {
            OffOptionPanel();
        }
        else
        {
            SwitchOptionActive();
        }
    }

    public void OnOptionPanel()
    {
        optionPanel.gameObject.SetActive(true);
    }

    public void OffOptionPanel()
    {
        optionPanel.gameObject.SetActive(false);
    }

    public void SwitchOptionActive()
    {
        optionPanel.gameObject.SetActive(!optionPanel.gameObject.activeSelf);
    }

    public void InitSoundOption()
    {
        soundOption.Init();
        soundOption.SynchronizeVolumeSliderValue();
    }

    public void OnSoundOption()
    {
        soundOption.gameObject.SetActive(true);
    }

    private void AddOnClickAllButtons()
    {
        AddButtonOnClick(menuExitButton, MySceneManager.Instance.menuSceneName);
        AddButtonOnClick(modeChoiceExitButton, MySceneManager.Instance.modeChoiceSceneName);
        AddButtonOnClick(stageExitButton, MySceneManager.Instance.stageSceneName);
    }

    private void AddButtonOnClick(Button btn, string sceneName)
    {
        if (SceneManager.GetActiveScene().name == MySceneManager.Instance.gameSceneName)
        {
            btn.onClick.AddListener(() => EffectManager.Instance.ClearEffectDict());
        }

        string name = MySceneManager.Instance.menuSceneName;
        btn.onClick.AddListener(() => MySceneManager.Instance.StartCoLoadScene(sceneName));
    }

    private void OnActiveButton(Button btn)
    {
        btn.gameObject.SetActive(true);
    }

    public void OnAllButtons()
    {
        OnMenuExitButton();
        OnStageExitButton();
        OnModeChoiceExitButton();
    }

    public void OnMenuExitButton()
    {
        OnActiveButton(menuExitButton);
    }
    public void OnStageExitButton()
    {
        OnActiveButton(stageExitButton);
    }
    public void OnModeChoiceExitButton()
    {
        OnActiveButton(modeChoiceExitButton);
    }

    //[SerializeField] private GameObject optionObject;
    //[SerializeField] private Slider bgmSlider;
    //[SerializeField] private Slider sfxSlider;

    //[Header("¹Ù²ð º¼·ý ÀÌ¹ÌÁöµé")]
    //[SerializeField] private Sprite maxVolumeSprite;
    //[SerializeField] private Sprite mediumVolumeSprite;
    //[SerializeField] private Sprite minVolumeSprite;
    //[SerializeField] private Sprite muteSprite;

    //[Header("º¼·ý ¹öÆ°, º¼·ýÀÇ ÀÌ¹ÌÁö")]
    //[SerializeField] private Button bgmVolumeButton;
    //[SerializeField] private Button sfxVolumeButton;
    //[SerializeField] private Image bgmVolumeImage;
    //[SerializeField] private Image sfxVolumeImage;

    //[Header("º¼·ý ¼öÄ¡ ÀÔ·Â Ä­")]
    //[SerializeField] private TMP_InputField bgmInputField;
    //[SerializeField] private TMP_InputField sfxInputField;

    //public void Init()
    //{
    //    SoundManager.Instance.LoadMuteData();

    //    // Button
    //    //optionButton.onClick.AddListener(() => optionObject.gameObject.SetActive(true));
    //    //exitButton.onClick.AddListener(() => optionObject.gameObject.SetActive(false));

    //    // Slider
    //    bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
    //    bgmSlider.onValueChanged.AddListener(SwitchBgmVolumeImage);
    //    sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);
    //    sfxSlider.onValueChanged.AddListener(SwitchSfxVolumeImage);

    //    // inputField
    //    bgmInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
    //    bgmInputField.onEndEdit.AddListener(SoundManager.Instance.ChangeBGMVolume);
    //    bgmInputField.onEndEdit.AddListener(SwitchBgmVolumeImage);
    //    sfxInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
    //    sfxInputField.onEndEdit.AddListener(SoundManager.Instance.ChangeSFXVolume);
    //    sfxInputField.onEndEdit.AddListener(SwitchSfxVolumeImage);

    //    // VolumeButton
    //    bgmVolumeButton.onClick.AddListener(SoundManager.Instance.ControlMuteBGM);
    //    bgmVolumeButton.onClick.AddListener(SwitchBGMSoundSprites);
    //    sfxVolumeButton.onClick.AddListener(SoundManager.Instance.ControlMuteSFX);
    //    sfxVolumeButton.onClick.AddListener(SwitchSFXSoundSprites);
    //}

    //private void SwitchOptionActive()
    //{
    //    optionObject.gameObject.SetActive(!optionObject.gameObject.activeSelf);
    //}

    //public void SynchronizeVolumeSliderValue()
    //{
    //    string bgmName = SoundManager.Instance.GetBGMGroupName();
    //    string sfxName = SoundManager.Instance.GetSFXGroupName();
    //    bgmSlider.value = PlayerPrefs.GetFloat(bgmName);
    //    sfxSlider.value = PlayerPrefs.GetFloat(sfxName);

    //    if (!SoundManager.Instance.GetIsMuteBGM())  SwitchBgmVolumeImage(bgmSlider.value);
    //    else                                        bgmVolumeImage.sprite = muteSprite;

    //    if (!SoundManager.Instance.GetIsMuteSFX())  SwitchSfxVolumeImage(sfxSlider.value);
    //    else                                        sfxVolumeImage.sprite = muteSprite;
    //}

    //private void SwitchBGMSoundSprites()
    //{
    //    if (bgmVolumeImage.sprite == muteSprite)    SwitchVolumeImage(bgmSlider.value, bgmVolumeImage);
    //    else                                        bgmVolumeImage.sprite = muteSprite;
    //}

    //private void SwitchSFXSoundSprites()
    //{
    //    if (sfxVolumeImage.sprite == muteSprite)    SwitchVolumeImage(sfxSlider.value, sfxVolumeImage);
    //    else                                        sfxVolumeImage.sprite = muteSprite;
    //}

    //private void SwitchBgmVolumeImage(float value)
    //{
    //    if (!SoundManager.Instance.GetIsMuteBGM())  SwitchVolumeImage(value, bgmVolumeImage);

    //    bgmInputField.text = MathF.Round(value * 100).ToString();
    //}
    //private void SwitchBgmVolumeImage(string value)
    //{
    //    if (int.TryParse(value, out int parseInt))
    //    {
    //        float parseFloatValue = parseInt / 100f;

    //        if (!SoundManager.Instance.GetIsMuteBGM())  SwitchVolumeImage(parseFloatValue, bgmVolumeImage);

    //        bgmSlider.value = parseFloatValue;
    //        bgmInputField.text = value;
    //    }
    //}

    //private void SwitchSfxVolumeImage(float value)
    //{
    //    if (!SoundManager.Instance.GetIsMuteSFX())  SwitchVolumeImage(value, sfxVolumeImage);

    //    sfxInputField.text = MathF.Round(value * 100).ToString();
    //}
    //private void SwitchSfxVolumeImage(string value)
    //{
    //    if (int.TryParse(value, out int parseInt))
    //    {
    //        float parseFloatValue = parseInt / 100f;

    //        if (!SoundManager.Instance.GetIsMuteSFX())  SwitchVolumeImage(parseFloatValue, sfxVolumeImage);

    //        sfxSlider.value = parseFloatValue;
    //        sfxInputField.text = value;
    //    }
    //}

    //private void SwitchVolumeImage(float value, Image volumeImage)
    //{
    //    if (value == 0f)
    //    {
    //        if (volumeImage.sprite != muteSprite)
    //            volumeImage.sprite = muteSprite;
    //    }
    //    else if (value > 0.66f)
    //    {
    //        if (volumeImage.sprite != maxVolumeSprite)
    //            volumeImage.sprite = maxVolumeSprite;
    //    }
    //    else if (value <= 0.66f && value > 0.33f)
    //    {
    //        if (volumeImage.sprite != mediumVolumeSprite)
    //            volumeImage.sprite = mediumVolumeSprite;
    //    }
    //    else if (value <= 0.33f && value > 0f)
    //    {
    //        if (volumeImage.sprite != minVolumeSprite)
    //            volumeImage.sprite = minVolumeSprite;
    //    }
    //}
}
