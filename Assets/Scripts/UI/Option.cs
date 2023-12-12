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
    
    public void ConnectButtonOnClick()
    {
        optionButton.onClick.AddListener(() => optionObject.gameObject.SetActive(true));
        exitButton.onClick.AddListener(() => optionObject.gameObject.SetActive(false));
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeBGMVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.ChangeSFXVolume);
    }

    public void SynchronizeVolumeSliderValue()
    {
        string bgmName = SoundManager.Instance.GetBGMGroupName();
        string sfxName = SoundManager.Instance.GetSFXGroupName();
        bgmSlider.value = PlayerPrefs.GetFloat(bgmName);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxName);
    }
}
