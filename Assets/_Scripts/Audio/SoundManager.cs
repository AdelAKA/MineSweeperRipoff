using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] int maxAudioSourceNumber = 10;
        [SerializeField] AudioSource sfxPrefab;
        [SerializeField] SoundAudioClipDuo[] soundAudioClipDuosArray;

        private AudioSource _musicAudioSource;
        private Queue<AudioSource> _audioSourceQueue;

        CancellationTokenSource _musicCancelationTokenSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        private void Start()
        {
            _musicCancelationTokenSource = new CancellationTokenSource();

            _audioSourceQueue = new();
            _musicAudioSource = Instantiate(sfxPrefab, transform);

            for (int i = 0; i < maxAudioSourceNumber; i++)
            {
                AudioSource newSFX = Instantiate(sfxPrefab, transform);
                newSFX.transform.parent = transform;
                _audioSourceQueue.Enqueue(newSFX);
            }
        }

        public void PlaySound(ClipName clipName, float pitch = 1)
        {
            //GameObject soundGameObject = new GameObject("Sound");
            //AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            AudioSource audioSource = _audioSourceQueue.Dequeue();
            audioSource.pitch = 1f;
            audioSource.Stop();
            audioSource.clip = GetAudioClip(clipName);
            audioSource.pitch = pitch;
            audioSource.Play();
            _audioSourceQueue.Enqueue(audioSource);
        }

        public async void PlayMusic(ClipName clipName, bool loop = false)
        {
            _musicCancelationTokenSource.Cancel();
            _musicCancelationTokenSource.Dispose();
            _musicCancelationTokenSource = new CancellationTokenSource();

            AudioSource audioSource = _musicAudioSource.GetComponent<AudioSource>();
            while (audioSource != null && audioSource.volume > 0)
            {
                audioSource.volume -= Time.deltaTime;
                try
                {
                    await Task.Delay(1);
                    //await Awaitable.NextFrameAsync(_musicCancelationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
            if (audioSource == null) return;
            audioSource.Stop();
            audioSource.clip = GetAudioClip(clipName);
            audioSource.volume = 1;
            audioSource.loop = loop;
            audioSource.Play();
        }

        private AudioClip GetAudioClip(ClipName clipName)
        {
            foreach (SoundAudioClipDuo soundAudioClip in soundAudioClipDuosArray)
            {
                if (clipName == soundAudioClip.clipName)
                {
                    return soundAudioClip.audioClip;
                }
            }
            Debug.Log("No Audio Clip: " + clipName);
            return null;
        }

        [System.Serializable]
        public class SoundAudioClipDuo
        {
            public ClipName clipName;
            public ClipCategory clipCategory;
            public AudioClip audioClip;
        }
    }
}