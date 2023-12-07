using UnityEngine;

public class Manager<T> : MonoBehaviour
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = GetComponent<T>();
        DontDestroyOnLoad(gameObject);
    }
}
