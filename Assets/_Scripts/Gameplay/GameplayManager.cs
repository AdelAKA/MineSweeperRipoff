using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MineSweeperRipeoff
{
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance { get; set; }

        [Header("References")]
        [SerializeField] FieldGrid fieldGrid;
        [SerializeField] Grid CurrentGrid;

        [Header("Particles")]
        [SerializeField] ParticleEffectPool clearParticleEffectPool;
        [SerializeField] ParticleEffectPool BombParticleEffectPool;

        [Header("Settings")]
        [SerializeField] GameSettings gameSettings;

        private DifficultyLevel currentDifficulty;
        private float timer;

        private string SavePath => $"{CurrentGameMode}_{CurrentDifficulty}";

        public UnityAction<GridState, bool> OnGridStateChanged;

        public MoveType CurrentMoveType { get; set; }

        public GridState CurrentGridState => CurrentGrid.CurrentState;
        public DifficultyLevel CurrentDifficulty => currentDifficulty;
        public GameMode CurrentGameMode => PlayerData.CurrentGameMode;

        public int RemainingMines => CurrentGrid.RemainingUnflaggedMines;
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

            Debug.Log(Application.persistentDataPath);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartNewGame();
            }
#endif
            if (CurrentGrid.CurrentState == GridState.Playing)
                timer += Time.deltaTime;
        }

        public async void RequestGame(DifficultyLevel level)
        {
            await LoadingScreen.Instance.Show();
            TabsManager.Instance.ShowTab(TabName.Board);

            currentDifficulty = level;
            if (GridDataSaver.SavedGridExists(SavePath))
            {
                Debug.Log("Loading Grid");
                LoadExistingGrid();
            }
            else
            {
                Debug.Log("No Saved Grid");
                StartNewGame();
            }

            LoadingScreen.Instance.Hide();
        }

        private void StartNewGame()
        {
            (var gridSize,var minesCount) = gameSettings.GetOptionsForDifficulty(CurrentDifficulty);
            CurrentGrid = new Grid(gridSize, minesCount, CurrentGameMode, PlayerData.IsSpeedRunMode);
            UpdateParameters();

            GridDataSaver.DeleteSave(SavePath);
        }

        private void LoadExistingGrid()
        {
            GridSaveData gridSaveData = GridDataSaver.TryLoadGrid(SavePath);
            CurrentGrid = new Grid(gridSaveData, CurrentGameMode, PlayerData.IsSpeedRunMode);
            UpdateParameters();
            timer = gridSaveData.timer;

            GridDataSaver.DeleteSave(SavePath);
        }

        private void UpdateParameters()
        {
            timer = 0;
            CurrentGrid.OnGridStateChanged.AddListener(Event_OnGridStateChanged);
            fieldGrid.Initialize(CurrentGrid);
            //inGameUi.OnNewGame();
            CurrentMoveType = MoveType.Clear;
            SoundManager.Instance.PlayMusic(ClipName.Playing);
            OnGridStateChanged?.Invoke(CurrentGridState, false);
        }

        public async void RequestRestart()
        {
            await LoadingScreen.Instance.Show();

            StartNewGame();

            LoadingScreen.Instance.Hide();
        }

        public async void RequestFinish()
        {
            if (!CurrentGrid.IsFirstMove)
            {
                Debug.Log("Saving Grid");
                GridSaveData gridSaveData = CurrentGrid.GetSaveData();
                gridSaveData.timer = timer;
                GridDataSaver.SaveGrid(SavePath, gridSaveData);
            }

            await LoadingScreen.Instance.Show();

            TabsManager.Instance.ShowTab(TabName.StartScreen);
            LoadingScreen.Instance.Hide();
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
                if (!PlayerData.IsSpeedRunMode) await Awaitable.WaitForSecondsAsync(0.5f);
                //if (!PlayerData.IsSpeedRunMode) await Task.Delay(500);

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