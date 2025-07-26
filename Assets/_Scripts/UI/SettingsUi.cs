using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class SettingsUi : MonoBehaviour
    {
        [SerializeField] Canvas mainCanvas;
        
        [SerializeField] Button close;
        
        [SerializeField] Button speedRunButton;
        [SerializeField] Button gameModeButton;

        [Header("Mute Buttons")]
        [SerializeField] Button musicMuteButton;
        [SerializeField] Image musicMuteButtonImage;
        [SerializeField] Sprite musicMuteSprite;
        [SerializeField] Sprite musicPlaySprite;

        [SerializeField] Button sfxMuteButton;
        [SerializeField] Image sfxMuteButtonImage;
        [SerializeField] Sprite sfxMuteSprite;
        [SerializeField] Sprite sfxPlaySprite;

        private void Start()
        {
            speedRunButton.onClick.AddListener(SwitchGameSpeedMode);
            gameModeButton.onClick.AddListener(SwitchGameMode);

            musicMuteButton.onClick.AddListener(SwitchMusicMuteOption);
            sfxMuteButton.onClick.AddListener(SwitchSfxMuteOption);

            UpdateGameSpeedModeButton();
            UpdateGameSpeedModeButton();
            UpdateMusicMuteButton();
            UpdateSfxMuteButton();

            close.onClick.AddListener(Hide);
        }

        public void Show() => mainCanvas.enabled = true;
        public void Hide() => mainCanvas.enabled = false;

        private void UpdateGameSpeedModeButton()
        {
            if (PlayerData.IsSpeedRunMode) speedRunButton.GetComponentInChildren<TMP_Text>().text = "Speed Run!";
            else speedRunButton.GetComponentInChildren<TMP_Text>().text = "Chillax";
        }
        private void SwitchGameSpeedMode()
        {
            GameManager.Instance.SwitchGameSpeedMode();
            UpdateGameSpeedModeButton();
        }

        private void UpdateGameModeButton()
        {
            gameModeButton.GetComponentInChildren<TMP_Text>().text = PlayerData.CurrentGameMode.ToString();
        }
        private void SwitchGameMode()
        {
            GameManager.Instance.SwitchGameMode();
            UpdateGameModeButton();
        }

        private void UpdateMusicMuteButton()
        {
            musicMuteButtonImage.sprite = PlayerData.MusicMuteOption ? musicMuteSprite : musicPlaySprite;
        }
        private void SwitchMusicMuteOption()
        {
            PlayerData.MusicMuteOption = !PlayerData.MusicMuteOption;
            SoundManager.Instance.ToggleSoundForMusic(PlayerData.MusicMuteOption);
            UpdateMusicMuteButton();
        }

        private void UpdateSfxMuteButton()
        {
            sfxMuteButtonImage.sprite = PlayerData.SfxMuteOption ? sfxMuteSprite : sfxPlaySprite;
        }
        private void SwitchSfxMuteOption()
        {
            PlayerData.SfxMuteOption = !PlayerData.SfxMuteOption;
            SoundManager.Instance.ToggleSoundForSFX(PlayerData.SfxMuteOption);
            UpdateSfxMuteButton();
        }
    }
}
