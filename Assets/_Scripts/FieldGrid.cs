using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class FieldGrid : MonoBehaviour
    {
        [Header("Game Panel")]
        [SerializeField] FieldCell fieldCellPrefab;
        [SerializeField] Transform cellContainer;
        [SerializeField] GridLayoutGroup gridLayout;

        public void Initialize(Grid targetGrid)
        {
            foreach (Transform item in cellContainer)
            {
                Destroy(item.gameObject);
            }

            gridLayout.constraintCount = targetGrid.gridSize.y;

            for (int i = 0; i < targetGrid.gridSize.x; i++)
            {
                for (int j = 0; j < targetGrid.gridSize.y; j++)
                {
                    FieldCell newFieldCell = Instantiate(fieldCellPrefab, cellContainer);
                    newFieldCell.Initialize(targetGrid.GetCell(new Vector2Int(i, j)));
                }
            }
        }

    }
}