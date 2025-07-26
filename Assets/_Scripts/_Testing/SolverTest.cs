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

        private void Start()
        {
            //CurrentGrid = new GridSolver(delayBetweenMoves, usePredefinedGrid);
            CurrentGrid = new Grid();
        }

        private async void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CurrentGrid.Initialize(gridSize, minesCount, GameMode.NoChance);
                gridSolver = new GridSolver(CurrentGrid, delayBetweenMoves);

                fieldGrid.Initialize(gridSolver);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                UnityEngine.Debug.Log("started");
                await gridSolver.TrySolve();
                UnityEngine.Debug.Log(gridSolver.CurrentState);
                UnityEngine.Debug.Log("ended");
            }
        }
    }
}
