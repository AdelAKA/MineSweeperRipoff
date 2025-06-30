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
