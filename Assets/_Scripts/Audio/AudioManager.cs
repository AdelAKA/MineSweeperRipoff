using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class AudioManager : MonoBehaviour
    {

        private readonly Dictionary<ClipCategory, List<AudioSource>> audioSourceCategoryDict = new();
        public void AddAudioSourceToCategoryDict(AudioSource audioSource, ClipCategory clipCategory)
        {
            if (!audioSourceCategoryDict.ContainsKey(clipCategory))
                audioSourceCategoryDict[clipCategory] = new List<AudioSource>();
            audioSourceCategoryDict[clipCategory].Add(audioSource);
        }

        private readonly Dictionary<ClipName, AudioClip> audioClipDict = new();
        public void AddAudioClipToAudioClipDict(AudioClip audioClip, ClipName clipName)
        {
            if (audioClipDict.ContainsKey(clipName)) return;
            audioClipDict[clipName] = audioClip;
        }

        public static AudioManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PlayAudio(ClipName.ButtonClicked, 1.5f);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayAudio(ClipName.ButtonClicked, 0.5f);
            }
        }

        public void SetMuteForCategory(ClipCategory clipCategory, bool mute)
        {
            if (!audioSourceCategoryDict.ContainsKey(clipCategory)) return;
            audioSourceCategoryDict[clipCategory].ForEach(source => source.mute = mute);
        }

        public AudioSource GetAudioSourceOnGameObject(GameObject gameObject, ClipName clipName)
        {
            var audioCatchers = gameObject.GetComponentsInChildren<AudioCatcher>();
            if (audioCatchers != null)
            {
                var audioCatcher = audioCatchers.FirstOrDefault(AC => AC.ClipName == clipName);
                if (audioCatcher == null) return null;
                else return audioCatcher.AudioSource;
            }
            else return null;
        }


        // ------------------- 2D SOUNDS ----------------------------

        public void PlayAudio(ClipName clipName, float pitch = 1, bool loop = false, bool checkIsPlaying = false) =>
            PlayAudioOnGameObject(gameObject, clipName, pitch, loop, checkIsPlaying);

        public void PlayMusic(ClipName clipName, float pitch = 1, bool loop = true, bool checkIsPlaying = false) =>
            PlayMusicOnGameObject(gameObject, clipName, pitch, loop, checkIsPlaying);

        public void StopAudio(ClipName clipName) =>
            StopAudioOnGameObject(gameObject, clipName);

        public async Task<float> PlayAudioAfter(ClipName clipName, float time, float chainTime = 0.0f, bool loop = false, bool checkIsPlaying = false) =>
            await PlayAudioOnGameObjectAfter(gameObject, clipName, time, chainTime, loop, checkIsPlaying);

        public async Task<float> PlayAudioAfter(ClipName clipName, ClipName otherClipName, float chainTime = 0.0f, bool loop = false, bool checkIsPlaying = false) =>
            await PlayAudioOnGameObjectAfter(gameObject, clipName, otherClipName, chainTime, loop, checkIsPlaying);


        // ------------------- 3D SOUNDS ----------------------------

        public void PlayAudioOnGameObject(GameObject gameObject, ClipName clipName, float pitch = 1, bool loop = false, bool checkIsPlaying = false)
        {
            var audioSource = GetAudioSourceOnGameObject(gameObject, clipName);
            if (audioSource != null && !checkIsPlaying || audioSource != null && checkIsPlaying && !audioSource.isPlaying)
            {
                audioSource.Play();
                audioSource.loop = loop;
                audioSource.pitch = pitch;
            }
        }

        public void PlayMusicOnGameObject(GameObject gameObject, ClipName clipName, float pitch = 1, bool loop = true, bool checkIsPlaying = false)
        {
            AudioSource audioSource;

            foreach (ClipName clipNameEnum in Enum.GetValues(typeof(ClipName)))
            {
                audioSource = GetAudioSourceOnGameObject(gameObject, clipNameEnum);
                if (audioSource != null && !checkIsPlaying || audioSource != null && checkIsPlaying && !audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }

            audioSource = GetAudioSourceOnGameObject(gameObject, clipName);
            if (audioSource != null && !checkIsPlaying || audioSource != null && checkIsPlaying && !audioSource.isPlaying)
            {
                audioSource.Play();
                audioSource.loop = loop;
                audioSource.pitch = pitch;
            }
        }

        public void StopAudioOnGameObject(GameObject gameObject, ClipName clipName)
        {
            var audioSource = GetAudioSourceOnGameObject(gameObject, clipName);
            if (audioSource != null) audioSource.Stop();
        }

        public async Task<float> PlayAudioOnGameObjectAfter(
                GameObject gameObject,
                ClipName clipName,
                float time,
                float chainTime = 0.0f,
                bool loop = false,
                bool checkIsPlaying = false
            )
        {
            var audioSource = GetAudioSourceOnGameObject(gameObject, clipName);
            if (audioSource != null && !checkIsPlaying || audioSource != null && checkIsPlaying && !audioSource.isPlaying)
            {
                //await Task.Delay((int)(time * 1000.0f));
                await Awaitable.WaitForSecondsAsync(time);

                audioSource.Play();
                audioSource.loop = loop;
                return time + chainTime;
            }
            else return chainTime;
        }

        public async Task<float> PlayAudioOnGameObjectAfter(
                GameObject gameObject,
                ClipName clipName,
                ClipName otherClipName,
                float chainTime = 0.0f,
                bool loop = false,
                bool checkIsPlaying = false
            )
        {
            var audioSource = GetAudioSourceOnGameObject(gameObject, clipName);
            if (audioSource != null && !checkIsPlaying || audioSource != null && checkIsPlaying && !audioSource.isPlaying)
            {
                var time = audioClipDict[otherClipName].length;
                //await Task.Delay((int)(time * 1000.0f));
                await Awaitable.WaitForSecondsAsync(time);
                audioSource.Play();
                audioSource.loop = loop;
                return time + chainTime;
            }
            else return chainTime;
        }
    }
}