using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] private GameObject optionObject;
    [SerializeField] private Button optionButton;
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
}
