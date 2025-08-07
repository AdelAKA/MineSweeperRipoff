using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class StartScreen : MonoBehaviour
    {
        private void Start()
        {
            TabsManager.Instance.ShowStartScreen();
            SoundManager.Instance.PlayMusic(ClipName.MainMenu);
            LoadingScreen.Instance.Hide();
        }
    }
}
