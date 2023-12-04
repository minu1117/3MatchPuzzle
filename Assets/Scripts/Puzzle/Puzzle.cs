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
        var sprites = LoadManager.Sprites;
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
            case PuzzleType.Blue:
                scoreNum = 10;
                break;
            case PuzzleType.Green:
                scoreNum = 7;
                break;
            case PuzzleType.Red:
                scoreNum = 8;
                break;
            case PuzzleType.Orange:
                scoreNum = 12;
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
