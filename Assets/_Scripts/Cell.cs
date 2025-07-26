using System;
using UnityEngine;
using UnityEngine.Events;

namespace MineSweeperRipeoff
{
    [Serializable]
    public class Cell
    {
        public CellType cellType;
        public bool isRevealed;
        public bool isFlagged;
        public bool isForcePress;
        public int number;

        public UnityAction<int> OnRevealed;
        public UnityAction<bool> OnFlagged;

        public Vector2Int coordinates;
        public bool isLosingCell;

        public Cell(Cell copy)
        {
            this.coordinates = copy.coordinates;
            this.cellType = copy.cellType;
            this.isRevealed = copy.isRevealed;
            this.isFlagged = copy.isFlagged;
            this.number = copy.number;
            this.isLosingCell = copy.isLosingCell;
        }

        public Cell(Vector2Int coordinates)
        {
            this.coordinates = coordinates;
            cellType = CellType.Empty;
            isRevealed = false;
            isFlagged = false;
            number = 0;
            isLosingCell = false;
        }

        public void SetFlagState(bool isFlagged)
        {
            this.isFlagged = isFlagged;
            OnFlagged?.Invoke(isFlagged);
        }

        public void RevealCell(int sequenceCount)
        {
            isRevealed = true;
            OnRevealed?.Invoke(sequenceCount);
        }

        public void SetAsMine()
        {
            cellType = CellType.Mine;
        }

        public void ClearMine()
        {
            cellType = number == 0 ? CellType.Empty : CellType.Number;
        }

        public void MarkAsForceStartCell()
        {
            isForcePress = true;
        }

        public void IncreaseCount()
        {
            number++;
            if (cellType == CellType.Mine) return;
            cellType = CellType.Number;
        }

        public void DecreaseCount()
        {
            number--;
            if (cellType == CellType.Mine) return;
            if (number == 0)
                cellType = CellType.Empty;

        }
    }
}