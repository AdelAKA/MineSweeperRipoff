using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if Unity_ANDROID
using CandyCoded.HapticFeedback;
#endif

namespace MineSweeperRipeoff
{
    public class BoardTab : Tab
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
        [SerializeField] EndGamePopup endGamePopup;

        [Header("Particle Effects")]
        [SerializeField] ParticleSystem confetti;
        [SerializeField] ParticleSystem poof;

        private void Start()
        {
            resetButton.onClick.AddListener(() =>
            {
                if (GameplayManager.Instance.CurrentGridState > GridState.Playing) return;
                GameplayManager.Instance.RequestRestart();
            });
            backButton.onClick.AddListener(() =>
            {
                if (GameplayManager.Instance.CurrentGridState > GridState.Playing) return;
                GameplayManager.Instance.RequestFinish();
            });

            clearButton.Initialize(() =>
            {
                if (GameplayManager.Instance.CurrentGridState > GridState.Playing) return;
                SwitchMoveType(MoveType.Clear);
            });
            flagButton.Initialize(() =>
            {
                if (GameplayManager.Instance.CurrentGridState > GridState.Playing) return;
                SwitchMoveType(MoveType.Flag);
            });

            GameplayManager.Instance.OnGridStateChanged += OnGridStateChanged_Event;
        }

        private void Update()
        {
            if (GameplayManager.Instance.CurrentGridState > GridState.Playing) return;

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

            minesCounterText.text = GameplayManager.Instance.RemainingMines.ToString();
            TimeSpan timer = TimeSpan.FromSeconds(GameplayManager.Instance.CurrentTime);
            timerText.text = string.Format("{0:D2}:{1:D2}", timer.Minutes, timer.Seconds);
        }

        public override void Show()
        {
            OnNewGame();
            base.Show();
            SoundManager.Instance.PlayMusic(ClipName.Playing);
        }

        private void OnNewGame()
        {
            clearButton.OnSelect();
            flagButton.OnDeselect();

            currentWinStreakText.text = PlayerData.GetCurrentWinStreak(GameplayManager.Instance.CurrentDifficulty, GameplayManager.Instance.CurrentGameMode).ToString();
            
            confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            poof.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            endGamePopup.Hide();

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
                TimeSpan time = TimeSpan.FromSeconds(PlayerData.GetBestScore(GameplayManager.Instance.CurrentDifficulty, GameplayManager.Instance.CurrentGameMode));
                description = "Hmmm?!\n" + string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            }

            await Awaitable.WaitForSecondsAsync(1);
            //await Task.Delay(1000);

            endGamePopup.Show("Board Cleared!", description);
        }

        private async void OnLose()
        {
            if (!PlayerData.IsSpeedRunMode) poof.Play();
            SoundManager.Instance.PlaySound(ClipName.Poof);
            //await Task.Delay(500);
            await Awaitable.WaitForSecondsAsync(0.5f);
            endGamePopup.Show("Mine?!");
        }

        private void SwitchMoveType(MoveType requestedMoveType)
        {
            if (requestedMoveType == MoveType.Clear && GameplayManager.Instance.CurrentMoveType != MoveType.Clear)
            {
                GameplayManager.Instance.CurrentMoveType = MoveType.Clear;
                clearButton.OnSelect();
                flagButton.OnDeselect();
#if UNITY_ANDROID
                HapticFeedback.LightFeedback();
#endif
            }
            else if (requestedMoveType == MoveType.Flag && GameplayManager.Instance.CurrentMoveType != MoveType.Flag)
            {
                GameplayManager.Instance.CurrentMoveType = MoveType.Flag;
                clearButton.OnDeselect();
                flagButton.OnSelect();
#if UNITY_ANDROID
                HapticFeedback.LightFeedback();
#endif
            }
        }

        public void OnGridStateChanged_Event(GridState gridState, bool isNewScore)
        {
            switch (gridState)
            {
                case GridState.NotStarted:
                    OnNewGame();
                    faceImage.sprite = happyFaceSprite;
                    break;
                case GridState.Playing:
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

            currentWinStreakText.text = PlayerData.GetCurrentWinStreak(GameplayManager.Instance.CurrentDifficulty, GameplayManager.Instance.CurrentGameMode).ToString();
        }
    }
}
