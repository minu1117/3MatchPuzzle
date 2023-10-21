using UnityEngine;

public class Puzzle : MonoBehaviour
{
    private SpriteRenderer sr;
    public bool isMatched = false;
    public bool isConnected = false;
    public PuzzleType type = PuzzleType.None;

    public void SetType(PuzzleType pt, SpriteRenderer[] sprites)
    {
        sr = GetComponent<SpriteRenderer>();

        type = pt;
        string tag = type.ToString().ToLower();
        Sprite sp = FindSprite(sprites, tag);
        if (sp != null)
        {
            sr.sprite = sp;
        }
        else
        {
            Debug.Log("Not Found");
        }
    }

    private Sprite FindSprite(SpriteRenderer[] sprites, string findTag)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i].tag.ToLower() == findTag)
            {
                return sprites[i].sprite;
            }
        }

        return null;
    }
}
