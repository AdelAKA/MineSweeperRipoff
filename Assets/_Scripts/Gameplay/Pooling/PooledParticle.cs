using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace MineSweeperRipeoff
{
    public class PooledParticle : MonoBehaviour
    {
        [SerializeField] ParticleSystem particle;

        public IObjectPool<PooledParticle> ObjectPool { get; set; }

        private IEnumerator StartReleaseTimer()
        {
            yield return new WaitForSeconds(particle.main.duration);
            ObjectPool.Release(this);
        }

        private void OnEnable()
        {
            particle.Play();
            StartCoroutine(StartReleaseTimer());
        }

        private void OnDisable()
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
