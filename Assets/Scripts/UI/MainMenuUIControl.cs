using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIControl : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    private void Start()
    {
        var mySceneMgr = MySceneManager.Instance;
        string stageSceneName = mySceneMgr.modeChoiceSceneName;
        gameStartButton.onClick.AddListener(() => mySceneMgr.StartCoLoadScene(stageSceneName));
    }
}
