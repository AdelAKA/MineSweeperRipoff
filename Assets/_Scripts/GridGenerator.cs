using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class GridGenerator
    {
        protected List<Vector2Int> DirectionsList = new List<Vector2Int>()
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

        public GridGenerator()
        {

        }

        protected bool IsWithinRange(Vector2Int gridSize, Vector2Int coordinates)
        {
            return coordinates.x >= 0
                    && coordinates.x < gridSize.x
                    && coordinates.y >= 0
                    && coordinates.y < gridSize.y;
        }

        private void SetMineAndAddCountToSurroundingCells(Cell[,] cells, Vector2Int gridSize, Vector2Int coordinates)
        {
            cells[coordinates.x, coordinates.y].SetAsMine();

            foreach (var direction in DirectionsList)
            {
                Vector2Int targetCoordinates = coordinates + direction;
                if (IsWithinRange(gridSize, targetCoordinates))
                    cells[targetCoordinates.x, targetCoordinates.y].IncreaseCount();
            }
        }

        public Cell[,] GenerateRandomGrid(Vector2Int gridSizeTarget, int numberOfMinesTarget)
        {
            Cell[,] cells = new Cell[gridSizeTarget.x, gridSizeTarget.y];

            for (int i = 0; i < gridSizeTarget.x; i++)
            {
                for (int j = 0; j < gridSizeTarget.y; j++)
                {
                    cells[i, j] = new Cell(new Vector2Int(i, j));
                }
            }

            List<Vector2Int> availableCellsForMines = new List<Vector2Int>();
            for (int i = 0; i < gridSizeTarget.x; i++)
            {
                for (int j = 0; j < gridSizeTarget.y; j++)
                {
                    availableCellsForMines.Add(new Vector2Int(i, j));
                }
            }

            for (int i = 0; i < numberOfMinesTarget; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableCellsForMines.Count);

                SetMineAndAddCountToSurroundingCells(cells, gridSizeTarget, availableCellsForMines[randomIndex]);

                availableCellsForMines.RemoveAt(randomIndex);
            }

            return cells;
        }

        public async Task<(Cell[,], Vector2Int)> GenerateNoChanceGrid(Vector2Int gridSizeTarget, int numberOfMinesTarget)
        {
            GridSolver gridSolver;
            Cell[,] testCells;
            Vector2Int startCell;
            do
            {
                Debug.Log("Testing Grid");
                testCells = GenerateRandomGrid(gridSizeTarget, numberOfMinesTarget);
                Debug.Log(testCells.GetLength(0) + ", " + testCells.GetLength(1));
                gridSolver = new GridSolver(testCells, numberOfMinesTarget);
                startCell = await gridSolver.TrySolve();
            } while (gridSolver.CurrentState != GridState.Cleared);
            // else generate new grid
            Debug.Log($"Found no chance grid starting at {startCell}");

            return (testCells, startCell);
        }
    }
}
