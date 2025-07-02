using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
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

        private void Start()
        {
            CurrentGrid = new GridSolver(delayBetweenMoves, usePredefinedGrid);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CurrentGrid.Initialize(gridSize, minesCount);
                gridSolver = new GridSolver(CurrentGrid, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                gridSolver.TrySolve();
            }
        }
    }
}
