using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

namespace MineSweeperRipeoff
{
    [Serializable]
    public class Grid
    {
        [SerializeField] protected bool isDebug;
        public Vector2Int gridSize;
        public int numberOfMines;

        public UnityEvent<GridState> OnGridStateChanged = new UnityEvent<GridState>();
        private GridState _currentState;
        public GridState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnGridStateChanged?.Invoke(_currentState);
            }
        }
        private GameMode _currentGameMode;
        private bool _shouldDelay;

        // Properties
        public int RemainingUnflaggedMines { get; protected set; }
        public bool IsFirstMove { get; protected set; }

        protected GridGenerator gridGenerator = new GridGenerator();
        protected Cell[,] cells;
        protected int numberOfRevealedCells;
        private List<Vector2Int> availableCellsForMines;

        protected static readonly float REVEAL_DELAY = 0.05f;
        protected static readonly float MINE_REVEAL_START_DELAY = 0.5f;
        protected static readonly float MINE_REVEAL_DELAY_MULTIPLIER = 0.8f;

        protected static readonly List<Vector2Int> DirectionsList = new List<Vector2Int>()
        {
            new Vector2Int( 0,  0),
            new Vector2Int( 0,  1),
            new Vector2Int( 0, -1),
            new Vector2Int( 1,  0),
            new Vector2Int( 1,  1),
            new Vector2Int( 1, -1),
            new Vector2Int(-1,  0),
            new Vector2Int(-1,  1),
            new Vector2Int(-1, -1),
        };

        public Cell GetCell(Vector2Int targetCoordinates) => cells[targetCoordinates.x, targetCoordinates.y];

        public GridSaveData GetSaveData()
        {
            List<Cell> cellsToSave = new List<Cell>(gridSize.x * gridSize.y);
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    cellsToSave.Add(cells[i, j]);
                }
            }
            GridSaveData dataToSave = new GridSaveData()
            {
                gridSize = this.gridSize,
                numberOfMines = this.numberOfMines,
                remainingMines = this.RemainingUnflaggedMines,
                numberOfRevealedCells = this.numberOfRevealedCells,
                cells = cellsToSave
            };
            return dataToSave;
        }

        public Cell[,] GetCopyOfCells()
        {
            Cell[,] cellsCopy = new Cell[gridSize.x, gridSize.y];
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    //revealedGrid[i, j] = new Cell(cells[i, j]);
                    cellsCopy[i, j] = new Cell(cells[i, j]);
                }
            }
            return cellsCopy;
        }

        public Grid() { }

        // Used for generating a grid through the grid generator
        public Grid(Vector2Int? gridSizeTraget, int? numberOfMinesTarget, GameMode gameMode, bool isSpeedRunMode)
        {
            if (gridSizeTraget.HasValue) this.gridSize = gridSizeTraget.Value;
            if (numberOfMinesTarget.HasValue) this.numberOfMines = numberOfMinesTarget.Value;
            this._currentGameMode = gameMode;
            this._shouldDelay = !isSpeedRunMode;

            ResetGrid();

            Vector2Int startCell;
            if (gameMode == GameMode.Chance)
            {
                cells = gridGenerator.GenerateRandomGrid(gridSize, numberOfMines);
            }
            else // No Chance Grid
            {
                var result = gridGenerator.GenerateNoChanceGrid(gridSize, numberOfMines);
                (cells, startCell) = (result.Result.Item1, result.Result.Item2);
                cells[startCell.x, startCell.y].MarkAsForceStartCell();
            }
        }

        // Used for loading a grid from saved data
        public Grid(GridSaveData loadedGridData, GameMode gameMode, bool isSpeedRunMode)
        {
            this.gridSize = loadedGridData.gridSize;
            this.numberOfMines = loadedGridData.numberOfMines;
            this.RemainingUnflaggedMines = loadedGridData.remainingMines;
            this.IsFirstMove = false;
            this.CurrentState = GridState.Playing;
            this.numberOfRevealedCells = loadedGridData.numberOfRevealedCells;
            this._currentGameMode = gameMode;
            this._shouldDelay = !isSpeedRunMode;
            OnGridStateChanged.RemoveAllListeners();

            //ResetGrid();

            cells = new Cell[gridSize.x, gridSize.y];
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    //revealedGrid[i, j] = new Cell(cells[i, j]);
                    cells[i, j] = new Cell(loadedGridData.cells[i * gridSize.y + j]);
                }
            }
        }

        // Used to copy another grid
        public Grid(Cell[,] cellsCopy, int numberOfMines)
        {
            this.gridSize = new Vector2Int(cellsCopy.GetLength(0), cellsCopy.GetLength(1));
            this.numberOfMines = numberOfMines;

            ResetGrid();

            cells = new Cell[gridSize.x, gridSize.y];
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    //revealedGrid[i, j] = new Cell(cells[i, j]);
                    cells[i, j] = new Cell(cellsCopy[i, j]);
                }
            }
        }

        private void ResetGrid()
        {
            IsFirstMove = true;
            CurrentState = GridState.NotStarted;
            numberOfRevealedCells = 0;
            OnGridStateChanged.RemoveAllListeners();
            RemainingUnflaggedMines = numberOfMines;
        }

        protected bool IsWithinRange(Vector2Int coordinates)
        {
            return coordinates.x >= 0
                    && coordinates.x < gridSize.x
                    && coordinates.y >= 0
                    && coordinates.y < gridSize.y;
        }

        protected void SetMineAndAddCountToSurroundingCells(Vector2Int coordinates)
        {
            cells[coordinates.x, coordinates.y].SetAsMine();

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates))
                    cells[targetCoordinates.x, targetCoordinates.y].IncreaseCount();
            }
        }

        private void ClearMineAndSubtractCountFromSurroundingCells(Vector2Int coordinates)
        {
            cells[coordinates.x, coordinates.y].ClearMine();

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates))
                    cells[targetCoordinates.x, targetCoordinates.y].DecreaseCount();
            }
        }

        public void CheckGameState()
        {
            if ((numberOfRevealedCells + numberOfMines) == gridSize.x * gridSize.y)
                CurrentState = GridState.Cleared;
        }

        public void UpdateCellFlagState(Vector2Int coordinates)
        {
            if (cells[coordinates.x, coordinates.y].isRevealed)
            {
                if (isDebug) Debug.Log("Already pressed");
                return;
            }
            cells[coordinates.x, coordinates.y].SetFlagState(!cells[coordinates.x, coordinates.y].isFlagged);
            if (cells[coordinates.x, coordinates.y].isFlagged) RemainingUnflaggedMines--;
            else RemainingUnflaggedMines++;
        }

        public void SwitchMinesAroundFirstMove(Vector2Int coordinates)
        {
            availableCellsForMines = new List<Vector2Int>();
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].cellType != CellType.Mine)
                        availableCellsForMines.Add(new Vector2Int(i, j));
                }
            }
            if (availableCellsForMines.Contains(coordinates))
            {
                availableCellsForMines.Remove(coordinates);
                if (isDebug) Debug.Log($"Remove Coordinates {coordinates}");
            }
            foreach (var direction in DirectionsList)
            {
                if (availableCellsForMines.Contains(coordinates + direction))
                {
                    availableCellsForMines.Remove(coordinates + direction);
                    if (isDebug) Debug.Log($"Remove Coordinates {coordinates + direction}");
                }
            }

            if (cells[coordinates.x, coordinates.y].cellType == CellType.Mine)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableCellsForMines.Count);
                //cells[coordinates.x, coordinates.y].ClearMine();
                ClearMineAndSubtractCountFromSurroundingCells(coordinates);
                SetMineAndAddCountToSurroundingCells(availableCellsForMines[randomIndex]);

                if (isDebug) Debug.Log($"Mine Moved to {availableCellsForMines[randomIndex]}");
                availableCellsForMines.RemoveAt(randomIndex);
            }

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates)
                    && cells[targetCoordinates.x, targetCoordinates.y].cellType == CellType.Mine)
                {
                    int randomIndex = UnityEngine.Random.Range(0, availableCellsForMines.Count);
                    //cells[targetCoordinates.x, targetCoordinates.y].ClearMine();
                    ClearMineAndSubtractCountFromSurroundingCells(targetCoordinates);
                    SetMineAndAddCountToSurroundingCells(availableCellsForMines[randomIndex]);

                    if (isDebug) Debug.Log($"Mine Moved to {availableCellsForMines[randomIndex]}");
                    availableCellsForMines.RemoveAt(randomIndex);
                }
            }
        }

        private void TryMakeFirstMove(Vector2Int coordinates)
        {
            if (_currentGameMode == GameMode.Chance)
            {
                if (isDebug) Debug.Log("Is First Move");
                SwitchMinesAroundFirstMove(coordinates);
                TryRevealCell(coordinates);
                CheckGameState();
                CurrentState = GridState.Playing;
                IsFirstMove = false;
            }
            else // No Chance Grid
            {
                if (cells[coordinates.x, coordinates.y].isForcePress)
                {
                    TryRevealCell(coordinates);
                    CheckGameState();
                    CurrentState = GridState.Playing;
                    IsFirstMove = false;
                }
            }
        }

        public void MakeMove(Vector2Int coordinates)
        {
            if (cells[coordinates.x, coordinates.y].isFlagged) return;
            if (IsFirstMove)
            {
                TryMakeFirstMove(coordinates);
                return;
            }
            bool isValid = true;
            if (cells[coordinates.x, coordinates.y].isRevealed
                && cells[coordinates.x, coordinates.y].number > 0)
            {
                isValid = TryRevealAround(coordinates);
                //if (isDebug) Debug.Log("Already pressed");
                //return;
            }
            else if (!cells[coordinates.x, coordinates.y].isRevealed)
            {
                isValid = TryRevealCell(coordinates);
            }

            if (!isValid)
            {
                EndGame();
                return;
            }
        }

        private bool HasEnoughMarks(Vector2Int coordinates)
        {
            int number = cells[coordinates.x, coordinates.y].number;
            int marksCount = 0;
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates)
                    && cells[targetCoordinates.x, targetCoordinates.y].isFlagged)
                {
                    marksCount++;
                    if (marksCount == number) return true;
                }
            }
            return false;
        }

        private bool TryRevealAround(Vector2Int coordinates)
        {
            if (HasEnoughMarks(coordinates))
            {
                foreach (var direction in DirectionsList)
                {
                    Vector2Int targetCoordinates = coordinates + direction;
                    if (IsWithinRange(targetCoordinates)
                        && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                        && !cells[targetCoordinates.x, targetCoordinates.y].isFlagged)
                    {
                        bool isSafe = TryRevealCell(targetCoordinates);
                        if (!isSafe) return false;
                    }
                }
            }
            else
            {
                Debug.Log("Not Enough Marks");
            }
            return true;
        }

        protected bool TryRevealCell(Vector2Int coordinates)
        {
            if (isDebug) Debug.Log($"try reveal {coordinates}");
            if (cells[coordinates.x, coordinates.y].cellType == CellType.Empty)
            {
                RevealCellsRecursivley(coordinates, 1);
                if (isDebug) Debug.Log("Finish Recursion");
            }

            if (cells[coordinates.x, coordinates.y].cellType == CellType.Mine)
            {
                cells[coordinates.x, coordinates.y].isLosingCell = true;
                RevealCell(coordinates, 1);
                if (isDebug) Debug.Log("you lost");
                return false;
            }

            if (cells[coordinates.x, coordinates.y].cellType == CellType.Number)
            {
                RevealCell(coordinates, 1);
            }
            return true;
        }

        protected virtual void RevealCell(Vector2Int coordinates, int sequenceCount)
        {
            cells[coordinates.x, coordinates.y].RevealCell(sequenceCount);
            if (cells[coordinates.x, coordinates.y].cellType != CellType.Mine)
                numberOfRevealedCells++;
            CheckGameState();
        }

        private async void RevealCellsRecursivley(Vector2Int coordinates, int sequenceCount)
        {
            if (cells[coordinates.x, coordinates.y].cellType == CellType.Number)
            {
                RevealCell(coordinates, sequenceCount);
                return;
            }
            else if (cells[coordinates.x, coordinates.y].cellType == CellType.Mine)
            {
                if (isDebug) Debug.Log("Error");
                return;
            }

            RevealCell(coordinates, sequenceCount);

            if (_shouldDelay) await Awaitable.WaitForSecondsAsync(REVEAL_DELAY);
            //if (ShouldDelay) await Task.Delay(50);

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates)
                    && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed)
                {
                    if (cells[targetCoordinates.x, targetCoordinates.y].isFlagged) UpdateCellFlagState(targetCoordinates);
                    RevealCellsRecursivley(targetCoordinates, sequenceCount++);
                }
            }
        }

        public void FlagTheMines()
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].cellType == CellType.Mine)
                    {
                        cells[i, j].SetFlagState(true);
                    }
                }
            }
        }

        public async Task RevealTheMines()
        {
            float TimeToWait = MINE_REVEAL_START_DELAY;
            int sequenceCount = 1;
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].cellType == CellType.Mine && !cells[i, j].isRevealed)
                    {
                        if (_shouldDelay) await Awaitable.WaitForSecondsAsync(TimeToWait);
                        //if (ShouldDelay) await Task.Delay((int)(TimeToWait * 1000));

                        TimeToWait *= MINE_REVEAL_DELAY_MULTIPLIER;
                        if (cells[i, j].isFlagged) cells[i, j].SetFlagState(false);
                        cells[i, j].RevealCell(sequenceCount++);
                    }
                }
            }
        }

        public void EndGame()
        {
            CurrentState = GridState.Boom;
        }
    }
}