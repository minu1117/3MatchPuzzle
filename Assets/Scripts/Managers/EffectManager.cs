using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    [SerializeField] private List<EffectPool> effectPools;
    private Dictionary<PuzzleType, EffectPool> effectPoolDict = new();

    protected override void Awake()
    {
        base.Awake();
    }

    public void CreateEffects(GameObject effectPoolObject, Vector2 size)
    {
        float sizeDelta = Mathf.Max(size.x, size.y);

        for (int i = 0; i < effectPools.Count; i++)
        {
            var type = effectPools[i].GetEffectType();
            var effectPool = Instantiate(effectPools[i], effectPoolObject.transform);
            effectPool.Init();
            effectPool.SetSize(sizeDelta);

            effectPoolDict.Add(type, effectPool);
        }
    }

    public void DestroyEffect(PuzzleType type)
    {
        effectPoolDict[type].pool.Clear();
    }

    public void GetEffect(PuzzleType type, Vector2 position)
    {
        if (effectPoolDict == null || effectPoolDict.Count == 0)
            return;

        var effectPool = effectPoolDict[type];
        var particles = effectPool.pool.Get();
        effectPool.SetUseEffectPosition(particles, position);
    }

    public void ClearEffectDict()
    {
        effectPoolDict.Clear();
    }
}
