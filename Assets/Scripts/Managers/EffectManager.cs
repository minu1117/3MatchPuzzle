using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Manager<EffectManager>
{
    [SerializeField] private List<Effect> effects;
    private Dictionary<PuzzleType, Effect> effectDict = new();

    protected override void Awake()
    {
        base.Awake();
    }

    public void CreateEffects(GameObject effectPoolObject)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            var type = effects[i].GetEffectType();

            GameObject parentObject = new GameObject($"{type} Effect Pool");
            parentObject.gameObject.transform.parent = effectPoolObject.gameObject.transform;
            parentObject.transform.localPosition = Vector3.zero;
            parentObject.transform.localScale = Vector3.one;

            var effect = Instantiate(effects[i], parentObject.transform);
            effect.Init();

            effectDict.Add(type, effect);
        }
    }

    public void DestroyEffect(PuzzleType type)
    {
        effectDict[type].pool.Clear();
    }

    public void GetEffect(PuzzleType type, Vector2 position)
    {
        var effect = effectDict[type];
        var particles = effect.pool.Get();

        effect.SetUseEffectPosition(particles, position);
    }

    public void ClearEffectDict()
    {
        effectDict.Clear();
    }
}
