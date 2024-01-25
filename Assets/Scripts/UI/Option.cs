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
    [SerializeField] private Button exitButton;

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
            optionOnButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
            optionOnButton.onClick.AddListener(() => OnOptionPanel());
        }
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => Application.Quit());
        }

        soundOptionButton.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        soundOptionButton.onClick.AddListener(() => soundOption.gameObject.SetActive(true));
        InitSoundOption();
        AddOnClickAllButtons();
    }

    public void EscapeControl()
    {
        if (optionPanel.activeSelf)
            SoundManager.Instance.PlayButtonClickSound();

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
        GameManager.Instance.Pause();
    }

    public void OffOptionPanel()
    {
        optionPanel.gameObject.SetActive(false);
        GameManager.Instance.Resume();
    }

    public void SwitchOptionActive()
    {
        bool isActive = !optionPanel.activeSelf;
        optionPanel.SetActive(isActive);
        
        if (isActive)
            GameManager.Instance.Pause();
        else
            GameManager.Instance.Resume();
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
        btn.onClick.AddListener(() => SoundManager.Instance.PlayButtonClickSound());
        btn.onClick.AddListener(GameManager.Instance.Resume);

        if (SceneManager.GetActiveScene().name == MySceneManager.Instance.gameSceneName)
        {
            btn.onClick.AddListener(() => GameManager.Instance.puzzleSceneHolder.board.StopTask());
            btn.onClick.AddListener(() => GameManager.Instance.GetBoardInfo().ClearSaveDict());
            btn.onClick.AddListener(() => GameManager.Instance.SetStageInfo(null));
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
}