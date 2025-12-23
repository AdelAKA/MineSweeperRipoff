using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        [SerializeField] OptimizedGridSolver gridSolver2;


        private void Start()
        {
            //CurrentGrid = new Grid();
        }

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                CurrentGrid = new OptimizedGridSolver(delayBetweenMoves, usePredefinedGrid);

                CurrentGrid.Initialize(gridSize, minesCount, GameMode.NoChance, true);
                gridSolver2 = new OptimizedGridSolver(CurrentGrid.GetCopyOfCells(), CurrentGrid.numberOfMines, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CurrentGrid = new GridSolver(delayBetweenMoves, usePredefinedGrid);

                CurrentGrid.Initialize(gridSize, minesCount, GameMode.NoChance, true);
                gridSolver = new GridSolver(CurrentGrid.GetCopyOfCells(), CurrentGrid.numberOfMines, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                UnityEngine.Debug.Log("started");
                await gridSolver2.TrySolve();
                UnityEngine.Debug.Log(gridSolver2.CurrentState);
                UnityEngine.Debug.Log("ended");
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UnityEngine.Debug.Log("started");
                await gridSolver.TrySolve();
                UnityEngine.Debug.Log(gridSolver2.CurrentState);
                UnityEngine.Debug.Log("ended");
            }
        }
    }
}
