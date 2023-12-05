using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Score score;

    public static UIManager Instanse;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (Instanse == null)
        {
            Instanse = this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
