using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace MineSweeperRipeoff
{
    public class GridSaveData
    {
        public float timer;
        public Vector2Int gridSize;
        public int numberOfMines;
        public int remainingMines;
        public int numberOfRevealedCells;
        public List<Cell> cells;
    }

    public static class GridDataSaver
    {
        public static bool SavedGridExists(string fileName) => File.Exists($"{Application.persistentDataPath}/{fileName}.json");
        public static void DeleteSave(string fileName) => File.Delete($"{Application.persistentDataPath}/{fileName}.json");

        public static void SaveGrid(string fileName, GridSaveData gridToSave)
        {
            string gridFile = JsonUtility.ToJson(gridToSave);
            File.WriteAllText($"{Application.persistentDataPath}/{fileName}.json" , gridFile);
        }

        public static GridSaveData TryLoadGrid(string fileName)
        {
            string gridFile = File.ReadAllText($"{Application.persistentDataPath}/{fileName}.json");
            if (string.IsNullOrEmpty(gridFile)) return null;

            GridSaveData gridData = JsonUtility.FromJson<GridSaveData>(gridFile);
            return gridData;
        }
    }
}
