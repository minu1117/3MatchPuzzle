using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Effect : MonoBehaviour
{
    [SerializeField] private EffectNameEnum effectName;
    [SerializeField] private List<ParticleSystem> particles;
    public IObjectPool<List<ParticleSystem>> pool;
    public List<List<ParticleSystem>> createdParticles = new();
    public int poolSize;

    private int poolMaxSize;

    public void Init()
    {
        poolMaxSize = poolSize * 2;
        pool = new ObjectPool<List<ParticleSystem>>(
            CreateEffect,
            GetEffect,
            Release,
            Destroy,
            maxSize : poolMaxSize
            );
    }

    public EffectNameEnum GetEffectName()
    {
        return effectName;
    }

    public void CreateEffects()
    {
        for (int i = 0; i < poolSize; i++)
        {
            createdParticles.Add(pool.Get());
        }
    }

    private List<ParticleSystem> CreateEffect()
    {
        List<ParticleSystem> particleSystems = new();

        for (int i = 0; i < particles.Count; i++)
        {
            var particle = Instantiate(particles[i]);
            particleSystems.Add(particle);
        }

        return particleSystems;
    }

    private void GetEffect(List<ParticleSystem> particles)
    {
        foreach (var particle in particles)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
        }
    }

    private void Release(List<ParticleSystem> particles)
    {
        foreach (var particle in particles)
        {
            particle.Stop();
            particle.gameObject.SetActive(false);
        }
    }

    private void Destroy(List<ParticleSystem> particles)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            Destroy(particles[i].gameObject);
        }
    }
}
