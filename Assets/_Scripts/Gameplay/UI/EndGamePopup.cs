using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class EndGamePopup : MonoBehaviour
    {
        [SerializeField] Canvas mainCanvas;
        [SerializeField] TMP_Text endText;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] Button rastartButton;
        [SerializeField] Button backButton;

        private void Start()
        {
            rastartButton.onClick.AddListener(GameplayManager.Instance.RequestRestart);

            backButton.onClick.AddListener(async () =>
            {
                await LoadingScreen.Instance.Show();
                Hide();
                TabsManager.Instance.ShowTab(TabName.StartScreen);
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
