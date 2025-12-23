using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeperRipeoff
{
    [Serializable]
    public class OptimizedGridSolver : Grid
    {
        private bool _isRecursiveDebug;
        private bool _usePredefinedGrid;
        private float _delay;

        private List<PatternMatcher> patternMatchers = new List<PatternMatcher>()
        {
            new PatternMatcherCorrespondingNumberOfUnrevealedCells(),
            new PatternMatcherCorrespondingNumberOfSurroundingMines(),
            new PatternMatcher121(),
            new PatternMatcher1221()
        };

        public OptimizedGridSolver(float delay = 1, bool usePredefinedGrid = false) : base()
        {
            this._usePredefinedGrid = usePredefinedGrid;
            this._delay = delay;
        }

        public OptimizedGridSolver(Cell[,] cellsCopy, int numberOfMines, float delay = 0, bool isDebug = false, bool isRecursiveDebug = false) : base(cellsCopy, numberOfMines)
        {
            this.isDebug = isDebug;
            this._isRecursiveDebug = isRecursiveDebug;
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

        private int[,] trackedNumbers;
        private bool[,] predictedMines;
        private HashSet<Vector2Int> monitoredCells = new HashSet<Vector2Int>();

        public override void Initialize(Vector2Int? gridSizeTraget, int? numberOfMinesTraget, GameMode gameMode, bool isSpeedRunMode = true)
        {
            if (!_usePredefinedGrid) base.Initialize(gridSizeTraget, numberOfMinesTraget, gameMode, isSpeedRunMode);
            else
            {
                this.gridSize = new Vector2Int(16, 8);
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
                    new Vector2Int(2, 5),
                    new Vector2Int(3, 7),
                    new Vector2Int(5, 2),
                    new Vector2Int(9, 3),
                    new Vector2Int(10, 1),
                    new Vector2Int(13, 5),
                    new Vector2Int(13, 2),
                    new Vector2Int(15, 7),
                    new Vector2Int(9, 5),
                    new Vector2Int(14, 1),
                    new Vector2Int(7, 5),
                    new Vector2Int(6, 2),
                    new Vector2Int(7, 1),
                    new Vector2Int(8, 3),
                };
                numberOfMines = minesList.Count;

                IsFirstMove = true;
                CurrentState = GridState.NotStarted;
                numberOfRevealedCells = 0;
                OnGridStateChanged.RemoveAllListeners();
                RemainingUnflaggedMines = minesList.Count;

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

        private async Task<bool> TryTestAllPossibleCases()
        {
            List<Vector2Int> FindUnpredictedEdgeCells()
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
                if (currentPredictedMinesCount + 1 > RemainingUnflaggedMines) return false;

                // The traget cell is checked previously for the range condition

                foreach (var direction in DirectionsList)
                {
                    Vector2Int lookDirection = targetCell + direction;
                    if (IsWithinRange(lookDirection)
                        && cells[lookDirection.x, lookDirection.y].isRevealed
                        && trackedNumbers[lookDirection.x, lookDirection.y] - 1 < 0)
                    {
                        if (_isRecursiveDebug) Debug.Log($"Not Safe at {targetCell} cuz {lookDirection} is {trackedNumbers[lookDirection.x, lookDirection.y]}");
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
                                      List<List<Vector2Int>> possibleGridSolutions,
                                      List<Vector2Int> predictedMines,
                                      int index)
            {
                index++;

                if (index == edgeCells.Count)
                {
                    if (IsASolution())
                    {
                        possibleGridSolutions.Add(new List<Vector2Int>(predictedMines));
                        string s = "";
                        predictedMines.ForEach(p => s += p);
                        if (_isRecursiveDebug) Debug.Log("Found Solution\n" + s);
                    }
                    return;
                }

                Vector2Int currentCell = edgeCells[index];
                if (_isRecursiveDebug) Debug.Log($"try mine at {currentCell}");
                if (IsSafeToPlaceMine(trackedNumbers, currentCell, predictedMines.Count))
                {
                    // Mark current cell as a mine
                    predictedMines.Add(currentCell);

                    // Recursive call with current cell having a mine
                    TestAllPossibleCases(edgeCells, possibleGridSolutions, predictedMines, index);

                    // Mark current cell as a safe
                    predictedMines.Remove(currentCell);
                    foreach (var direction in DirectionsList)
                    {
                        Vector2Int lookDirection = currentCell + direction;
                        if (IsWithinRange(lookDirection))
                            trackedNumbers[lookDirection.x, lookDirection.y]++;
                    }
                }
                if (_isRecursiveDebug) Debug.Log($"try with no mine at {currentCell}");
                // Recursive call with current cell having a mine
                TestAllPossibleCases(edgeCells, possibleGridSolutions, predictedMines, index);
            }


            // this is used to sort the cells from top to bottom for easier debugging
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

            List<Vector2Int> edgeCells = FindUnpredictedEdgeCells();

            string unpredictedCellsString = "";
            edgeCells.ForEach(e => unpredictedCellsString += e);
            //Debug.Log("Unpredicted cells: " + unpredictedCellsString);

            edgeCells.Sort((x, y) => Compare(x, y));
            string unpredictedSortedCellsString = "";
            edgeCells.ForEach(e => unpredictedSortedCellsString += e);
            //Debug.Log("Unpredicted cells after sort:" + unpredictedSortedCellsString);

            List<List<Vector2Int>> possibleGirdSolutions = new List<List<Vector2Int>>();
            TestAllPossibleCases(edgeCells, possibleGirdSolutions, new List<Vector2Int>(), -1);

            if (possibleGirdSolutions.Count == 0)
            {
                // no possible solution
                // don't apply anything
                return false;
            }
            else if (possibleGirdSolutions.Count == 1)
            {
                // only one correct possible solution
                // Apply solution

                List<Vector2Int> certainMineCells = possibleGirdSolutions[0];
                List<Vector2Int> certainSafeCells = edgeCells.Where(e => !certainMineCells.Contains(e)).ToList();
                List<Vector2Int> cellsToUntarget = new List<Vector2Int>(monitoredCells);

                await MarkCells(certainMineCells);
                await RevealCells(certainSafeCells);
                UntargetCells(cellsToUntarget);

                bool isMatch = certainMineCells.Count > 0 || certainSafeCells.Count > 0 || cellsToUntarget.Count > 0;

                return isMatch;
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
                    if (possibleGirdSolutions.All(list => list.Contains(edgeCell)))
                        certainMineCells.Add(edgeCell);

                    // iF non of the possible solutions contain this cell as a mine
                    // THEN this cell is certainly safe
                    else if (possibleGirdSolutions.All(list => !list.Contains(edgeCell)))
                        certainSafeCells.Add(edgeCell);
                }

                await MarkCells(certainMineCells);
                await RevealCells(certainSafeCells);

                string s = "";
                monitoredCells.ToList().ForEach(m => s += m);
                //Debug.Log("monitored cells " + s);

                return isChanging;
            }
        }

        private async Task RevealCells(IEnumerable<Vector2Int> cellsToReveal)
        {
            foreach (var cell in cellsToReveal)
            {
                if (cells[cell.x, cell.y].isRevealed) continue;
                //if (_delay != 0) await Task.Delay((int)(_delay * 1000f));
                if (_delay != 0) await Awaitable.WaitForSecondsAsync(_delay);

                TryRevealCell(cell);
            }
        }
        private async Task MarkCells(IEnumerable<Vector2Int> cellsToMark)
        {
            foreach (var possibleMineCell in cellsToMark)
            {
                //if (_delay != 0) await Task.Delay((int)(_delay * 1000f));
                if (_delay != 0) await Awaitable.WaitForSecondsAsync(_delay);

                predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                UpdateCellFlagState(possibleMineCell);
                SubtractCountFromSurroundingCells(possibleMineCell);
            }
        }
        private void UntargetCells(IEnumerable<Vector2Int> cellsToUntarget)
        {
            foreach (var cell in cellsToUntarget)
            {
                monitoredCells.Remove(cell);
                if (isDebug) Debug.Log($"untargeting {cell}");
            }
        }

        private async Task DealWithThose(
            HashSet<Vector2Int> cellsToReveal,
            HashSet<Vector2Int> cellsToMark,
            HashSet<Vector2Int> cellsToUntarget
            )
        {
            await RevealCells(cellsToReveal);
            await MarkCells(cellsToMark);
            UntargetCells(cellsToUntarget);
        }

        public async Task<Vector2Int> TrySolve()
        {
            Vector2Int startCell = GetRandomEmptyCell();
            TryRevealCell(startCell);
            bool isChanging = true;

            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            HashSet<Vector2Int> cellsToReveal = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToMark = new HashSet<Vector2Int>();
            HashSet<Vector2Int> cellsToUntarget = new HashSet<Vector2Int>();

            while (isChanging)
            {
                cellsToReveal.Clear();
                cellsToMark.Clear();
                cellsToUntarget.Clear();

                isChanging = false;

                foreach (var monitoredCell in monitoredCells)
                {
                    foreach (var patternMatcher in patternMatchers)
                    {
                        bool patternWasMatched = patternMatcher.TryMatch(cells, gridSize, trackedNumbers, predictedMines, monitoredCell, cellsToReveal, cellsToMark, cellsToUntarget);
                        isChanging = isChanging || patternWasMatched;

                        await MarkCells(cellsToMark);// marking cells can be applied off the bat because it doesn't modify the monitoredCells list
                        cellsToMark.Clear();
                        if (patternWasMatched) break;
                    }
                }

                if (isChanging)
                    await DealWithThose(cellsToReveal, cellsToMark, cellsToUntarget);

                // brute forece is expencive and should not be applied until all the patterns fail to match
                if (isChanging) continue;
                else
                {
                    isChanging = await TryTestAllPossibleCases();
                }
            }
            Debug.Log("Finish");
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            UnityEngine.Debug.Log(ts);
            return startCell;
        }

        private Vector2Int GetRandomEmptyCell()
        {
            List<Vector2Int> emptyCells = new List<Vector2Int>();
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (cells[i, j].number == 0) emptyCells.Add(new Vector2Int(i, j));
                    //if (cells[i, j].number == 0) return new Vector2Int(i, j);
                }
            }
            return emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
        }
    }
}
