using UnityEngine;
using UnityEngine.UIElements;

public class Puzzle : MonoBehaviour
{
    private SpriteRenderer sprite;
    public bool isMatched = false;
    public bool isConnected = false;
    public PuzzleType type = PuzzleType.None;

    public void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetType(PuzzleType pt)
    {
        type = pt;
    }

    private void SetSprite(Sprite sp)
    {
        sprite.sprite = sp;
    }
}
