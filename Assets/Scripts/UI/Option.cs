using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [Header("������ �ɼ� â�� ������ �ϴ� ��ư ����")]
    [SerializeField] private Button optionButton;

    [SerializeField] private GameObject optionObject;
    [SerializeField] private Button exitButton;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("�ٲ� ���� �̹�����")]
    [SerializeField] private Sprite maxVolumeSprite;
    [SerializeField] private Sprite mediumVolumeSprite;
    [SerializeField] private Sprite minVolumeSprite;
    [SerializeField] private Sprite muteSprite;

    [Header("���� ��ư, ������ �̹���")]
    [SerializeField] private Button bgmVolumeButton;
    [SerializeField] private Button sfxVolumeButton;
    [SerializeField] private Image bgmVolumeImage;
    [SerializeField] private Image sfxVolumeImage;

    [Header("���� ��ġ �Է� ĭ")]
    [SerializeField] private TMP_InputField bgmInputField;
    [SerializeField] private TMP_InputField sfxInputField;

    public void Init()
    {
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
    }

    public void SynchronizeVolumeSliderValue()
    {
        string bgmName = SoundManager.Instance.GetBGMGroupName();
        string sfxName = SoundManager.Instance.GetSFXGroupName();
        bgmSlider.value = PlayerPrefs.GetFloat(bgmName);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxName);
        SwitchBgmVolumeImage(bgmSlider.value);
        SwitchSfxVolumeImage(sfxSlider.value);
    }

    private void SwitchBgmVolumeImage(float value)
    {
        SwitchVolumeImage(value, bgmVolumeImage);
        bgmInputField.text = MathF.Round(value * 100).ToString();
    }
    private void SwitchBgmVolumeImage(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;
            SwitchVolumeImage(parseFloatValue, bgmVolumeImage);
            bgmSlider.value = parseFloatValue;
            bgmInputField.text = value;
        }
    }

    private void SwitchSfxVolumeImage(float value)
    {
        SwitchVolumeImage(value, sfxVolumeImage);
        sfxInputField.text = MathF.Round(value * 100).ToString();
    }
    private void SwitchSfxVolumeImage(string value)
    {
        if (int.TryParse(value, out int parseInt))
        {
            float parseFloatValue = parseInt / 100f;
            SwitchVolumeImage(parseFloatValue, sfxVolumeImage);
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
