using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public abstract class PatternMatcher
    {
        protected static readonly List<Vector2Int> DirectionsList = new List<Vector2Int>()
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

        public abstract bool TryMatch(
                Cell[,] cells,
                Vector2Int gridSize,
                int[,] trackedNumbers,
                bool[,] predictedMines,
                Vector2Int monitoredCell,
                HashSet<Vector2Int> cellsToReveal,
                HashSet<Vector2Int> cellsToMark,
                HashSet<Vector2Int> cellsToUntarget);

        protected bool IsWithinRange(Vector2Int gridSize, Vector2Int coordinates)
        {
            return coordinates.x >= 0
                    && coordinates.x < gridSize.x
                    && coordinates.y >= 0
                    && coordinates.y < gridSize.y;
        }

        protected List<Vector2Int> GetSurroundingUnPredictedCells(Cell[,] cells, Vector2Int gridSize, Vector2Int targetCell)
        {
            List<Vector2Int> unPredictedCells = new List<Vector2Int>();
            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = targetCell + direction;
                if (IsWithinRange(gridSize, targetCoordinates)
                && !cells[targetCoordinates.x, targetCoordinates.y].isRevealed
                && !cells[targetCoordinates.x, targetCoordinates.y].isFlagged)
                    unPredictedCells.Add(targetCoordinates);
            }
            return unPredictedCells;
        }
    }
}
