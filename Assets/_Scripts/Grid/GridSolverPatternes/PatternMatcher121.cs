using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class PatternMatcher121 : PatternMatcher
    {
        public override bool TryMatch(
                Cell[,] cells,
                Vector2Int gridSize,
                int[,] trackedNumbers,
                bool[,] predictedMines,
                Vector2Int monitoredCell,
                HashSet<Vector2Int> cellsToReveal,
                HashSet<Vector2Int> cellsToMark,
                HashSet<Vector2Int> cellsToUntarget)
        {
            bool isGridChanged = false;

            // Check if there are two side 1s next to the 2
            bool Check1Condition(Vector2Int targetCell, Vector2Int lookDirection)
            {
                Vector2Int cell1 = targetCell + lookDirection;
                bool condition1 = IsWithinRange(gridSize, cell1)
                    && cells[cell1.x, cell1.y].isRevealed
                    && trackedNumbers[cell1.x, cell1.y] == 1;
                if (!condition1) return false;

                Vector2Int cell2 = targetCell - lookDirection;
                bool condition2 = IsWithinRange(gridSize, cell2)
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

                List<Vector2Int> unPredictededCells = GetSurroundingUnPredictedCells(cells, gridSize, targetCell);
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

            if (trackedNumbers[monitoredCell.x, monitoredCell.y] != 2) return false;

            Vector2Int? safeCell = null;
            List<Vector2Int> mineCells = new List<Vector2Int>();

            // Check top and bottom cells for 1
            if (Check1Condition(monitoredCell, Vector2Int.up)
                || Check1Condition(monitoredCell, Vector2Int.right))
            {
                CheckFor3UnPredictedCellsInARow(monitoredCell, out safeCell, out mineCells);
                if (safeCell.HasValue)
                {
                    //Debug.Log($"Matched 121 Pattern at {monitoredCell}");
                    //if (cellsToReveal.Contains(safeCell.Value)) Debug.Log($"adding {safeCell.Value} again to cellsToReveal");
                    cellsToReveal.Add(safeCell.Value);
                    //if (cellsToUntarget.Contains(monitoredCell)) Debug.Log($"adding {monitoredCell} again to cellsToUntarget");
                    cellsToUntarget.Add(monitoredCell);
                    mineCells.ForEach(m =>
                    {
                        //if (cellsToMark.Contains(m)) Debug.Log($"adding {m} again to cellsToMark");
                        cellsToMark.Add(m);
                    });
                    isGridChanged = true;
                }
            }

            return isGridChanged;
        }
    }
}
