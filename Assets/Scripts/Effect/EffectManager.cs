using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public List<Effect> effects;
    public Dictionary<EffectNameEnum, Effect> effectDict = new();

    public void Init()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            var name = effects[i].GetEffectName();

            GameObject parentObject = new GameObject($"{name}");
            parentObject.gameObject.transform.parent = gameObject.transform;

            var effect = Instantiate(effects[i], parentObject.transform);
            effect.Init();
            effect.CreateEffects();

            effectDict.Add(name, effect);
        }
    }

    public void DestroyEffect(EffectNameEnum name)
    {
        effectDict[name].pool.Clear();
    }

    public void GetEffect(EffectNameEnum name)
    {
        effectDict[name].pool.Get();
    }
}
