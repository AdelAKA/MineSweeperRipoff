using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MineSweeperRipeoff
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Serializable]
        public class DifficultyOptions { public DifficultyLevel difficulty; public Vector2Int gridSize; public int minesCount; }

        private static readonly Vector2Int DEFAULT_SIZE = new Vector2Int(8, 8);
        private static readonly int DEFAULT_MINES_COUNT = 8;

        public List<DifficultyOptions> difficultyOptionsList;

        public (Vector2Int, int) GetOptionsForDifficulty(DifficultyLevel difficulty)
        {
            var result = difficultyOptionsList.FirstOrDefault(x => x.difficulty == difficulty);
            if (result.gridSize == Vector2Int.zero) return (DEFAULT_SIZE, DEFAULT_MINES_COUNT);
            return (result.gridSize, result.minesCount);
        }
    }
}