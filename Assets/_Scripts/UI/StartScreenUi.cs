using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class StartScreenUi : Tab
    {
        //[SerializeField] NumbersSpawner numbersSpawner;
        [SerializeField] ParticleSystem numbersParticle;

        [Header("GameModeButton")]
        [SerializeField] Button gameModeButton;

        [Header("Scores")]
        [SerializeField] TMP_Text bestScoreEasyText;
        [SerializeField] TMP_Text bestScoreMediumText;
        [SerializeField] TMP_Text bestScoreHardText;

        private void Start()
        {
            gameModeButton.onClick.AddListener(SwitchGameMode);
        }

        public override void Show()
        {
            base.Show();
            UpdateSocres();
            UpdateGameModeButton();
            ToggleSpawning(true);
        }

        public override void Hide()
        {
            base.Hide();
            ToggleSpawning(false);
        }

        private void UpdateGameModeButton()
        {
            if (PlayerData.IsSpeedRunMode) gameModeButton.GetComponentInChildren<TMP_Text>().text = "Speed Run!";
            else gameModeButton.GetComponentInChildren<TMP_Text>().text = "Chillax";
        }

        private void SwitchGameMode()
        {
            PlayerData.IsSpeedRunMode = !PlayerData.IsSpeedRunMode;
            UpdateGameModeButton();
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
            float easyScore = PlayerData.GetBestScore(DifficultyLevel.Easy);
            if (easyScore == 0)
                bestScoreEasyText.text = "--:--";
            else
            {
                TimeSpan bestTime = TimeSpan.FromSeconds(easyScore);
                bestScoreEasyText.text = string.Format("{0:D2}:{1:D2}", bestTime.Minutes, bestTime.Seconds);
            }

            float mediumScore = PlayerData.GetBestScore(DifficultyLevel.Medium);
            if (mediumScore == 0)
                bestScoreMediumText.text = "--:--";
            else
            {
                TimeSpan bestTime = TimeSpan.FromSeconds(mediumScore);
                bestScoreMediumText.text = string.Format("{0:D2}:{1:D2}", bestTime.Minutes, bestTime.Seconds);
            }

            float hardScore = PlayerData.GetBestScore(DifficultyLevel.Hard);
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
