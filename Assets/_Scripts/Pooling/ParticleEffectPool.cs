using UnityEngine;
using UnityEngine.Pool;

namespace MineSweeperRipeoff
{
    public class ParticleEffectPool : MonoBehaviour
    {
        [SerializeField] PooledParticle particleSystemPrefab;
        [SerializeField] Transform container;

        [SerializeField] bool collectionCheck;
        [SerializeField] int defaultSize = 10;
        [SerializeField] int maxSize = 100;

        public IObjectPool<PooledParticle> objectPool;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            objectPool = new ObjectPool<PooledParticle>(CreateParticle, OnGetFromPool, OnReleasedToPool, OnDestroyPooledObject, collectionCheck, defaultSize, maxSize);
        }

        private PooledParticle CreateParticle()
        {
            PooledParticle particle = Instantiate(particleSystemPrefab, container);
            particle.ObjectPool = objectPool;
            return particle;
        }

        private void OnGetFromPool(PooledParticle pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
            //pooledObject.Play();
        }

        private void OnReleasedToPool(PooledParticle pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
            //pooledObject.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnDestroyPooledObject(PooledParticle pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}
