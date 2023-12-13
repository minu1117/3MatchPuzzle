using UnityEngine;
using System.Collections.Generic;

public class Effect : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particles;
    public List<ParticleSystem> createdParticles;

    public void CreateEffect()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            var particle = Instantiate(particles[i], gameObject.transform);
            particle.gameObject.SetActive(false);
            createdParticles.Add(particle);
        }
    }

    public void GetEffect()
    {
        gameObject.SetActive(true);
        foreach (var particle in createdParticles)
        {
            particle.gameObject.SetActive(true);
            particle.Play();
        }
    }

    public void Release()
    {
        foreach (var particle in createdParticles)
        {
            particle.Stop();
            particle.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public void SetEffectPosition(Vector3 position)
    {
        foreach (var particle in createdParticles)
        {
            particle.gameObject.transform.localPosition = position;
        }
    }

    public IEnumerator<bool> CoReleaseTimer()
    {
        int complateCount = 0;

        while (complateCount < createdParticles.Count)
        {
            foreach (var particle in createdParticles)
            {
                if (particle.isPlaying) break;
                complateCount++;
            }

            yield return false;
        }

        yield return true;
    }
}
