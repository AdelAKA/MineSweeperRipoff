using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_ANDROID
using CandyCoded.HapticFeedback;
#endif

namespace MineSweeperRipeoff
{
    public class FieldCell : MonoBehaviour, IPointerClickHandler
    {
        [Header("Resource")]
        [SerializeField] protected NumbersCollection numbersCollection;
        [SerializeField] public Sprite flagSprite;
        [SerializeField] public Sprite mineSprite;
        [SerializeField] public Sprite startMarkSprite;

        [Header("Settings")]
        [SerializeField] protected Color clickedMineColor;
        [SerializeField] protected Color revealedMineColoer;

        [Header("Cell Body")]
        [SerializeField] public Image ClosedCellImage;
        [SerializeField] public Image OpenedCellImage;

        [Header("Content")]
        [SerializeField] public Image contentImage;

        [Header("Debugging")]
        [SerializeField] public bool isDebug;

        public Cell referencedCell;
        //private bool _isFlagged;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            referencedCell.OnRevealed += Event_UpdateCellContent;
            referencedCell.OnFlagged += Event_UpdateFlagState;
        }

        public void Initialize(Cell referencedCell)
        {
            this.referencedCell = referencedCell;
            ResetCell();
            if (referencedCell.isRevealed)
            {
                ClosedCellImage.enabled = false;
                OpenedCellImage.enabled = true;

                contentImage.enabled = false;
                if (referencedCell.cellType == CellType.Number)
                {
                    contentImage.enabled = true;
                    contentImage.sprite = numbersCollection.GetSpriteOfNumber(referencedCell.number);
                }
            }
            else if (referencedCell.isFlagged)
            {
                contentImage.sprite = flagSprite;
                contentImage.enabled = true;
            }
        }

        private void ResetCell()
        {
            ClosedCellImage.enabled = true;
            OpenedCellImage.enabled = false;

            contentImage.sprite = null;
            contentImage.enabled = false;
            if (referencedCell.isForcePress)
            {
                contentImage.enabled = true;
                contentImage.sprite = startMarkSprite;
            }
        }

        protected virtual void Event_UpdateCellContent(int sequenceCount)
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
#if UNITY_ANDROID
               if (!PlayerData.IsSpeedRunMode) HapticFeedback.MediumFeedback();
#endif
            }
            else if (referencedCell.cellType == CellType.Number)
            {
                contentImage.enabled = true;
                contentImage.sprite = numbersCollection.GetSpriteOfNumber(referencedCell.number);
            }

            if (referencedCell.cellType != CellType.Mine)
            {
                GameManager.Instance.RequestClearParticle(this);
                SoundManager.Instance.PlaySound(ClipName.TileCleared, 1 + 0.1f * sequenceCount + Random.Range(-0.2f, 0.2f));
                //AudioManager.Instance.PlayAudio(ClipName.TileCleared, 1 + 0.1f * sequenceCount);
            }
            else
            {
                GameManager.Instance.RequestBombParticle(this);
                SoundManager.Instance.PlaySound(ClipName.Bomb, 1 + 0.1f * sequenceCount + Random.Range(-0.2f, 0.2f));
                //AudioManager.Instance.PlayAudio(ClipName.Bomb, 1 + 0.1f * sequenceCount);
            }
        }

        protected virtual void Event_UpdateFlagState(bool isFlagged)
        {
            if (isDebug) Debug.Log("Update Flag State");

            if (isFlagged)
            {
                contentImage.sprite = flagSprite;
                contentImage.enabled = true;
                SoundManager.Instance.PlaySound(ClipName.TileFlagged, 1f);
            }
            else
            {
                contentImage.sprite = null;
                contentImage.enabled = false;
                SoundManager.Instance.PlaySound(ClipName.TileFlagged, 0.8f);
            }
        }

        private void TryRevealCell()
        {
            if (referencedCell.isFlagged) return;

            //SoundManager.Instance.PlaySound(ClipName.TileClicked);
            GameManager.Instance.ReavealCell(referencedCell.coordinates);
        }

        private void TryFlagCell()
        {
            if (referencedCell.isRevealed) return;

            //SoundManager.Instance.PlaySound(ClipName.TileClicked);
            GameManager.Instance.UpdateCellFlagState(referencedCell.coordinates);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (GameManager.Instance.CurrentGridState > GridState.Playing) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (GameManager.Instance.CurrentMoveType == MoveType.Clear)
                {
                    if (referencedCell.isFlagged) return;
                    TryRevealCell();
                }
                else
                {
                    TryFlagCell();
                }

#if UNITY_ANDROID
                HapticFeedback.LightFeedback();
#endif
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                TryFlagCell();
            }
        }
    }
}