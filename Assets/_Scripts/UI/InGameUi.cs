using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_EDITOR && !PLATFORM_STANDALONE_WIN
using CandyCoded.HapticFeedback;
#endif

namespace MineSweeperRipeoff
{
    public class InGameUi : Tab
    {
        [Header("Statistics")]
        [SerializeField] TMP_Text minesCounterText;
        [SerializeField] TMP_Text timerText;
        [SerializeField] TMP_Text currentWinStreakText;

        [Header("Buttons")]
        [SerializeField] Button resetButton;
        [SerializeField] Button backButton;

        [Header("Face")]
        [SerializeField] Image faceImage;
        [SerializeField] Sprite happyFaceSprite;
        [SerializeField] Sprite coolFaceSprite;
        [SerializeField] Sprite deadFaceSprite;
        [SerializeField] Sprite surprisedFaceSprite;

        [Header("Mobile Buttons")]
        [SerializeField] GameObject mobileModeButtons;
        [SerializeField] SwitchableButton clearButton;
        [SerializeField] SwitchableButton flagButton;

        [Header("End Panel")]
        [SerializeField] EndGameUi endGameUi;

        [Header("Particle Effects")]
        [SerializeField] ParticleSystem confetti;
        [SerializeField] ParticleSystem poof;

        private void Start()
        {
            OnNewGame();
            resetButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance.CurrentGridState > GridState.Playing) return;
                GameManager.Instance.RequestRestart();
            });
            backButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance.CurrentGridState > GridState.Playing) return;
                TabsManager.Instance.ShowStartScreen();
            });

            clearButton.Initialize(() =>
            {
                if (GameManager.Instance.CurrentGridState > GridState.Playing) return;
                SwitchMoveType(MoveType.Clear);
            });
            flagButton.Initialize(() =>
            {
                if (GameManager.Instance.CurrentGridState > GridState.Playing) return;
                SwitchMoveType(MoveType.Flag);
            });

            currentWinStreakText.text = PlayerData.GetCurrentWinStreak(GameManager.Instance.CurrentDifficulty).ToString();
            GameManager.Instance.OnGridStateChanged += OnGridStateChanged_Event;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentGridState > GridState.Playing) return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    faceImage.sprite = surprisedFaceSprite;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    faceImage.sprite = happyFaceSprite;
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                faceImage.sprite = surprisedFaceSprite;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                faceImage.sprite = happyFaceSprite;
            }

            minesCounterText.text = GameManager.Instance.RemainingMines.ToString();
            TimeSpan timer = TimeSpan.FromSeconds(GameManager.Instance.CurrentTime);
            timerText.text = string.Format("{0:D2}:{1:D2}", timer.Minutes, timer.Seconds);
        }

        public void OnGridStateChanged_Event(GridState gridState, bool isNewScore)
        {
            switch (gridState)
            {
                case GridState.NotStarted:
                    OnNewGame();
                    faceImage.sprite = happyFaceSprite;
                    break;
                case GridState.Cleared:
                    OnWin(isNewScore);
                    faceImage.sprite = coolFaceSprite;
                    break;
                case GridState.Boom:
                    OnLose();
                    faceImage.sprite = deadFaceSprite;
                    break;
                default:
                    break;
            }

            currentWinStreakText.text = PlayerData.GetCurrentWinStreak(GameManager.Instance.CurrentDifficulty).ToString();
        }

        private void OnNewGame()
        {
            clearButton.OnSelect();
            flagButton.OnDeselect();

            confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            poof.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            endGameUi.Hide();

#if !UNITY_EDITOR
            mobileModeButtons.SetActive(Application.isMobilePlatform);
#endif
        }

        private async void OnWin(bool isNewScore)
        {
            confetti.Play();
            SoundManager.Instance.PlaySound(ClipName.ConfettiPop);
            SoundManager.Instance.PlaySound(ClipName.ConfettiBeep);

            //AudioManager.Instance.PlayAudio(ClipName.ConfettiPop);
            //AudioManager.Instance.PlayAudio(ClipName.ConfettiBeep);
            string description = "";
            if (isNewScore)
            {
                TimeSpan time = TimeSpan.FromSeconds(PlayerData.GetBestScore(GameManager.Instance.CurrentDifficulty));
                description = "Hmmm?!\n" + string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            }

            //await Awaitable.WaitForSecondsAsync(1);
            await Task.Delay(1000);

            endGameUi.Show("Board Cleared!", description);
        }

        private async void OnLose()
        {
            if (!PlayerData.IsSpeedRunMode) poof.Play();
            SoundManager.Instance.PlaySound(ClipName.Poof);
            await Task.Delay(500);
            //await Awaitable.WaitForSecondsAsync(0.5f);
            endGameUi.Show("Mine?!");
        }

        private void SwitchMoveType(MoveType requestedMoveType)
        {
            if (requestedMoveType == MoveType.Clear && GameManager.Instance.CurrentMoveType != MoveType.Clear)
            {
                GameManager.Instance.CurrentMoveType = MoveType.Clear;
                clearButton.OnSelect();
                flagButton.OnDeselect();
#if !UNITY_EDITOR && !PLATFORM_STANDALONE_WIN
                HapticFeedback.LightFeedback();
#endif
            }
            else if (requestedMoveType == MoveType.Flag && GameManager.Instance.CurrentMoveType != MoveType.Flag)
            {
                GameManager.Instance.CurrentMoveType = MoveType.Flag;
                clearButton.OnDeselect();
                flagButton.OnSelect();
#if !UNITY_EDITOR && !PLATFORM_STANDALONE_WIN
                HapticFeedback.LightFeedback();
#endif
            }
        }
    }
}
