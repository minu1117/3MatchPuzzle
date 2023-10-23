using System.Collections;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    private SpriteRenderer sr;
    public bool isMatched = false;
    public bool isConnected = false;
    public PuzzleType type = PuzzleType.None;
    public (int, int) gridNum;

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

    public void SetGridNum((int, int) gn)
    {
        gridNum = gn;
    }

    public IEnumerator CoMove(Vector2 movePos, float speed)
    {
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(gameObject.transform.position, movePos);

        while (true)
        {
            float distanceCovered = (Time.time - startTime) * speed;
            float journeyFraction = distanceCovered / journeyLength;

            gameObject.transform.position = Vector2.Lerp(gameObject.transform.position, movePos, journeyFraction);

            if (journeyFraction >= 1.0f)
            {
                yield break;
            }

            yield return null;
        }
    }
}
