using UnityEngine;

namespace MineSweeperRipeoff
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioCatcher : MonoBehaviour
	{
		private AudioSource audioSource;

		[SerializeField] private ClipCategory clipCategory;
		[SerializeField] private ClipName clipName;

		private void Start()
		{
			audioSource = GetComponent<AudioSource>();
			//AudioManager.Instance.AddAudioSourceToClipNameDict(audioSource, clipName);
			AudioManager.Instance.AddAudioSourceToCategoryDict(audioSource, clipCategory);
			AudioManager.Instance.AddAudioClipToAudioClipDict(audioSource.clip, clipName);
		}

		public AudioSource AudioSource => audioSource;
		public ClipCategory ClipCategory => clipCategory;
		public ClipName ClipName => clipName;
	}
}