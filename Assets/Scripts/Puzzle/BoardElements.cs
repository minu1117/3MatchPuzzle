using UnityEngine;
using UnityEngine.UI;

public class BoardElements : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    private Sprite unBlockedSprite;
    private Sprite blockedSprite;

    public bool isBlocked { get; private set; } = false;
    public (int, int) gridNum { get; set; } // (y,x)

    private void Awake()
    {
        button.onClick.AddListener(SwitchBlocked);
        blockedSprite = GameManager.Instance.blockedGrid.GetComponent<Image>().sprite;
        unBlockedSprite = GameManager.Instance.unblockedGrid.GetComponent<Image>().sprite;
    }

    private void SwitchBlocked()
    {
        image.sprite = image.sprite == blockedSprite ? unBlockedSprite : blockedSprite;
        isBlocked = image.sprite == blockedSprite;
    }

    public void SetBlocked(bool isBlocked)
    {
        image.sprite = isBlocked ? blockedSprite : unBlockedSprite;
        this.isBlocked = isBlocked;
    }
}
