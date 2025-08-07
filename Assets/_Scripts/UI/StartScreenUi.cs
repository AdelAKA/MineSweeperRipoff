using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class StartScreenUi : Tab
    {
        //[SerializeField] NumbersSpawner numbersSpawner;
        [SerializeField] ParticleSystem numbersParticle;


        [Header("Settings Panel")]
        [SerializeField] SettingsUi settingsUi;
        [SerializeField] Button settingsButton;

        [Header("Credits Panel")]
        [SerializeField] CreditsUi creditsUi;
        [SerializeField] Button creditsButton;

        [Header("Game Mode Buttons")]
        [SerializeField] Button easyDifficultyButton;
        [SerializeField] Button mediumDifficultyButton;
        [SerializeField] Button hardDifficultyButton;

        [Header("Scores")]
        [SerializeField] TMP_Text gameModeText;
        [SerializeField] TMP_Text bestScoreEasyText;
        [SerializeField] TMP_Text bestScoreMediumText;
        [SerializeField] TMP_Text bestScoreHardText;

        private void Start()
        {
            settingsButton.onClick.AddListener(settingsUi.Show);
            creditsButton.onClick.AddListener(creditsUi.Show);
            GameManager.Instance.OnGameModeChanged += UpdateSocres;

            easyDifficultyButton.onClick.AddListener(() => GameManager.Instance.RequestGame(DifficultyLevel.Easy));
            mediumDifficultyButton.onClick.AddListener(() => GameManager.Instance.RequestGame(DifficultyLevel.Medium));
            hardDifficultyButton.onClick.AddListener(() => GameManager.Instance.RequestGame(DifficultyLevel.Hard));
        }

        public override void Show()
        {
            base.Show();
            UpdateSocres();
            settingsUi.Hide();
            creditsUi.Hide();
            ToggleSpawning(true);
        }

        public override void Hide()
        {
            base.Hide();
            ToggleSpawning(false);
        }

        public void ToggleSpawning(bool spawn)
        {
            if (spawn) numbersParticle.Play();
            else numbersParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            //if (spawn) numbersSpawner.TurnOn();
            //else numbersSpawner.TurnOff();
        }

        public void UpdateSocres()
        {
            gameModeText.text = GameManager.Instance.CurrentGameMode.ToString();

            float easyScore = PlayerData.GetBestScore(DifficultyLevel.Easy, GameManager.Instance.CurrentGameMode);
            if (easyScore == 0)
                bestScoreEasyText.text = "--:--";
            else
            {
                TimeSpan bestTime = TimeSpan.FromSeconds(easyScore);
                bestScoreEasyText.text = string.Format("{0:D2}:{1:D2}", bestTime.Minutes, bestTime.Seconds);
            }

            float mediumScore = PlayerData.GetBestScore(DifficultyLevel.Medium, GameManager.Instance.CurrentGameMode);
            if (mediumScore == 0)
                bestScoreMediumText.text = "--:--";
            else
            {
                TimeSpan bestTime = TimeSpan.FromSeconds(mediumScore);
                bestScoreMediumText.text = string.Format("{0:D2}:{1:D2}", bestTime.Minutes, bestTime.Seconds);
            }

            float hardScore = PlayerData.GetBestScore(DifficultyLevel.Hard, GameManager.Instance.CurrentGameMode);
            if (hardScore == 0)
                bestScoreHardText.text = "--:--";
            else
            {
                TimeSpan bestTime = TimeSpan.FromSeconds(hardScore);
                bestScoreHardText.text = string.Format("{0:D2}:{1:D2}", bestTime.Minutes, bestTime.Seconds);
            }
        }
    }
}
