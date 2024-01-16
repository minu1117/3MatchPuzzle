using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class MySceneManager : Singleton<MySceneManager>
{
    public string gameSceneName;
    public string menuSceneName;
    public string stageSceneName;
    public string boardCreateSceneName;
    public string modeChoiceSceneName;

    private bool isLoading = false;

    protected override void Awake()
    {
        base.Awake();
    }

    public async void StartCoLoadScene(string name)
    {
        if (isLoading)
            return;

        isLoading = true;
        await DoorControler.Instance.CloseDoor();

        StartCoroutine(CoLoadScene(name));
        while (!IsSceneLoaded(name))
        {
            await Task.Yield();
        }

        await DoorControler.Instance.OpenDoor();
        isLoading = false;
    }

    public IEnumerator CoLoadScene(string name)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(name);
        
        // 로드가 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            //float progress = Mathf.Clamp01(asyncLoad.progress);
            //Debug.Log($"Loading... {progress * 100f}%");
            yield return null;
        }

        UIManager.Instance.FindOptionObject();

        if (name == gameSceneName)
        {
            GameManager.Instance.StartGame();
        }
        else if (name == modeChoiceSceneName)
        {
            GameManager.Instance.StartChoiceScene();
        }
    }

    private bool IsSceneLoaded(string name)
    {
        return SceneManager.GetSceneByName(name).isLoaded;
    }
}
