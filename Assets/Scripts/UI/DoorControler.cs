using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class DoorControler : Singleton<DoorControler>
{
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;

    [SerializeField] private RectTransform leftDoorRect;
    [SerializeField] private RectTransform rightDoorRect;

    public float slideTime;

    protected override void Awake()
    {
        base.Awake();
    }

    public async Task Move(Vector2 leftDoorPos, Vector2 rightDoorPos)
    {
        SoundManager.Instance.PlayDoorSlideSound();

        await DOTween.Sequence()
            .Join(leftDoor.transform.DOLocalMove(leftDoorPos, slideTime))
            .Join(rightDoor.transform.DOLocalMove(rightDoorPos, slideTime))
            .AsyncWaitForCompletion();

        SoundManager.Instance.StopSlideDoorSound();
    }

    public async Task CloseDoor()
    {
        float leftWidth = leftDoorRect.rect.width;
        float rightWidth = rightDoorRect.rect.width;
        Vector2 left = new Vector2(-leftWidth / 2, 0f);
        Vector2 right = new Vector2(rightWidth / 2, 0f);

        await Move(left, right);
    }

    public async Task OpenDoor()
    {
        float leftWidth = leftDoorRect.rect.width;
        float rightWidth = rightDoorRect.rect.width;
        Vector2 left = new Vector2(-leftWidth - leftWidth / 2, 0f);
        Vector2 right = new Vector2(rightWidth + rightWidth / 2, 0f);

        await Move(left, right);
    }
}
