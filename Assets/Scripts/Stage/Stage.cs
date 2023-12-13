using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    public StageInfo StageInfo;
    public TextMeshProUGUI textMeshPro;
    public Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
}
