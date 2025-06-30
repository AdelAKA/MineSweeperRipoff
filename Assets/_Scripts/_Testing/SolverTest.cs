using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class SolverTest : MonoBehaviour
    {
        [SerializeField] FieldGrid fieldGrid;

        private Grid CurrentGrid = new Grid();
        [SerializeField] GridSolver gridSolver = new GridSolver();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CurrentGrid.Initialize(new Vector2Int(8, 8), 8);
                gridSolver = new GridSolver(CurrentGrid);
                fieldGrid.Initialize(gridSolver);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                gridSolver.TrySolve();
            }
        }
    }
}
