using UnityEngine;

namespace MineSweeperRipeoff
{
    public static class PlayerData
    {
        public static int GetCurrentWinStreak(DifficultyLevel difficultyLevel)
        {
            return PlayerPrefs.HasKey("CurrentStreak" + difficultyLevel.ToString()) ? PlayerPrefs.GetInt("CurrentStreak" + difficultyLevel.ToString()) : 0;

            //switch (difficultyLevel)
            //{
            //    case DifficultyLevel.Easy:
            //        return CurrentStreakEasy;
            //    case DifficultyLevel.Medium:
            //        return CurrentStreakMedium;
            //    case DifficultyLevel.Hard:
            //        return CurrentStreakHard;
            //    default:
            //        return 0;
            //}
        }

        public static float GetBestScore(DifficultyLevel difficultyLevel)
        {
            return PlayerPrefs.HasKey("BestScore" + difficultyLevel.ToString()) ? PlayerPrefs.GetFloat("BestScore" + difficultyLevel.ToString()) : 0;
            //switch (difficultyLevel)
            //{
            //    case DifficultyLevel.Easy:
            //        return BestScoreEasy;
            //    case DifficultyLevel.Medium:
            //        return BestScoreMedium;
            //    case DifficultyLevel.Hard:
            //        return BestScoreHard;
            //    default:
            //        return 0;
            //}
        }

        public static void SetCurrentWinStreak(DifficultyLevel difficultyLevel, int newStreak)
        {
            PlayerPrefs.SetInt("CurrentStreak" + difficultyLevel.ToString(), newStreak);

            //switch (difficultyLevel)
            //{
            //    case DifficultyLevel.Easy:
            //        CurrentStreakEasy = newStreak;
            //        break;
            //    case DifficultyLevel.Medium:
            //        CurrentStreakMedium = newStreak;
            //        break;
            //    case DifficultyLevel.Hard:
            //        CurrentStreakHard = newStreak;
            //        break;
            //    default:
            //        break;
            //}
        }

        public static void SetBestScore(DifficultyLevel difficultyLevel, float newScore)
        {
            PlayerPrefs.SetFloat("BestScore" + difficultyLevel.ToString(), newScore);

            //switch (difficultyLevel)
            //{
            //    case DifficultyLevel.Easy:
            //        BestScoreEasy = newScore;
            //        break;
            //    case DifficultyLevel.Medium:
            //        BestScoreMedium = newScore;
            //        break;
            //    case DifficultyLevel.Hard:
            //        BestScoreHard = newScore;
            //        break;
            //    default:
            //        break;
            //}
        }

        public static bool IsSpeedRunMode
        {
            get => PlayerPrefs.HasKey("IsSpeedRunMode") ? (PlayerPrefs.GetInt("IsSpeedRunMode") == 1 ? true : false) : false;
            set => PlayerPrefs.SetInt("IsSpeedRunMode", value ? 1 : 0);
        }

        //public static int CurrentStreakEasy
        //{
        //    get => PlayerPrefs.HasKey("CurrentStreakEasy") ? PlayerPrefs.GetInt("CurrentStreakEasy") : 0;
        //    set => PlayerPrefs.SetInt("CurrentStreakEasy", value);
        //}

        //public static float BestScoreEasy
        //{
        //    get => PlayerPrefs.HasKey("BestScoreEasy") ? PlayerPrefs.GetFloat("BestScoreEasy") : 0;
        //    set => PlayerPrefs.SetFloat("BestScoreEasy", value);
        //}

        //public static int CurrentStreakMedium
        //{
        //    get => PlayerPrefs.HasKey("CurrentStreakMedium") ? PlayerPrefs.GetInt("CurrentStreakMedium") : 0;
        //    set => PlayerPrefs.SetInt("CurrentStreakMedium", value);
        //}

        //public static float BestScoreMedium
        //{
        //    get => PlayerPrefs.HasKey("BestScoreMedium") ? PlayerPrefs.GetFloat("BestScoreMedium") : 0;
        //    set => PlayerPrefs.SetFloat("BestScoreMedium", value);
        //}

        //public static int CurrentStreakHard
        //{
        //    get => PlayerPrefs.HasKey("CurrentStreakHard") ? PlayerPrefs.GetInt("CurrentStreakHard") : 0;
        //    set => PlayerPrefs.SetInt("CurrentStreakHard", value);
        //}

        //public static float BestScoreHard
        //{
        //    get => PlayerPrefs.HasKey("BestScoreHard") ? PlayerPrefs.GetFloat("BestScoreHard") : 0;
        //    set => PlayerPrefs.SetFloat("BestScoreHard", value);
        //}
    }
}
