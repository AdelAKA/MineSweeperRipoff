using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class EndGameUi : MonoBehaviour
    {
        [SerializeField] Canvas mainCanvas;
        [SerializeField] TMP_Text endText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button rastartButton;
        [SerializeField] Button backButton;

        private void Start()
        {
            rastartButton.onClick.AddListener(GameManager.Instance.RequestRestart);

            backButton.onClick.AddListener(async () =>
            {
                await LoadingScreen.Instance.Show();
                Hide();
                TabsManager.Instance.ShowStartScreen();
                LoadingScreen.Instance.Hide();
            });
        }
        
        public void Show(string text, string description = "")
        {
            mainCanvas.enabled = true;
            endText.text = text;
            descriptionText.text = description;
        }

        public void Hide()
        {
            mainCanvas.enabled = false;
        }
    }
}
