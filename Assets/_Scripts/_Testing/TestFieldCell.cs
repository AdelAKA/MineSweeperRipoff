using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MineSweeperRipeoff
{
    public class TestFieldCell : FieldCell
    {
        // Start is called before the first frame update
        protected override void Event_UpdateCellContent(int sequenceCount)
        {
            if (isDebug) Debug.Log("Update Cell Content");
            ClosedCellImage.enabled = false;
            OpenedCellImage.enabled = true;

            contentImage.enabled = false;
            if (referencedCell.cellType == CellType.Mine)
            {
                contentImage.enabled = true;
                contentImage.sprite = mineSprite;
                OpenedCellImage.color = referencedCell.isLosingCell ? clickedMineColor : revealedMineColoer;
#if !UNITY_EDITOR && !PLATFORM_STANDALONE_WIN
               if (!PlayerData.IsSpeedRunMode) HapticFeedback.MediumFeedback();
#endif
            }
            else if (referencedCell.cellType == CellType.Number)
            {
                contentImage.enabled = true;
                contentImage.sprite = numbersCollection.GetSpriteOfNumber(referencedCell.number);
            }
        }

        protected override void Event_UpdateFlagState(bool isFlagged)
        {
            if (isDebug) Debug.Log("Update Flag State");

            if (isFlagged)
            {
                contentImage.sprite = flagSprite;
                contentImage.enabled = true;
            }
            else
            {
                contentImage.sprite = null;
                contentImage.enabled = false;
            }
        }
        public override void OnPointerClick(PointerEventData eventData) { }
    }
}
