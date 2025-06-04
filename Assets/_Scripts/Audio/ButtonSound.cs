using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
	[RequireComponent(typeof(Button))]
	public class ButtonSound : MonoBehaviour
	{
		[SerializeField] ClipName clipName = ClipName.ButtonClicked;

        private void Awake()
		{
			var button = GetComponent<Button>();
			//button.onClick.AddListener(() => AudioManager.Instance.PlayAudio(clipName));
            button.onClick.AddListener(() => SoundManager.Instance.PlaySound(clipName));
        }
    }
}