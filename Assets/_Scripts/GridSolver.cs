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
        public GridSolver() : base() { }

        public GridSolver(Grid copy) : base(copy)
        {
            isDebug = true;
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
        private HashSet<Vector2Int> targetCells = new HashSet<Vector2Int>();

        protected override void RevealCell(Vector2Int coordinates, int sequenceCount)
        {
            base.RevealCell(coordinates, sequenceCount);
            if (cells[coordinates.x, coordinates.y].cellType == CellType.Number
                && cells[coordinates.x, coordinates.y].number > 0
                )
            {
                targetCells.Add(coordinates);
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

        private bool TrySolveForSameNumberOfUnrevealedCells()
        {
            List<Vector2Int> possibleMineCells = new List<Vector2Int>();
            bool isGridChanged = false;
            // Loop through unprocessed number cells
            foreach (var targetCell in targetCells)
            {
                if (trackedNumbers[targetCell.x, targetCell.y] == 0) continue;
                possibleMineCells.Clear();
                // For each surrounding cell, check for a possible mine
                foreach (var direction in DirectionsList)
                {
                    Vector2Int targetCoordinates = targetCell + direction;
                    if (IsWithinRange(targetCoordinates)
                        && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                        && !predictedMines[targetCoordinates.x, targetCoordinates.y])
                    {
                        possibleMineCells.Add(targetCoordinates);
                    }
                }
                // if the number of possible cells is the same as the cell number
                // => confirm mines and lower count for surrounding cells
                if (possibleMineCells.Count == trackedNumbers[targetCell.x, targetCell.y])
                {
                    isGridChanged = true;
                    foreach (var possibleMineCell in possibleMineCells)
                    {
                        predictedMines[possibleMineCell.x, possibleMineCell.y] = true;
                        UpdateCellFlagState(possibleMineCell);
                        SubtractCountFromSurroundingCells(possibleMineCell);
                    }
                }
            }
            return isGridChanged;
        }

        private async Task<bool> TrySolveForSameNumberOfSurroundingMines()
        {
            List<Vector2Int> unrevealedCells = new List<Vector2Int>();
            List<Vector2Int> cellsToReveal = new List<Vector2Int>();
            List<Vector2Int> cellsToUntarget = new List<Vector2Int>();
            bool isGridChanged = false;
            // Loop through unprocessed number cells
            for (int i = targetCells.Count - 1; i >= 0; i--)
            {
                var targetCell = targetCells.ElementAt(i);
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
                await Task.Delay(1000);
                TryRevealCell(cell);
            }
            foreach (var cell in cellsToUntarget)
            {
                targetCells.Remove(cell);
                if (isDebug) Debug.Log($"untargeting {cell}");
            }
            return isGridChanged;
        }

        //private bool TryFind121Pattern()
        //{
        //    bool isGridChanged = false;

        //    bool Check1Condition(Vector2Int targetCell, Vector2Int lookDirection)
        //    {
        //        Vector2Int cell1 = targetCell + lookDirection;
        //        bool condition1 = IsWithinRange(cell1)
        //            && cells[cell1.x, cell1.y].isRevealed
        //            && trackedNumbers[cell1.x, cell1.y] == 1;
        //        if (!condition1) return false;

        //        Vector2Int cell2 = targetCell - lookDirection;
        //        bool condition2 = IsWithinRange(cell2)
        //            && cells[cell2.x, cell2.y].isRevealed
        //            && trackedNumbers[cell2.x, cell2.y] == 1;
        //        if (!condition2) return false;

        //        return true;
        //    }

        //    List<Vector2Int> GetSurroundingUnRevealedCells(Vector2Int targetCell)
        //    {
        //        List<Vector2Int> unRevealedCells = new List<Vector2Int>();
        //        foreach (var direction in DirectionsList)
        //        {
        //            Vector2Int targetCoordinates = targetCell + direction;
        //            if (IsWithinRange(targetCoordinates)
        //            && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed)
        //                unRevealedCells.Add(targetCoordinates);
        //        }
        //        return unRevealedCells;
        //    }

        //    bool CheckFor3UnRevealedCells(Vector2Int targetCell, Vector2Int lookDirection)
        //    {
        //        List<Vector2Int> unRevealedCells = GetSurroundingUnRevealedCells(targetCell);
        //        if (unRevealedCells.Count != 3) return false;

        //        // same x
        //        int targetX = unRevealedCells[0].x;
        //        if (unRevealedCells.All(c=> c.x == targetX))
        //        {
        //            // the cell in the middle is safe and the rest are mines
        //            int ySum = unRevealedCells.Sum(c => c.y);
        //            int middleY = ySum / 3;
        //            Vector2Int safeCell = unRevealedCells.First(c => c.y == middleY);
        //        }
        //        // same y
        //        int targetY = unRevealedCells[0].y;
        //        if (unRevealedCells.All(c => c.y == targetY))
        //        {
        //            int xSum = unRevealedCells.Sum(c => c.x);
        //            int middleX = xSum / 3;
        //            Vector2Int safeCell = unRevealedCells.First(c => c.x == middleX);
        //        }
        //    }

        //    // Loop through unprocessed number cells
        //    for (int i = targetCells.Count - 1; i >= 0; i--)
        //    {
        //        Vector2Int targetCell = targetCells.ElementAt(i);
        //        if (trackedNumbers[targetCell.x, targetCell.y] == 2)
        //        {
        //            // Check top and bottom cells for 1
        //            bool topBottomCondition = Check1Condition(targetCell, Vector2Int.up);

        //            // Check Right and left cells
        //            if (topBottomCondition)
        //            {
        //                List<Vector2Int> monitoredCells = new List<Vector2Int>() {
        //                    new Vector2Int(targetCell.x + 1, targetCell.y + 1),
        //                    new Vector2Int(targetCell.x + 1, targetCell.y),
        //                    new Vector2Int(targetCell.x + 1, targetCell.y - 1)
        //                };
        //                bool passedEmptyCellsCondition = true;
        //                foreach (var monitoredCell in monitoredCells)
        //                {
        //                    bool condition = IsWithinRange(monitoredCell)
        //                    && !cells[monitoredCell.x, monitoredCell.y].isRevealed;
        //                    if (!condition)
        //                    {
        //                        passedEmptyCellsCondition = false;
        //                        break;
        //                    }
        //                }
        //                if (passedEmptyCellsCondition)
        //                {
        //                    // Top and Bottom are mines
        //                }
        //            }
        //        }
        //    }
        //}

        public async void TrySolve()
        {
            Vector2Int emptyCell = GetRandomEmptyCell();
            TryRevealCell(emptyCell);
            bool isChanging = true;
            while (isChanging)
            {
                bool isMinesPredicted = await TrySolveForSameNumberOfSurroundingMines();
                bool isCellsRevealed = TrySolveForSameNumberOfUnrevealedCells();
                isChanging = isMinesPredicted || isCellsRevealed;
            }
            if (isDebug) Debug.Log("Finish");
        }

        private Vector2Int GetRandomEmptyCell()
        {
            List<Vector2Int> emptyCells = new();
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
