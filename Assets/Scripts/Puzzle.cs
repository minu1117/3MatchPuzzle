using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class Puzzle : MonoBehaviour
{
    private SpriteRenderer sr;
    public bool isConnected = false;
    public bool isMatched = false;
    public PuzzleType type = PuzzleType.None;
    public (int, int) gridNum;

    public void SetType(PuzzleType pt)
    {
        type = pt;
    }

    public void SetSprite(SpriteRenderer[] sprites)
    {
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

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

    public void SetPosition(Vector2 pos)
    {
        gameObject.transform.position = pos;
    }

    public IEnumerator CoMove(Vector2 movePos, float speed)
    {
        float startTime = Time.time;
        float journeyLength = Vector2.Distance(gameObject.transform.position, movePos);
        float journeyFraction = 0f;

        while (journeyFraction < 1.0f)
        {
            float distanceCovered = (Time.time - startTime) * speed;
            journeyFraction = distanceCovered / journeyLength;
            journeyFraction = Mathf.Clamp01(journeyFraction);

            gameObject.transform.position = Vector2.Lerp(gameObject.transform.position, movePos, journeyFraction);

            yield return null;
        }
    }

    public void Move(Vector2 movePos, float speed)
    {
        //var sequence = DOTween.Sequence();
        //sequence.Join(gameObject.transform.DOMove(movePos, speed));
        //await sequence.Play().AsyncWaitForCompletion();

        DOTween.Sequence().Join(gameObject.transform.DOMove(movePos, speed));
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
