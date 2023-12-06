using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIControl : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;

    private void Start()
    {
        var mySceneMgr = MySceneManager.Instance;
        string gameSceneName = mySceneMgr.gameSceneName;
        gameStartButton.onClick.AddListener(() => mySceneMgr.StartCoLoadScene(gameSceneName));
    }
}
