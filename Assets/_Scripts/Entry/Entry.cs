using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MineSweeperRipeoff
{
    public class Entry : MonoBehaviour
    {
        async void Start()
        {
            Application.targetFrameRate = 60;
            SoundManager.Instance.ToggleSoundForMusic(PlayerData.MusicMuteOption);
            SoundManager.Instance.ToggleSoundForSFX(PlayerData.SfxMuteOption);

            await LoadingScreen.Instance.Show();
            SceneManager.LoadScene("GameScene");
        }
    }
}
