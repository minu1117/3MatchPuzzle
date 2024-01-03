using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DoorControler : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    public float slideTime;
    private Vector2 canvasSize;

    public void Awake()
    {
        if (canvas.TryGetComponent(out CanvasScaler scaler))
        {
            canvasSize.x = scaler.referenceResolution.x;
            canvasSize.y = scaler.referenceResolution.y;
        }
        SetDontDestroyOnLoad();
    }

    public void SetDontDestroyOnLoad()
    {
        DontDestroyOnLoad(canvas.gameObject);
    }

    public async Task Move(Vector2 leftDoorPos, Vector2 rightDoorPos)
    {
        SoundManager.Instance.PlayDoorSlideSound(slideTime);

        await DOTween.Sequence()
            .Join(leftDoor.transform.DOLocalMove(leftDoorPos, slideTime))
            .Join(rightDoor.transform.DOLocalMove(rightDoorPos, slideTime))
            .AsyncWaitForCompletion();

        SoundManager.Instance.StopSlideDoorSound();
        SoundManager.Instance.PlayFullDoorSound();
    }

    public async Task CloseDoor()
    {
        Vector2 left = new Vector2(-canvasSize.x / 2, 0f);
        Vector2 right = new Vector2(canvasSize.x / 2, 0f);
        await Move(left, right);
    }

    public async Task OpenDoor()
    {
        Vector2 left = new Vector2(-canvasSize.x, 0f);
        Vector2 right = new Vector2(canvasSize.x, 0f);
        await Move(left, right);
    }
}
