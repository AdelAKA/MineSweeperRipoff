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
        private Queue<AudioSource> _sfxAudioSourceQueue;

        CancellationTokenSource _musicCancelationTokenSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _musicAudioSource = Instantiate(sfxPrefab, transform);

            _sfxAudioSourceQueue = new();
            for (int i = 0; i < maxAudioSourceNumber; i++)
            {
                AudioSource newSFX = Instantiate(sfxPrefab, transform);
                newSFX.transform.parent = transform;
                _sfxAudioSourceQueue.Enqueue(newSFX);
            }
        }

        private void Start()
        {
            _musicCancelationTokenSource = new CancellationTokenSource();
        }

        public void ToggleSoundForSFX(bool isMute)
        {
            for (int i = 0; i < _sfxAudioSourceQueue.Count; i++)
            {
                AudioSource audioSource = _sfxAudioSourceQueue.Dequeue();
                audioSource.mute = isMute;
                _sfxAudioSourceQueue.Enqueue(audioSource);
            }
        }

        public void ToggleSoundForMusic(bool isMute)
        {
            _musicAudioSource.mute = isMute;
        }

        public void PlaySound(ClipName clipName, float pitch = 1)
        {
            //GameObject soundGameObject = new GameObject("Sound");
            //AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            AudioSource audioSource = _sfxAudioSourceQueue.Dequeue();
            audioSource.pitch = 1f;
            audioSource.Stop();
            audioSource.clip = GetAudioClip(clipName);
            audioSource.pitch = pitch;
            audioSource.Play();
            _sfxAudioSourceQueue.Enqueue(audioSource);
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
                    //await Task.Delay(1);
                    await Awaitable.NextFrameAsync(_musicCancelationTokenSource.Token);
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