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
        public int RemainingMines { get; protected set; }

        protected GridGenerator gridGenerator = new GridGenerator();
        protected bool isFirstMove;
        protected Cell[,] cells;
        protected int numberOfRevealedCells;
        private List<Vector2Int> availableCellsForMines;

        protected List<Vector2Int> DirectionsList = new List<Vector2Int>()
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

        public virtual bool ShouldDelay => !PlayerData.IsSpeedRunMode;
        public Cell GetCell(Vector2Int targetCoordinates) => cells[targetCoordinates.x, targetCoordinates.y];
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

        public Grid()
        {
        }

        public Grid(Grid copy)
        {
            this.gridSize = copy.gridSize;
            this.numberOfMines = copy.numberOfMines;

            ResetGrid();

            cells = copy.GetCopyOfCells();
        }

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
            isFirstMove = true;
            CurrentState = GridState.NotStarted;
            numberOfRevealedCells = 0;
            OnGridStateChanged.RemoveAllListeners();
            RemainingMines = numberOfMines;
            Debug.Log(RemainingMines);
        }

        public virtual async void Initialize(Vector2Int? gridSizeTraget, int? numberOfMinesTraget)
        {
            if (gridSizeTraget.HasValue) this.gridSize = gridSizeTraget.Value;
            if (numberOfMinesTraget.HasValue) this.numberOfMines = numberOfMinesTraget.Value;

            //isFirstMove = true;
            //CurrentState = GridState.NotStarted;
            //numberOfRevealedCells = 0;
            //OnGridStateChanged.RemoveAllListeners();
            //RemainingMines = numberOfMines;

            ResetGrid();

            Vector2Int startCell;
            (cells, startCell) = await gridGenerator.GenerateNoChanceGrid(gridSize, numberOfMines);

            {
                //cells = new Cell[gridSize.x, gridSize.y];

                //if (isDebug) Debug.Log($"{cells.Length}");

                //for (int i = 0; i < gridSize.x; i++)
                //{
                //    for (int j = 0; j < gridSize.y; j++)
                //    {
                //        if (isDebug) Debug.Log($"{i}, {j}, {gridSize}");
                //        //Debug.Log($"{i < gridSize.x}");
                //        cells[i, j] = new Cell(new Vector2Int(i, j));
                //    }
                //}

                //availableCellsForMines = new List<Vector2Int>();
                //for (int i = 0; i < gridSize.x; i++)
                //{
                //    for (int j = 0; j < gridSize.y; j++)
                //    {
                //        availableCellsForMines.Add(new Vector2Int(i, j));
                //    }
                //}

                //for (int i = 0; i < numberOfMines; i++)
                //{
                //    int randomIndex = UnityEngine.Random.Range(0, availableCellsForMines.Count);

                //    SetMineAndAddCountToSurroundingCells(availableCellsForMines[randomIndex]);

                //    availableCellsForMines.RemoveAt(randomIndex);
                //}
            }
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
            if (cells[coordinates.x, coordinates.y].isFlagged) RemainingMines--;
            else RemainingMines++;
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

        private void MakeFirstMove(Vector2Int coordinates)
        {
            if (isDebug) Debug.Log("Is First Move");
            SwitchMinesAroundFirstMove(coordinates);
            TryRevealCell(coordinates);
            CheckGameState();
            CurrentState = GridState.Playing;
        }

        public void MakeMove(Vector2Int coordinates)
        {
            if (cells[coordinates.x, coordinates.y].isFlagged) return;
            if (isFirstMove)
            {
                MakeFirstMove(coordinates);
                isFirstMove = false;
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

            //if (!PlayerData.IsSpeedRunMode) await Awaitable.WaitForSecondsAsync(0.05f);
            if (ShouldDelay) await Task.Delay(50);

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
            float TimeToWait = 0.5f;
            int sequenceCount = 1;
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].cellType == CellType.Mine && !cells[i, j].isRevealed)
                    {
                        //if (!PlayerData.IsSpeedRunMode) await Awaitable.WaitForSecondsAsync(TimeToWait);
                        if (ShouldDelay) await Task.Delay((int)(TimeToWait * 1000));

                        TimeToWait *= 0.8f;
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