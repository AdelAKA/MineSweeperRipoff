using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeperRipeoff
{
    [Serializable]
    public class GridSolver : Grid
    {
        private bool _usePredefinedGrid;
        private float _delay;

        public GridSolver(float delay = 1, bool usePredefinedGrid = false) : base()
        {
            this._usePredefinedGrid = usePredefinedGrid;
            this._delay = delay;
        }

        public GridSolver(Grid copy, float delay = 1, bool isDebug = false) : base(copy)
        {
            this.isDebug = isDebug;
            this._delay = delay;
            trackedNumbers = new int[gridSize.x, gridSize.y];
            predictedMines = new bool[gridSize.x, gridSize.y];
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    //revealedGrid[i, j] = new Cell(cells[i, j]);
                    trackedNumbers[i, j] = cells[i, j].number;
                    if (isDebug) Debug.Log($"{i} {j} {trackedNumbers[i, j]}");
                }
            }
        }

        public override bool ShouldDelay => false;
        private int[,] trackedNumbers;
        private bool[,] predictedMines;
        private HashSet<Vector2Int> monitoredCells = new HashSet<Vector2Int>();

        public override void Initialize(Vector2Int? gridSizeTraget, int? numberOfMinesTraget)
        {
            if (!_usePredefinedGrid) base.Initialize(gridSizeTraget, numberOfMinesTraget);
            else
            {
                this.gridSize = new Vector2Int(8, 8);
                #region OldGrids
                //List<Vector2Int> minesList = new List<Vector2Int>()
                //{
                //    new Vector2Int(0, 0),
                //    new Vector2Int(2, 0),
                //    new Vector2Int(0, 6),
                //    new Vector2Int(2, 6),
                //    new Vector2Int(4, 2),
                //    new Vector2Int(4, 3),
                //    new Vector2Int(4, 4),
                //    new Vector2Int(5, 0),
                //    new Vector2Int(6, 0),
                //};

                //List<Vector2Int> minesList = new List<Vector2Int>()
                //{
                //    new Vector2Int(0, 2),
                //    new Vector2Int(1, 4),
                //    new Vector2Int(3, 4),
                //    new Vector2Int(5, 0),
                //    new Vector2Int(5, 3),
                //    new Vector2Int(5, 7),
                //    new Vector2Int(0, 6),
                //    new Vector2Int(1, 6),
                //    new Vector2Int(2, 6),
                //};

                //List<Vector2Int> minesList = new List<Vector2Int>()
                //{
                //    new Vector2Int(2, 2),
                //    new Vector2Int(4, 1),
                //    new Vector2Int(4, 3),
                //    new Vector2Int(2, 5),
                //    new Vector2Int(2, 6),
                //    new Vector2Int(2, 7),
                //    new Vector2Int(3, 5),
                //    new Vector2Int(3, 6),
                //    new Vector2Int(3, 7),
                //};
                //List<Vector2Int> minesList = new List<Vector2Int>()
                //{
                //    new Vector2Int(4, 1),
                //    new Vector2Int(4, 2),
                //    new Vector2Int(2, 2),
                //    new Vector2Int(0, 5),
                //    new Vector2Int(1, 5),
                //    new Vector2Int(2, 5)
                //};
                #endregion
                List<Vector2Int> minesList = new List<Vector2Int>()
                {
                    new Vector2Int(2, 0),
                    new Vector2Int(0, 2),
                    new Vector2Int(4, 4),
                    new Vector2Int(0, 5),
                    new Vector2Int(1, 5),
                    new Vector2Int(2, 5)
                };
                numberOfMines = minesList.Count;

                isFirstMove = true;
                CurrentState = GridState.NotStarted;
                numberOfRevealedCells = 0;
                OnGridStateChanged.RemoveAllListeners();
                RemainingMines = minesList.Count;

                cells = new Cell[gridSize.x, gridSize.y];

                if (isDebug) Debug.Log($"{cells.Length}");

                for (int i = 0; i < gridSize.x; i++)
                {
                    for (int j = 0; j < gridSize.y; j++)
                    {
                        if (isDebug) Debug.Log($"{i}, {j}, {gridSize}");
                        //Debug.Log($"{i < gridSize.x}");
                        cells[i, j] = new Cell(new Vector2Int(i, j));
                    }
                }

                for (int i = 0; i < minesList.Count; i++)
                {
                    SetMineAndAddCountToSurroundingCells(minesList[i]);
                }
            }
        }

        protected override void RevealCell(Vector2Int coordinates, int sequenceCount)
        {
            base.RevealCell(coordinates, sequenceCount);
            if (cells[coordinates.x, coordinates.y].cellType == CellType.Number
                && cells[coordinates.x, coordinates.y].number > 0
                )
            {
                monitoredCells.Add(coordinates);
                if (isDebug) Debug.Log($"targeting {coordinates}," +
                    $" num {cells[coordinates.x, coordinates.y].number}," +
                    $" tracked num {trackedNumbers[coordinates.x, coordinates.y]}");
            }
        }

        private void SubtractCountFromSurroundingCells(Vector2Int coordinates)
        {
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates))
                {
                    trackedNumbers[targetCoordinates.x, targetCoordinates.y]--;
                    if (isDebug) Debug.Log($"subtracting from {targetCoordinates} to {trackedNumbers[targetCoordinates.x, targetCoordinates.y]}");
                }
            }
        }

        List<Vector2Int> GetSurroundingUnPredictedCells(Vector2Int targetCell)
        {
            List<Vector2Int> unPredictedCells = new List<Vector2Int>();
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = targetCell + direction;
                if (IsWithinRange(targetCoordinates)
                && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                && !cells[targetCoordinates.x, targetCoordinates.y].isFlagged)
                    unPredictedCells.Add(targetCoordinates);
            }
            return unPredictedCells;
        }

        #region Patterns
        private async Task<bool> TrySolveForCorrespondingNumberOfUnrevealedCells()
        {
            List<Vector2Int> possibleMineCells = new List<Vector2Int>();
            bool isGridChanged = false;
            // Loop through unprocessed number cells
            foreach (var monitoredCell in monitoredCells)
            {
                if (trackedNumbers[monitoredCell.x, monitoredCell.y] == 0) continue;
                possibleMineCells.Clear();
                // For each surrounding cell, check for a possible mine
                foreach (var direction in DirectionsList)
                {
                    Vector2Int targetCoordinates = monitoredCell + direction;
                    if (IsWithinRange(targetCoordinates)
                        && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                        && !predictedMines[targetCoordinates.x, targetCoordinates.y])
                    {
                        possibleMineCells.Add(targetCoordinates);
                    }
                }
                // if the number of possible cells is the same as the cell number
                // => confirm mines and lower count for surrounding cells
                if (possibleMineCells.Count == trackedNumbers[monitoredCell.x, monitoredCell.y])
                {
                    isGridChanged = true;
                    foreach (var possibleMineCell in possibleMineCells)
                    {
                        await Task.Delay((int)(_delay * 1000f));
                        predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                        UpdateCellFlagState(possibleMineCell);
                        SubtractCountFromSurroundingCells(possibleMineCell);
                    }
                }
            }
            return isGridChanged;
        }

        private async Task<bool> TrySolveForCorrespondingNumberOfSurroundingMines()
        {
            HashSet<Vector2Int> unrevealedCells = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToReveal = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToUntarget = new HashSet<Vector2Int>();
            bool isGridChanged = false;
            // Loop through unprocessed number cells
            for (int i = monitoredCells.Count - 1; i >= 0; i--)
            {
                var targetCell = monitoredCells.ElementAt(i);
                unrevealedCells.Clear();
                // For each surrounding cell, check for a mine count and empty cells
                foreach (var direction in DirectionsList)
                {
                    Vector2Int targetCoordinates = targetCell + direction;
                    if (IsWithinRange(targetCoordinates))
                    {
                        if (!cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                            && !predictedMines[targetCoordinates.x, targetCoordinates.y])
                            unrevealedCells.Add(targetCoordinates);
                    }
                }
                // tracked number count is 0 then all the surrounding mines are found
                // => reveal the unrevealed cells and remove the target cells from the monitored list
                if (trackedNumbers[targetCell.x, targetCell.y] == 0)
                {
                    isGridChanged = true;
                    //targetCells.Remove(targetCell);
                    cellsToUntarget.Add(targetCell);
                    foreach (var unrevealedCell in unrevealedCells)
                    {
                        cellsToReveal.Add(unrevealedCell);
                    }
                }
            }
            foreach (var cell in cellsToReveal)
            {
                if (cells[cell.x, cell.y].isRevealed) continue;
                await Task.Delay((int)(_delay * 1000f));
                TryRevealCell(cell);
            }
            foreach (var cell in cellsToUntarget)
            {
                monitoredCells.Remove(cell);
                if (isDebug) Debug.Log($"untargeting {cell}");
            }
            return isGridChanged;
        }

        private async Task<bool> TryFind121Pattern()
        {
            bool isGridChanged = false;

            HashSet<Vector2Int> cellsToReveal = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToMark = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToUntarget = new HashSet<Vector2Int>();

            // Check if there are two side 1s next to the 2
            bool Check1Condition(Vector2Int targetCell, Vector2Int lookDirection)
            {
                Vector2Int cell1 = targetCell + lookDirection;
                bool condition1 = IsWithinRange(cell1)
                    && cells[cell1.x, cell1.y].isRevealed
                    && trackedNumbers[cell1.x, cell1.y] == 1;
                if (!condition1) return false;

                Vector2Int cell2 = targetCell - lookDirection;
                bool condition2 = IsWithinRange(cell2)
                    && cells[cell2.x, cell2.y].isRevealed
                    && trackedNumbers[cell2.x, cell2.y] == 1;
                if (!condition2) return false;

                return true;
            }

            // Check in the surrounding cells if there's ONLY 3 unrevealed and unmarked cells and all of them are in the same row or column
            void CheckFor3UnPredictedCellsInARow(Vector2Int targetCell, out Vector2Int? safeCell, out List<Vector2Int> mineCells)
            {
                safeCell = null;
                mineCells = new List<Vector2Int>();

                List<Vector2Int> unPredictededCells = GetSurroundingUnPredictedCells(targetCell);
                if (unPredictededCells.Count != 3) return;

                // all of the cells are on the same x
                if (unPredictededCells.All(c => c.x == unPredictededCells[0].x))
                {
                    // the cell in the middle is safe and the rest are mines
                    int ySum = unPredictededCells.Sum(c => c.y);
                    int middleY = ySum / 3;
                    Vector2Int predictedSafeCell = unPredictededCells.First(c => c.y == middleY);
                    safeCell = predictedSafeCell;
                    mineCells = unPredictededCells.Where(c => c.y != middleY).ToList();
                }
                //  all of the cells are on the same y
                else if (unPredictededCells.All(c => c.y == unPredictededCells[0].y))
                {
                    // the cell in the middle is safe and the rest are mines
                    int xSum = unPredictededCells.Sum(c => c.x);
                    int middleX = xSum / 3;
                    Vector2Int predictedSafeCell = unPredictededCells.First(c => c.x == middleX);
                    safeCell = predictedSafeCell;
                    mineCells = unPredictededCells.Where(c => c.x != middleX).ToList();
                }
            }

            // Loop through unprocessed number cells
            for (int i = monitoredCells.Count - 1; i >= 0; i--)
            {
                Vector2Int? safeCell = null;
                List<Vector2Int> mineCells = new List<Vector2Int>();

                Vector2Int monitoredCell = monitoredCells.ElementAt(i);
                if (trackedNumbers[monitoredCell.x, monitoredCell.y] == 2)
                {
                    // Check top and bottom cells for 1
                    if (Check1Condition(monitoredCell, Vector2Int.up)
                        || Check1Condition(monitoredCell, Vector2Int.right))
                    {
                        CheckFor3UnPredictedCellsInARow(monitoredCell, out safeCell, out mineCells);
                        if (safeCell.HasValue)
                        {
                            Debug.Log($"Matched 121 Pattern at {monitoredCell}");
                            cellsToReveal.Add(safeCell.Value);
                            cellsToUntarget.Add(monitoredCell);
                            mineCells.ForEach(m => cellsToMark.Add(m));
                            isGridChanged = true;
                        }
                    }
                }
            }
            foreach (var cell in cellsToReveal)
            {
                if (cells[cell.x, cell.y].isRevealed) continue;
                await Task.Delay((int)(_delay * 1000f));
                TryRevealCell(cell);
            }
            foreach (var possibleMineCell in cellsToMark)
            {
                await Task.Delay((int)(_delay * 1000f));
                predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                UpdateCellFlagState(possibleMineCell);
                SubtractCountFromSurroundingCells(possibleMineCell);
            }
            foreach (var cell in cellsToUntarget)
            {
                monitoredCells.Remove(cell);
                if (isDebug) Debug.Log($"untargeting {cell}");
            }
            return isGridChanged;
        }

        private async Task<bool> TryFind1221Pattern()
        {
            bool isGridChanged = false;

            HashSet<Vector2Int> cellsToReveal = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToMark = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToUntarget = new HashSet<Vector2Int>();

            // Check if there are two side 1s next to the 2 middle 2s
            bool CheckPatternCondition(Vector2Int targetCell, Vector2Int lookDirection)
            {
                List<int> patternNumbers = new List<int>() { 1, 2, 1 };
                List<int> lineIndexMultiplier = new List<int>() { -1, 1, 2 };

                for (int i = 0; i < patternNumbers.Count; i++)
                {
                    Vector2Int checkCell = targetCell + (lookDirection * lineIndexMultiplier[i]);
                    if (!IsWithinRange(checkCell)
                        || !cells[checkCell.x, checkCell.y].isRevealed
                        || trackedNumbers[checkCell.x, checkCell.y] != patternNumbers[i])
                        return false;
                }
                return true;
            }

            // Check in the surrounding cells if there's ONLY 4 unrevealed cells and all of them are in the same row or column
            void CheckFor4UnRevealedCellsInARow(Vector2Int targetCell, Vector2Int lookDirection, out List<Vector2Int> safeCells, out List<Vector2Int> mineCells)
            {
                Vector2Int perpendicularDirection = new Vector2Int(lookDirection.y, lookDirection.x);
                List<int> lineIndexMultiplier = new List<int>() { -1, 0, 1, 2 };

                // positive perpendicular direction
                Vector2Int positiveSideCell = targetCell + perpendicularDirection;
                List<Vector2Int> positiveSideCells = new List<Vector2Int>();
                foreach (var multiplier in lineIndexMultiplier)
                {
                    Vector2Int checkCell = positiveSideCell + (lookDirection * multiplier);
                    if (IsWithinRange(checkCell)
                        && !cells[checkCell.x, checkCell.y].isRevealed
                        && !cells[checkCell.x, checkCell.y].isFlagged)
                    {
                        positiveSideCells.Add(checkCell);
                    }
                }

                // negative perpendicular direction
                Vector2Int negativeSideCell = targetCell - perpendicularDirection;
                List<Vector2Int> negativeSideCells = new List<Vector2Int>();
                foreach (var multiplier in lineIndexMultiplier)
                {
                    Vector2Int checkCell = negativeSideCell + (lookDirection * multiplier);
                    if (IsWithinRange(checkCell)
                        && !cells[checkCell.x, checkCell.y].isRevealed
                        && !cells[checkCell.x, checkCell.y].isFlagged)
                    {
                        negativeSideCells.Add(checkCell);
                    }
                }

                safeCells = new List<Vector2Int>();
                mineCells = new List<Vector2Int>();
                // If the count of the cells is not 4, the pattern fails
                if (positiveSideCells.Count + negativeSideCells.Count != 4) return;

                // If those unpredicted cells are all on the positive side, process those
                if (positiveSideCells.Count == 4)
                {
                    safeCells.Add(positiveSideCells[0]);
                    mineCells.Add(positiveSideCells[1]);
                    mineCells.Add(positiveSideCells[2]);
                    safeCells.Add(positiveSideCells[3]);
                }
                // If those unpredicted cells are all on the negative side, process those
                else if (negativeSideCells.Count == 4)
                {
                    safeCells.Add(negativeSideCells[0]);
                    mineCells.Add(negativeSideCells[1]);
                    mineCells.Add(negativeSideCells[2]);
                    safeCells.Add(negativeSideCells[3]);
                }
            }

            // Loop through unprocessed number cells
            for (int i = monitoredCells.Count - 1; i >= 0; i--)
            {
                List<Vector2Int> safeCells = new List<Vector2Int>();
                List<Vector2Int> mineCells = new List<Vector2Int>();

                Vector2Int targetCell = monitoredCells.ElementAt(i);
                if (trackedNumbers[targetCell.x, targetCell.y] == 2)
                {
                    if (CheckPatternCondition(targetCell, Vector2Int.up))
                    {
                        CheckFor4UnRevealedCellsInARow(targetCell, Vector2Int.up, out safeCells, out mineCells);
                        if (safeCells.Count != 0) // we found a match
                        {
                            Debug.Log($"Matched 1221 Pattern at {targetCell}");
                            safeCells.ForEach(s => cellsToReveal.Add(s));
                            mineCells.ForEach(m => cellsToMark.Add(m));
                            cellsToUntarget.Add(targetCell);
                            isGridChanged = true;
                            continue;
                        }
                    }
                }
            }
            foreach (var cell in cellsToReveal)
            {
                if (cells[cell.x, cell.y].isRevealed) continue;
                await Task.Delay((int)(_delay * 1000f));
                TryRevealCell(cell);
            }
            foreach (var possibleMineCell in cellsToMark)
            {
                await Task.Delay((int)(_delay * 1000f));
                predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                UpdateCellFlagState(possibleMineCell);
                SubtractCountFromSurroundingCells(possibleMineCell);
            }
            foreach (var cell in cellsToUntarget)
            {
                monitoredCells.Remove(cell);
                if (isDebug) Debug.Log($"untargeting {cell}");
            }
            return isGridChanged;
        }
        #endregion

        private async Task<bool> TryTestAllPossibleCases()
        {
            List<Vector2Int> FindEdgeUnpredictedCells()
            {
                // hashsets don't repeat values
                HashSet<Vector2Int> EdgeCells = new HashSet<Vector2Int>();

                foreach (var monitoredCell in monitoredCells)
                {
                    List<Vector2Int> result = GetSurroundingUnPredictedCells(monitoredCell);
                    result.ForEach(c => EdgeCells.Add(c));
                }

                return EdgeCells.ToList();
            }

            bool IsASolution()
            {
                foreach (var monitoredCell in monitoredCells)
                {
                    // IF all the edge cells are visited
                    // AND there is still monitored cells not pointing to corresponding number of mines
                    // THEN the current solution is not coorect
                    if (trackedNumbers[monitoredCell.x, monitoredCell.y] > 0) return false;
                }
                return true;
            }

            bool IsSafeToPlaceMine(int[,] trackedNumbers, Vector2Int targetCell, int currentPredictedMinesCount)
            {
                // Not safe if the current predicted mines count + current cell as mine IS more than the remaining mines
                if (currentPredictedMinesCount + 1 > RemainingMines) return false;

                // The traget cell is checked previously for the range condition

                foreach (var direction in DirectionsList)
                {
                    Vector2Int lookDirection = targetCell + direction;
                    if (IsWithinRange(lookDirection)
                        && cells[lookDirection.x, lookDirection.y].isRevealed
                        && trackedNumbers[lookDirection.x, lookDirection.y] - 1 < 0)
                    {
                        Debug.Log($"Not Safe at {targetCell} cuz {lookDirection} is {trackedNumbers[lookDirection.x, lookDirection.y]}");
                        return false;
                    }
                }

                foreach (var direction in DirectionsList)
                {
                    Vector2Int lookDirection = targetCell + direction;
                    if (IsWithinRange(lookDirection))
                        trackedNumbers[lookDirection.x, lookDirection.y]--;
                }

                return true;
            }

            void TestAllPossibleCases(List<Vector2Int> edgeCells,
                                      List<List<Vector2Int>> possibleSolutions,
                                      List<Vector2Int> predictedMines,
                                      int index)
            {
                index++;

                if (index == edgeCells.Count)
                {
                    if (IsASolution())
                    {
                        possibleSolutions.Add(new List<Vector2Int>(predictedMines));
                        string s = "";
                        predictedMines.ForEach(p => s += p);
                        Debug.Log("Found Solution\n" + s);
                    }
                    return;
                }

                Vector2Int currentCell = edgeCells[index];
                Debug.Log($"try mine at {currentCell}");
                if (IsSafeToPlaceMine(trackedNumbers, currentCell, predictedMines.Count))
                {
                    // Mark current cell as a mine
                    predictedMines.Add(currentCell);

                    // Recursive call with current cell having a mine
                    TestAllPossibleCases(edgeCells, possibleSolutions, predictedMines, index);

                    // Mark current cell as a safe
                    predictedMines.Remove(currentCell);
                    foreach (var direction in DirectionsList)
                    {
                        Vector2Int lookDirection = currentCell + direction;
                        if (IsWithinRange(lookDirection))
                            trackedNumbers[lookDirection.x, lookDirection.y]++;
                    }
                }
                Debug.Log($"try with no mine at {currentCell}");
                // Recursive call with current cell having a mine
                TestAllPossibleCases(edgeCells, possibleSolutions, predictedMines, index);
            }


            int Compare(Vector2Int v1, Vector2Int v2)
            {
                if (v1.x > v2.x) return 1;
                else if (v1.x < v2.x) return -1;
                else // (v1.x == v2.x)
                {
                    if (v1.y > v2.y) return 1;
                    else if (v1.y < v2.y) return -1;
                    else return 0;
                }
            }

            List<Vector2Int> edgeCells = FindEdgeUnpredictedCells();

            Debug.Log("Unpredicted cells");
            edgeCells.ForEach(e => Debug.Log(e));

            edgeCells.Sort((x, y) => Compare(x, y));
            Debug.Log("Unpredicted cells after sort");
            edgeCells.ForEach(e => Debug.Log(e));


            List<List<Vector2Int>> possibleSolutions = new List<List<Vector2Int>>();
            TestAllPossibleCases(edgeCells, possibleSolutions, new List<Vector2Int>(), -1);

            if (possibleSolutions.Count == 0)
            {
                // no possible solution
                // don't apply anything
                return false;
            }
            else if (possibleSolutions.Count == 1)
            {
                // only one correct possible solution
                // Apply solution
                foreach (var possibleMineCell in possibleSolutions[0])
                {
                    await Task.Delay((int)(_delay * 1000f));
                    predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                    UpdateCellFlagState(possibleMineCell);
                    SubtractCountFromSurroundingCells(possibleMineCell);
                }
                return true;
            }
            else // multiple possible solutions
            {
                List<Vector2Int> certainMineCells = new List<Vector2Int>();
                List<Vector2Int> certainSafeCells = new List<Vector2Int>();

                bool isChanging = false;

                foreach (var edgeCell in edgeCells)
                {
                    // iF all possible solutions contain this cell as a mine
                    // THEN this cell is certainly a mine
                    if (possibleSolutions.All(list => list.Contains(edgeCell)))
                        certainMineCells.Add(edgeCell);

                    // iF non of the possible solutions contain this cell as a mine
                    // THEN this cell is certainly safe
                    else if (possibleSolutions.All(list => !list.Contains(edgeCell)))
                        certainSafeCells.Add(edgeCell);
                }

                foreach (var certainMineCell in certainMineCells)
                {
                    await Task.Delay((int)(_delay * 1000f));
                    predictedMines[certainMineCell.x, certainMineCell.y] = true;
                    UpdateCellFlagState(certainMineCell);
                    SubtractCountFromSurroundingCells(certainMineCell);
                    isChanging = true;
                }

                foreach (var certainSafeCell in certainSafeCells)
                {
                    if (cells[certainSafeCell.x, certainSafeCell.y].isRevealed) continue;
                    await Task.Delay((int)(_delay * 1000f));
                    TryRevealCell(certainSafeCell);
                    isChanging = true;
                }

                return isChanging;
            }
        }

        public async void TrySolve()
        {
            Vector2Int emptyCell = GetRandomEmptyCell();
            TryRevealCell(emptyCell);
            bool isChanging = true;

            isChanging = await TryTestAllPossibleCases();
            await Awaitable.WaitForSecondsAsync(2);
            isChanging = await TryTestAllPossibleCases();
            await Awaitable.WaitForSecondsAsync(2);
            isChanging = await TryTestAllPossibleCases();
            await Awaitable.WaitForSecondsAsync(2);

            while (isChanging)
            {
                //bool isMinesPredicted = await TrySolveForCorrespondingNumberOfSurroundingMines();
                //bool isCellsRevealed = await TrySolveForCorrespondingNumberOfUnrevealedCells();
                //bool is121PatternFound = await TryFind121Pattern();
                //bool is1221PatternFound = await TryFind1221Pattern();
                //isChanging = isMinesPredicted || isCellsRevealed || is121PatternFound || is1221PatternFound;
                //if (isChanging) continue;
                //else
                //{
                //    isChanging = await TryTestAllPossibleCases();
                //}

                isChanging = await TryTestAllPossibleCases();
            }
            Debug.Log("Finish");
        }

        private Vector2Int GetRandomEmptyCell()
        {
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].number == 0) return new Vector2Int(i, j);
                }
            }
            return Vector2Int.zero;
        }
    }
}
