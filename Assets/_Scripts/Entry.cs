using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class Entry : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            TabsManager.Instance.ShowStartScreen();
            //AudioManager.Instance.PlayMusic(ClipName.MainMenu);
            SoundManager.Instance.ToggleSoundForMusic(PlayerData.MusicMuteOption);
            SoundManager.Instance.ToggleSoundForSFX(PlayerData.SfxMuteOption);

            SoundManager.Instance.PlayMusic(ClipName.MainMenu);

        }

        public void Action_StartGame(int difficulty)
        {
            GameManager.Instance.RequestGame((DifficultyLevel)difficulty);
            Prepare();
        }

        private void Prepare()
        {
            TabsManager.Instance.ShowGameScreen();
        }
    }
}
