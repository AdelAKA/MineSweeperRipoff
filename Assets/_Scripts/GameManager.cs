using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; set; }

        [SerializeField] FieldGrid fieldGrid;
        [SerializeField] Grid CurrentGrid;
        [SerializeField] ParticleEffectPool clearParticleEffectPool;
        [SerializeField] ParticleEffectPool BombParticleEffectPool;

        private Vector2Int easySize = new Vector2Int(8, 8);
        private int easymines = 8;

        private Vector2Int mediumSize = new Vector2Int(10, 9);
        private int mediumMines = 15;

        private Vector2Int hardSize = new Vector2Int(14, 9);
        private int hardMines = 24;

        private DifficultyLevel currentDifficulty;
        private float timer;

        public UnityAction<GridState, bool> OnGridStateChanged;
        public UnityAction OnGameModeChanged;

        public MoveType CurrentMoveType { get; set; }

        public GridState CurrentGridState => CurrentGrid.CurrentState;
        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public GameMode CurrentGameMode => PlayerData.CurrentGameMode;

        public int RemainingMines => CurrentGrid.RemainingMines;
        public float CurrentTime => timer;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartNewGame(currentDifficulty);
            }
#endif
            if (CurrentGrid.CurrentState == GridState.Playing)
                timer += Time.deltaTime;
        }

        public void StartNewGame(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Easy:
                    CurrentGrid.Initialize(easySize, easymines, CurrentGameMode);
                    break;
                case DifficultyLevel.Medium:
                    CurrentGrid.Initialize(mediumSize, mediumMines, CurrentGameMode);
                    break;
                case DifficultyLevel.Hard:
                    CurrentGrid.Initialize(hardSize, hardMines, CurrentGameMode);
                    break;
                default:
                    CurrentGrid.Initialize(mediumSize, mediumMines, CurrentGameMode);
                    break;
            }
            timer = 0;
            currentDifficulty = level;
            CurrentGrid.OnGridStateChanged.AddListener(Event_OnGridStateChanged);
            fieldGrid.Initialize(CurrentGrid);
            //inGameUi.OnNewGame();
            CurrentMoveType = MoveType.Clear;
            SoundManager.Instance.PlayMusic(ClipName.Playing);
            OnGridStateChanged?.Invoke(CurrentGridState, false);
        }

        public void RequestRestart()
        {
            StartNewGame(currentDifficulty);
        }

        public void UpdateCellFlagState(Vector2Int targetCoordinates)
        {
            CurrentGrid.UpdateCellFlagState(targetCoordinates);
        }

        public void ReavealCell(Vector2Int targetCoordinates)
        {
            CurrentGrid.MakeMove(targetCoordinates);
        }

        public void RequestClearParticle(FieldCell fieldCell)
        {
            if (PlayerData.IsSpeedRunMode) return;

            PooledParticle particle = clearParticleEffectPool.objectPool.Get();
            particle.transform.position = fieldCell.transform.position;
        }

        public void RequestBombParticle(FieldCell fieldCell)
        {
            if (PlayerData.IsSpeedRunMode) return;

            PooledParticle particle = BombParticleEffectPool.objectPool.Get();
            particle.transform.position = fieldCell.transform.position;
        }

        public void SwitchGameSpeedMode()
        {
            PlayerData.IsSpeedRunMode = !PlayerData.IsSpeedRunMode;
        }

        public void SwitchGameMode()
        {
            if (PlayerData.CurrentGameMode == GameMode.Chance)
                PlayerData.CurrentGameMode = GameMode.NoChance;
            else
                PlayerData.CurrentGameMode = GameMode.Chance;
            OnGameModeChanged?.Invoke();
        }

        private void UpdateScore()
        {
            PlayerData.SetCurrentWinStreak(currentDifficulty, CurrentGameMode, PlayerData.GetCurrentWinStreak(currentDifficulty, CurrentGameMode) + 1);

            if (timer < PlayerData.GetBestScore(currentDifficulty, CurrentGameMode) || PlayerData.GetBestScore(currentDifficulty, CurrentGameMode) == 0)
            {
                PlayerData.SetBestScore(currentDifficulty, CurrentGameMode, timer);
                Debug.Log($"New Player Score {timer}");
                OnGridStateChanged?.Invoke(GridState.Cleared, true);
            }
            else
            {
                OnGridStateChanged?.Invoke(GridState.Cleared, false);
            }
        }

        private void ResetStreak()
        {
            PlayerData.SetCurrentWinStreak(CurrentDifficulty, CurrentGameMode, 0);
        }

        private void FlagTheMines() => CurrentGrid.FlagTheMines();
        private async Task RevealTheMines() => await CurrentGrid.RevealTheMines();

        public async void Event_OnGridStateChanged(GridState gridState)
        {
            if (gridState == GridState.Boom)
            {
                //if (!PlayerData.IsSpeedRunMode) await Awaitable.WaitForSecondsAsync(0.5f);
                if (!PlayerData.IsSpeedRunMode) await Task.Delay(500);

                await RevealTheMines();
                ResetStreak();
                OnGridStateChanged?.Invoke(GridState.Boom, false);
                SoundManager.Instance.PlayMusic(ClipName.Losing);
            }
            else if (gridState == GridState.Cleared)
            {
                FlagTheMines();
                SoundManager.Instance.PlayMusic(ClipName.Wining);
                UpdateScore();
            }
        }
    }
}