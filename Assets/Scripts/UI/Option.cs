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

    public void ConnectButtonOnClick()
    {
        optionButton.onClick.AddListener(() => optionObject.gameObject.SetActive(true));
        exitButton.onClick.AddListener(() => optionObject.gameObject.SetActive(false));

        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
        bgmSlider.onValueChanged.AddListener(SwitchBgmVolumeImage);

        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);
        sfxSlider.onValueChanged.AddListener(SwitchSfxVolumeImage);

    }

    public void SynchronizeVolumeSliderValue()
    {
        string bgmName = SoundManager.Instance.GetBGMGroupName();
        string sfxName = SoundManager.Instance.GetSFXGroupName();
        bgmSlider.value = PlayerPrefs.GetFloat(bgmName);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxName);
    }

    private void SwitchBgmVolumeImage(float value)
    {
        SwitchVolumeImage(value, bgmVolumeImage);
    }

    private void SwitchSfxVolumeImage(float value)
    {
        SwitchVolumeImage(value, sfxVolumeImage);
    }

    private void SwitchVolumeImage(float value, Image volumeImage)
    {
        if (value == 0f)
        {
            if (volumeImage.sprite != muteSprite)
                volumeImage.sprite = muteSprite;
        }
        else if (value >= 0.75f)
        {
            if (volumeImage.sprite != maxVolumeSprite)
                volumeImage.sprite = maxVolumeSprite;
        }
        else if (value >= 0.5f)
        {
            if (volumeImage.sprite != mediumVolumeSprite)
                volumeImage.sprite = mediumVolumeSprite;
        }
        else if (value > 0f)
        {
            if (volumeImage.sprite != minVolumeSprite)
                volumeImage.sprite = minVolumeSprite;
        }
    }
}
