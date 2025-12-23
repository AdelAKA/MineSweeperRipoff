using System;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public static class PlayerData
    {
        public static event Action OnGameModeChanged;

        public static int GetCurrentWinStreak(DifficultyLevel difficultyLevel, GameMode gameMode)
            => PlayerPrefs.HasKey("CurrentStreak" + difficultyLevel.ToString() + gameMode.ToString()) ?
            PlayerPrefs.GetInt("CurrentStreak" + difficultyLevel.ToString() + gameMode.ToString()) : 0;

        public static float GetBestScore(DifficultyLevel difficultyLevel, GameMode gameMode)
            => PlayerPrefs.HasKey("BestScore" + difficultyLevel.ToString() + gameMode.ToString()) ?
            PlayerPrefs.GetFloat("BestScore" + difficultyLevel.ToString() + gameMode.ToString()) : 0;

        public static void SetCurrentWinStreak(DifficultyLevel difficultyLevel, GameMode gameMode, int newStreak)
            => PlayerPrefs.SetInt("CurrentStreak" + difficultyLevel.ToString() + gameMode.ToString(), newStreak);

        public static void SetBestScore(DifficultyLevel difficultyLevel, GameMode gameMode, float newScore)
            => PlayerPrefs.SetFloat("BestScore" + difficultyLevel.ToString() + gameMode.ToString(), newScore);

        public static bool IsSpeedRunMode
        {
            get => PlayerPrefs.HasKey("IsSpeedRunMode") ? (PlayerPrefs.GetInt("IsSpeedRunMode") == 1 ? true : false) : false;
            set => PlayerPrefs.SetInt("IsSpeedRunMode", value ? 1 : 0);
        }

        public static GameMode CurrentGameMode
        {
            get => PlayerPrefs.HasKey("GameMode") ? (GameMode)PlayerPrefs.GetInt("GameMode") : GameMode.Chance;
            set
            {
                PlayerPrefs.SetInt("GameMode", (int)value);
                OnGameModeChanged?.Invoke();
            }
        }

        public static bool MusicMuteOption
        {
            get => PlayerPrefs.HasKey("MusicMuteOption") ? (PlayerPrefs.GetInt("MusicMuteOption") == 1 ? true : false) : false;
            set => PlayerPrefs.SetInt("MusicMuteOption", value ? 1 : 0);
        }

        public static bool SfxMuteOption
        {
            get => PlayerPrefs.HasKey("SfxMuteOption") ? (PlayerPrefs.GetInt("SfxMuteOption") == 1 ? true : false) : false;
            set => PlayerPrefs.SetInt("SfxMuteOption", value ? 1 : 0);
        }
    }
}
