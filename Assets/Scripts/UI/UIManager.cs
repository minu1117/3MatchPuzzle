using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Score scorePrefab;
    public Score score { get; set; }

    public static UIManager Instanse;

    public void Init(GameObject scoreParentsObject)
    {
        if (Instanse == null)
        {
            Instanse = this;
        }

        DontDestroyOnLoad(gameObject);

        // ���Ӿ��϶���
        score = Instantiate(scorePrefab, scoreParentsObject.transform);
    }
}
