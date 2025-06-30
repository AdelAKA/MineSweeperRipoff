using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MineSweeperRipeoff
{
    public class MineSweeperSolver
    {
        private Cell[,] revealedGrid;
        private int[,] trackedNumbers;
        private bool[,] predictedMines;
        private HashSet<Vector2Int> targetCells = new HashSet<Vector2Int>();

        private Vector2Int gridSize;
        private int numberOfRevealedCells = 0;
        private int numberOfMines = 0;

        private List<Vector2Int> DirectionsList = new List<Vector2Int>()
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

        public void Initialize(Cell[,] cells, Vector2Int gridSize, int numberOfMines)
        {
            //revealedGrid = new Cell[gridSize.x, gridSize.y];
            revealedGrid = cells;
            trackedNumbers = new int[gridSize.x, gridSize.y];
            predictedMines = new bool[gridSize.x, gridSize.y];
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    //revealedGrid[i, j] = new Cell(cells[i, j]);
                    trackedNumbers[i, j] = cells[i, j].number;
                }
            }
            this.gridSize = gridSize;
            numberOfRevealedCells = 0;
            this.numberOfMines = numberOfMines;
        }

        private void RevealCell(Vector2Int coordinates)
        {
            revealedGrid[coordinates.x, coordinates.y].RevealCell(0);
            if (revealedGrid[coordinates.x, coordinates.y].cellType == CellType.Number
                && trackedNumbers[coordinates.x, coordinates.y] > 0
                ) 
                targetCells.Add(coordinates);
            Debug.Log($"RevealedCell {coordinates}");
            numberOfRevealedCells++;
        }

        private bool TryRevealCell(Vector2Int coordinates)
        {
            if (revealedGrid[coordinates.x, coordinates.y].cellType == CellType.Empty)
            {
                RevealCellsRecursivley(coordinates);
                Debug.Log("Finish Recursion");
            }

            if (revealedGrid[coordinates.x, coordinates.y].cellType == CellType.Number)
            {
                RevealCell(coordinates);
            }
            return true;
        }

        private void RevealCellsRecursivley(Vector2Int coordinates)
        {
            if (revealedGrid[coordinates.x, coordinates.y].cellType == CellType.Number)
            {
                RevealCell(coordinates);
                return;
            }

            RevealCell(coordinates);

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates)
                    && !revealedGrid[targetCoordinates.x, targetCoordinates.y].isRevealed)
                {
                    RevealCellsRecursivley(targetCoordinates);
                }
            }
        }

        private bool IsWithinRange(Vector2Int coordinates)
        {
            return coordinates.x >= 0
                    && coordinates.x < gridSize.x
                    && coordinates.y >= 0
                    && coordinates.y < gridSize.y;
        }

        public bool IsSolved()
        {
            return numberOfRevealedCells + numberOfMines == gridSize.x * gridSize.y;
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
                        && !revealedGrid[targetCoordinates.x, targetCoordinates.y].isRevealed
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
                        SubtractCountFromSurroundingCells(possibleMineCell);
                    }
                }
            }
            return isGridChanged;
        }

        private bool TrySolveForSameNumberOfSurroundingMines()
        {
            List<Vector2Int> unrevealedCells = new List<Vector2Int>();
            int surroundingMinesCount;
            bool isGridChanged = false;
            // Loop through unprocessed number cells
            foreach (var targetCell in targetCells)
            {
                unrevealedCells.Clear();
                surroundingMinesCount = 0;
                // For each surrounding cell, check for a mine count and empty cells
                foreach (var direction in DirectionsList)
                {
                    Vector2Int targetCoordinates = targetCell + direction;
                    if (IsWithinRange(targetCoordinates))
                    {
                        if (!predictedMines[targetCoordinates.x, targetCoordinates.y])
                            surroundingMinesCount++;
                        else if (!revealedGrid[targetCoordinates.x, targetCoordinates.y].isRevealed)
                            unrevealedCells.Add(targetCoordinates);
                    }
                }
                // If mine count is the same as cell number
                // => reveal the unrevealed cells and remove the target cells from the monitored list
                if (surroundingMinesCount == trackedNumbers[targetCell.x, targetCell.y])
                {
                    isGridChanged = true;
                    foreach (var unrevealedCell in unrevealedCells)
                    {
                        TryRevealCell(unrevealedCell);
                    }

                }
            }
            return isGridChanged;
        }

        private void SubtractCountFromSurroundingCells(Vector2Int coordinates)
        {
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(targetCoordinates))
                {
                    trackedNumbers[targetCoordinates.x, targetCoordinates.y]--;
                }
            }
        }

        public void TrySolve()
        {
            Vector2Int emptyCell =  GetRandomEmptyCell();
            TryRevealCell(emptyCell);
            bool isChanging = true;
            while (isChanging)
            {
                bool isMinesPredicted = TrySolveForSameNumberOfSurroundingMines();
                bool isCellsRevealed = TrySolveForSameNumberOfUnrevealedCells();
                isChanging = isMinesPredicted || isCellsRevealed;
            }
        }

        private Vector2Int GetRandomEmptyCell()
        {
            List<Vector2Int> emptyCells = new();
            for (int i = 0; i < gridSize.x; i++)
            {
                for (int j = 0; j < gridSize.y; j++)
                {
                    if (revealedGrid[i, j].number == 0) return new Vector2Int(i, j);
                }
            }
            return Vector2Int.zero;
        }
    }
}