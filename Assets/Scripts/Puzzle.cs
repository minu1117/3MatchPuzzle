using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class Puzzle : MonoBehaviour
{
    //private SpriteRenderer sr;
    private Image sr;
    public bool isConnected = false;
    public bool isMatched = false;
    public PuzzleType type = PuzzleType.None;
    public (int, int) gridNum;
    public RectTransform RectTransform { get; private set; }
    private EventTrigger eventTrigger;

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

    public void SetType(PuzzleType pt)
    {
        type = pt;
    }

    public void SetSprite()
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

    public void SetGridNum((int, int) gn)
    {
        gridNum = gn;
    }

    public void SetPosition(Vector2 pos)
    {
        gameObject.transform.localPosition = pos;
    }

    public async Task Move(Vector2 movePos, float speed)
    {
        await DOTween.Sequence()
            .Join(gameObject.transform.DOLocalMove(movePos, speed))
            .AsyncWaitForCompletion();
    }

    public void SetPuzzleType(PuzzleType pt)
    {
        SetType(pt);
        SetSprite();
    }

    public Puzzle SetRandomPuzzleType()
    {
        PuzzleType pt = (PuzzleType)UnityEngine.Random.Range(0, (int)PuzzleType.Count);
        SetPuzzleType(pt);

        return this;
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
        return gridNum.Equals(other.gridNum);
    }
}
