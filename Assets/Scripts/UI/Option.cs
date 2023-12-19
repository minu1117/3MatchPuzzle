using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [Header("누르면 옵션 창이 열려야 하는 버튼 연결")]
    [SerializeField] private Button optionButton;

    [SerializeField] private GameObject optionObject;
    [SerializeField] private Button exitButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("바뀔 볼륨 이미지들")]
    [SerializeField] private Sprite maxVolumeSprite;
    [SerializeField] private Sprite mediumVolumeSprite;
    [SerializeField] private Sprite minVolumeSprite;
    [SerializeField] private Sprite muteSprite;

    [Header("볼륨 버튼, 볼륨의 이미지")]
    [SerializeField] private Button bgmVolumeButton;
    [SerializeField] private Button sfxVolumeButton;
    [SerializeField] private Image bgmVolumeImage;
    [SerializeField] private Image sfxVolumeImage;

    [Header("볼륨 수치 입력 칸")]
    [SerializeField] private TMP_InputField bgmInputField;
    [SerializeField] private TMP_InputField sfxInputField;

    public void Init()
    {
        SoundManager.Instance.LoadMuteData();

        // Button
        optionButton.onClick.AddListener(() => optionObject.gameObject.SetActive(true));
        exitButton.onClick.AddListener(() => optionObject.gameObject.SetActive(false));

        // Slider
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
        bgmSlider.onValueChanged.AddListener(SwitchBgmVolumeImage);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);
        sfxSlider.onValueChanged.AddListener(SwitchSfxVolumeImage);

        // inputField
        bgmInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        bgmInputField.onEndEdit.AddListener(SoundManager.Instance.ChangeBGMVolume);
        bgmInputField.onEndEdit.AddListener(SwitchBgmVolumeImage);
        sfxInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        sfxInputField.onEndEdit.AddListener(SoundManager.Instance.ChangeSFXVolume);
        sfxInputField.onEndEdit.AddListener(SwitchSfxVolumeImage);

        // VolumeButton
        bgmVolumeButton.onClick.AddListener(SoundManager.Instance.ControlMuteBGM);
        bgmVolumeButton.onClick.AddListener(SwitchBGMSoundSprites);
        sfxVolumeButton.onClick.AddListener(SoundManager.Instance.ControlMuteSFX);
        sfxVolumeButton.onClick.AddListener(SwitchSFXSoundSprites);
    }

    public void SynchronizeVolumeSliderValue()
    {
        string bgmName = SoundManager.Instance.GetBGMGroupName();
        string sfxName = SoundManager.Instance.GetSFXGroupName();
        bgmSlider.value = PlayerPrefs.GetFloat(bgmName);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxName);

        if (!SoundManager.Instance.GetIsMuteBGM())  SwitchBgmVolumeImage(bgmSlider.value);
        else                                        bgmVolumeImage.sprite = muteSprite;

        if (!SoundManager.Instance.GetIsMuteSFX())  SwitchSfxVolumeImage(sfxSlider.value);
        else                                        sfxVolumeImage.sprite = muteSprite;
    }

    private void SwitchBGMSoundSprites()
    {
        if (bgmVolumeImage.sprite == muteSprite)    SwitchVolumeImage(bgmSlider.value, bgmVolumeImage);
        else                                        bgmVolumeImage.sprite = muteSprite;
    }

    private void SwitchSFXSoundSprites()
    {
        if (sfxVolumeImage.sprite == muteSprite)    SwitchVolumeImage(sfxSlider.value, sfxVolumeImage);
        else                                        sfxVolumeImage.sprite = muteSprite;
    }

    private void SwitchBgmVolumeImage(float value)
    {
        if (!SoundManager.Instance.GetIsMuteBGM())  SwitchVolumeImage(value, bgmVolumeImage);

        bgmInputField.text = MathF.Round(value * 100).ToString();
    }
    private void SwitchBgmVolumeImage(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;

            if (!SoundManager.Instance.GetIsMuteBGM())  SwitchVolumeImage(parseFloatValue, bgmVolumeImage);

            bgmSlider.value = parseFloatValue;
            bgmInputField.text = value;
        }
    }

    private void SwitchSfxVolumeImage(float value)
    {
        if (!SoundManager.Instance.GetIsMuteSFX())  SwitchVolumeImage(value, sfxVolumeImage);

        sfxInputField.text = MathF.Round(value * 100).ToString();
    }
    private void SwitchSfxVolumeImage(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;

            if (!SoundManager.Instance.GetIsMuteSFX())  SwitchVolumeImage(parseFloatValue, sfxVolumeImage);

            sfxSlider.value = parseFloatValue;
            sfxInputField.text = value;
        }
    }

    private void SwitchVolumeImage(float value, Image volumeImage)
    {
        if (value == 0f)
        {
            if (volumeImage.sprite != muteSprite)
                volumeImage.sprite = muteSprite;
        }
        else if (value > 0.66f)
        {
            if (volumeImage.sprite != maxVolumeSprite)
                volumeImage.sprite = maxVolumeSprite;
        }
        else if (value <= 0.66f && value > 0.33f)
        {
            if (volumeImage.sprite != mediumVolumeSprite)
                volumeImage.sprite = mediumVolumeSprite;
        }
        else if (value <= 0.33f && value > 0f)
        {
            if (volumeImage.sprite != minVolumeSprite)
                volumeImage.sprite = minVolumeSprite;
        }
    }
}
