using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public List<Effect> effects;
    public Dictionary<PuzzleType, Effect> effectDict = new();
    public static EffectManager Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
    }

    public void Init()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            var type = effects[i].GetEffectType();

            GameObject parentObject = new GameObject($"{name}");
            parentObject.gameObject.transform.parent = gameObject.transform;
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
}
