using UnityEngine;

namespace MineSweeperRipeoff
{
    public class TabsManager : MonoBehaviour
    {
        public static TabsManager Instance { get; set; }

        [SerializeField] Tab startTab;
        [SerializeField] Tab gameTab;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void ShowStartScreen()
        {
            startTab.Show();
            gameTab.Hide();
            SoundManager.Instance.PlayMusic(ClipName.MainMenu);
        }

        public void ShowGameScreen()
        {
            startTab.Hide();
            gameTab.Show();
            SoundManager.Instance.PlayMusic(ClipName.Playing);
        }
    }
}
