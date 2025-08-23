using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class PatternMatcher1221 : PatternMatcher
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

            // Check if there are two side 1s next to the 2 middle 2s
            bool CheckPatternCondition(Vector2Int targetCell, Vector2Int lookDirection)
            {
                List<int> patternNumbers = new List<int>() { 1, 2, 1 };
                List<int> lineIndexMultiplier = new List<int>() { -1, 1, 2 };

                for (int i = 0; i < patternNumbers.Count; i++)
                {
                    Vector2Int checkCell = targetCell + (lookDirection * lineIndexMultiplier[i]);
                    if (!IsWithinRange(gridSize, checkCell)
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
                    if (IsWithinRange(gridSize, checkCell)
                        && !cells[checkCell.x, checkCell.y].isRevealed
                        && !cells[checkCell.x, checkCell.y].isFlagged)
                    {
                        positiveSideCells.Add(checkCell);
                    }
                    else break;
                }

                // negative perpendicular direction
                Vector2Int negativeSideCell = targetCell - perpendicularDirection;
                List<Vector2Int> negativeSideCells = new List<Vector2Int>();
                foreach (var multiplier in lineIndexMultiplier)
                {
                    Vector2Int checkCell = negativeSideCell + (lookDirection * multiplier);
                    if (IsWithinRange(gridSize, checkCell)
                        && !cells[checkCell.x, checkCell.y].isRevealed
                        && !cells[checkCell.x, checkCell.y].isFlagged)
                    {
                        negativeSideCells.Add(checkCell);
                    }
                    else break;
                }

                safeCells = new List<Vector2Int>();
                mineCells = new List<Vector2Int>();

                // there should only be a total of 4 unrevealed cells for the pattern to match
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

            Vector2Int targetCell = monitoredCell;
            if (trackedNumbers[targetCell.x, targetCell.y] != 2) return false;

            List<Vector2Int> safeCells = new List<Vector2Int>();
            List<Vector2Int> mineCells = new List<Vector2Int>();

            // try to match the pattern vertically or horizontally
            if (CheckPatternCondition(targetCell, Vector2Int.up))
            {
                CheckFor4UnRevealedCellsInARow(targetCell, Vector2Int.up, out safeCells, out mineCells);
            }
            else if (CheckPatternCondition(targetCell, Vector2Int.right))
            {
                CheckFor4UnRevealedCellsInARow(targetCell, Vector2Int.right, out safeCells, out mineCells);
            }

            // no match was found
            if (safeCells.Count == 0) return false;
            //Debug.Log($"Matched 1221 Pattern at {targetCell}");

            safeCells.ForEach(s =>
            {
                //if (cellsToReveal.Contains(s)) Debug.Log($"adding {s} again to cellsToReveal");
                cellsToReveal.Add(s);
            });

            mineCells.ForEach(m =>
            {
                //if (cellsToMark.Contains(m)) Debug.Log($"adding {m} again to cellsToMark");
                cellsToMark.Add(m);
            });
            //if (cellsToUntarget.Contains(targetCell)) Debug.Log($"adding {targetCell} again to cellsToUntarget");
            cellsToUntarget.Add(targetCell);
            isGridChanged = true;

            return isGridChanged;
        }
    }
}
