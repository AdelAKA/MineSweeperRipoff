using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class PatternMatcherCorrespondingNumberOfSurroundingMines : PatternMatcher
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
            if (trackedNumbers[monitoredCell.x, monitoredCell.y] != 0) return false;
            // tracked number count is 0 then all the surrounding mines are found

            HashSet<Vector2Int> unrevealedCells = new HashSet<Vector2Int>();
            bool isGridChanged = false;

            //unrevealedCells.Clear();
            // For each surrounding cell, check for a mine count and empty cells
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = monitoredCell + direction;
                if (IsWithinRange(gridSize, targetCoordinates))
                {
                    if (!cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                        && !predictedMines[targetCoordinates.x, targetCoordinates.y])
                        unrevealedCells.Add(targetCoordinates);
                }
            }

            // reveal the unrevealed cells and remove the target cells from the monitored list
            isGridChanged = true;
            //if (cellsToUntarget.Contains(monitoredCell)) Debug.Log($"adding {monitoredCell} again to cellsToUntarget");
            cellsToUntarget.Add(monitoredCell);
            foreach (var unrevealedCell in unrevealedCells)
            {
                //if (cellsToReveal.Contains(unrevealedCell)) Debug.Log($"adding {unrevealedCell} again to cellsToReveal");
                cellsToReveal.Add(unrevealedCell);
            }

            return isGridChanged;
        }
    }
}
