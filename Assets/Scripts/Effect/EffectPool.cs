using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectPool : MonoBehaviour
{
    [SerializeField] private PuzzleType puzzleEffectType;
    [SerializeField] private Effect effect;
    public IObjectPool<Effect> pool;
    public int poolSize;
    private float particleSize;

    public void Init()
    {
        pool = new ObjectPool<Effect>(
                    Create,
                    Get,
                    Release,
                    Destroy,
                    maxSize: poolSize
        );
    }

    public void SetSize(float size)
    {
        particleSize = size;
    }

    public PuzzleType GetEffectType()
    {
        return puzzleEffectType;
    }

    private Effect Create()
    {
        var effectObject = Instantiate(effect, gameObject.transform);
        effectObject.CreateEffect();
        effectObject.SetSize(particleSize);
        effectObject.gameObject.SetActive(false);
        return effectObject;
    }

    private void Get(Effect effect)
    {
        effect.GetEffect();
        StartCoroutine(CoEffectReleaseTimer(effect));
    }

    private void Release(Effect effect)
    {
        effect.Release();
    }

    private void Destroy(Effect effect)
    {
        Destroy(effect.gameObject);
    }

    public void SetUseEffectPosition(Effect effect, Vector3 position)
    {
        effect.SetEffectPosition(position);
    }

    private IEnumerator CoEffectReleaseTimer(Effect effect)
    {
        IEnumerator<bool> timer = effect.CoReleaseTimer();
        while (timer.MoveNext())
        {
            yield return timer.Current;
        }

        pool.Release(effect);
    }
}
