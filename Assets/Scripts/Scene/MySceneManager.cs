using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance;
    public string gameSceneName;
    public string menuSceneName;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartCoLoadScene(string name)
    {
        StartCoroutine(CoLoadScene(name));
    }

    public IEnumerator CoLoadScene(string name)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(name);

        // 로드가 완료될 때까지 대기 (테스트)
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress);
            Debug.Log($"Loading... {progress * 100f}%");
            yield return null;
        }
    }
}
