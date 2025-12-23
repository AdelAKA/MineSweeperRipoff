using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class PatternMatcherCorrespondingNumberOfUnrevealedCells : PatternMatcher
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
            // Loop through unprocessed number cells
            if (trackedNumbers[monitoredCell.x, monitoredCell.y] == 0) return false;
            List<Vector2Int> possibleMineCells = new List<Vector2Int>();
            bool isGridChanged = false;

            // For each surrounding cell, check for a possible mine
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = monitoredCell + direction;
                if (IsWithinRange(gridSize, targetCoordinates)
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
                //Debug.Log($"Matched Corresponding Unpredicted cells at {monitoredCell}");

                isGridChanged = true;
                //if (cellsToUntarget.Contains(monitoredCell)) Debug.Log($"adding {monitoredCell} again to cellsToUntarget");
                cellsToUntarget.Add(monitoredCell);
                foreach (var possibleMineCell in possibleMineCells)
                {
                    //if (cellsToMark.Contains(possibleMineCell)) Debug.Log($"adding {possibleMineCell} again to cellsToMark");
                    cellsToMark.Add(possibleMineCell);
                }
            }

            return isGridChanged;
        }
    }
}
