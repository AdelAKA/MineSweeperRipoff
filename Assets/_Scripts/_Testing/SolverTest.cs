using UnityEngine;

namespace MineSweeperRipeoff.Test
{
    public class SolverTest : MonoBehaviour
    {
        [SerializeField] FieldGrid fieldGrid;
        [SerializeField] Vector2Int gridSize;
        [SerializeField] int minesCount;
        [SerializeField] bool usePredefinedGrid;
        [SerializeField] float delayBetweenMoves;

        private Grid CurrentGrid;
        [SerializeField] GridSolver gridSolver;
        [SerializeField] OptimizedGridSolver gridSolver2;

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if(usePredefinedGrid)
                    CurrentGrid = new GridSolver(delayBetweenMoves);
                else
                    CurrentGrid = new GridSolver(gridSize, minesCount, GameMode.Chance, true);
                gridSolver = new GridSolver(CurrentGrid.GetCopyOfCells(), CurrentGrid.numberOfMines, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver);
            }


            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if(usePredefinedGrid)
                    CurrentGrid = new OptimizedGridSolver(delayBetweenMoves);
                else
                    CurrentGrid = new OptimizedGridSolver(gridSize, minesCount, GameMode.Chance, true);
                gridSolver2 = new OptimizedGridSolver(CurrentGrid.GetCopyOfCells(), CurrentGrid.numberOfMines, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver2);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                UnityEngine.Debug.Log("started");
                gridSolver.TrySolve();
                UnityEngine.Debug.Log(gridSolver2.CurrentState);
                UnityEngine.Debug.Log("ended");
            }


            if (Input.GetKeyDown(KeyCode.W))
            {
                UnityEngine.Debug.Log("started");
                gridSolver2.TrySolve();
                UnityEngine.Debug.Log(gridSolver2.CurrentState);
                UnityEngine.Debug.Log("ended");
            }
        }
    }
}
