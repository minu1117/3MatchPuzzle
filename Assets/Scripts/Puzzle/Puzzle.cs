using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Puzzle : MonoBehaviour
{
    public bool isConnected = false;
    public bool isMatched = false;
    public RectTransform RectTransform { get; private set; }
    public (int, int) GridNum { get; set; }
    public PuzzleType type = PuzzleType.None;

    private Image sr;
    private EventTrigger eventTrigger;

    public int scoreNum { get; set; } = 0;

    public void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        eventTrigger = GetComponent<EventTrigger>();
    }

    public void AddPointerDownEventTrigger(Action<Puzzle> action)
    {
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerDown
        };

        entry.callback.AddListener((eventData) => { action(this); });
        eventTrigger.triggers.Add(entry);
    }

    public void AddDragEventTrigger(Action action)
    {
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.Drag
        };

        entry.callback.AddListener((eventData) => { action(); });
        eventTrigger.triggers.Add(entry);
    }

    private void SetSprite()
    {
        if (sr == null)
        {
            sr = GetComponent<Image>();
        }

        Sprite sp = FindSprite();
        if (sp != null)
        {
            sr.sprite = sp;
        }
        else
        {
            Debug.Log("Not Found");
        }
    }

    private Sprite FindSprite()
    {
        var sprites = GameManager.Instance.puzzleSprites;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i].type == type)
            {
                return sprites[i].sprite;
            }
        }

        return null;
    }

    public void SetPosition(Vector2 pos)
    {
        gameObject.transform.localPosition = pos;
    }

    public Vector2 GetPosition()
    {
        return gameObject.transform.localPosition;
    }

    public async Task Move(Vector2 movePos, float speed)
    {
        await DOTween.Sequence()
            .Join(gameObject.transform.DOLocalMove(movePos, speed))
            .AsyncWaitForCompletion();
    }

    // 사이즈 0 -> size 값
    //public async Task Expands(Vector2 size, float duration)
    //{
    //    SetSize(Vector2.zero);

    //    var tcs = new TaskCompletionSource<bool>();

    //    RectTransform.DOSizeDelta(size, duration)
    //        .OnComplete(() => tcs.SetResult(true));

    //    await tcs.Task;
    //}

    // 사이즈 0 -> size 값 보다 조금 더 커지고 -> size 값으로 작아짐
    public async Task Expands(Vector2 size, float duration)
    {
        Vector2 upSize = size + (size * 0.3f);
        SetSize(Vector2.zero);

        float halfDuration = duration * 0.5f;
        var tcs1 = new TaskCompletionSource<bool>();
        var tcs2 = new TaskCompletionSource<bool>();

        RectTransform.DOSizeDelta(upSize, halfDuration)
            .OnComplete(() => tcs1.SetResult(true));
        await tcs1.Task;

        RectTransform.DOSizeDelta(size, halfDuration)
            .OnComplete(() => tcs2.SetResult(true));
        await tcs2.Task;
    }

    public void SetPuzzleType(PuzzleType pt)
    {
        type = pt;
        SetAddScore();
        SetSprite();
    }

    public Puzzle SetRandomPuzzleType()
    {
        PuzzleType pt = (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count);
        SetPuzzleType(pt);

        return this;
    }

    public void SetSize(Vector2 size)
    {
        RectTransform.sizeDelta = size;
    }

    private void SetAddScore()
    {
        switch (type)
        {
            case PuzzleType.Apple:
                scoreNum = 10;
                break;
            case PuzzleType.Kiwi:
                scoreNum = 7;
                break;
            case PuzzleType.Orange:
                scoreNum = 8;
                break;
            case PuzzleType.Avocado:
                scoreNum = 12;
                break;
            case PuzzleType.Grape:
                scoreNum = 9;
                break;
            default:
                break;
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Puzzle other = (Puzzle)obj;
        return GridNum.Equals(other.GridNum);
    }
}
