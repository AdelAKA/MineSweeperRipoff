using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MineSweeperRipeoff
{
    public class NumbersSpawner : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] NumbersCollection numbersCollection;
        [SerializeField] Sprite mineIcon;
        [SerializeField] Sprite FlagIcon;

        [Header("Settings")]
        [SerializeField] FloatingNumber floatingNumberPrefab;
        [SerializeField] Transform container;
        [SerializeField] Vector2 spawnIntervals;

        private bool startSpawning;

        private List<Sprite> spritesPool = new List<Sprite>();
        private List<FloatingNumber> spawnedObjects = new List<FloatingNumber>();

        private void Start()
        {
            foreach (Transform item in container)
            {
                Destroy(item.gameObject);
            }

            spritesPool.Add(mineIcon);
            spritesPool.Add(FlagIcon);

            for (int i = 1; i < 9; i++)
            {
                spritesPool.Add(numbersCollection.GetSpriteOfNumber(i));
            }
        }

        float timer;
        private void Update()
        {
            if (!startSpawning) return;
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Spawn();
                timer = UnityEngine.Random.Range(spawnIntervals.x, spawnIntervals.y);
            }
        }

        public void TurnOn()
        {
            startSpawning = true;
        }

        public void TurnOff()
        {
            startSpawning = false;
            foreach (var spawnedObject in spawnedObjects)
            {
                Destroy(spawnedObject.gameObject);
            }
        }

        private void Spawn()
        {
            FloatingNumber floatingNumber = Instantiate(floatingNumberPrefab, container);
            floatingNumber.Initialize(spritesPool[UnityEngine.Random.Range(0, spritesPool.Count)]);
            spawnedObjects.Add(floatingNumber);
        }
    }
}
