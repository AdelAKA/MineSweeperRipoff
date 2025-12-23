using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace MineSweeperRipeoff
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; set; }

        [SerializeField] Canvas canvas;
        [Header("Animatoin Components")]
        [SerializeField] List<Image> closedMainCells;
        [SerializeField] List<Image> openedMainCells;
        [SerializeField] List<Image> mainCellsContent;
        [SerializeField] List<Image> otherCells;

        Sequence sequence;

        UnityAction currentCompleteAction;
        UnityAction currentPlayAction;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);

            InitialState();
            RandomizeCellsOrder();
            SetUp();
        }

        private void RandomizeCellsOrder()
        {
            Random.InitState(101);

            List<Image> randomizedList = new List<Image>();
            int numberOfIteations = otherCells.Count;
            for (int i = 0; i < numberOfIteations; i++)
            {
                int randomIndex = Random.Range(0, otherCells.Count);
                randomizedList.Add(otherCells[randomIndex]);
                otherCells.Remove(otherCells[randomIndex]);
            }
            otherCells = randomizedList;
            Random.InitState((int)System.DateTime.Now.Ticks);
        }

        private void SetUp()
        {
            sequence = DOTween.Sequence();
            sequence.SetAutoKill(false);

            sequence.OnPlay(() => currentPlayAction?.Invoke());
            for (int i = 0; i < otherCells.Count; i++)
            {
                sequence.Insert(0.02f * i, otherCells[i].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            }
            sequence.Append(closedMainCells[0].transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack));
            sequence.Join(openedMainCells[0].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.Join(mainCellsContent[0].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));

            sequence.Append(closedMainCells[1].transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack));
            sequence.Join(openedMainCells[1].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.Join(mainCellsContent[1].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));

            sequence.Append(closedMainCells[2].transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack));
            sequence.Join(openedMainCells[2].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.Join(mainCellsContent[2].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));

            sequence.Append(closedMainCells[3].transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 8, 0.2f));
            sequence.Append(mainCellsContent[3].transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.OnComplete(() => currentCompleteAction?.Invoke());

            sequence.Pause();
        }

        public async Task Show()
        {
            while (sequence.IsPlaying() && !sequence.IsComplete()) await Awaitable.NextFrameAsync();

            currentPlayAction = () => canvas.enabled = true;
            currentCompleteAction = null;

            sequence.PlayForward();
            await Awaitable.WaitForSecondsAsync(sequence.Duration());

        }

        public async Task Hide()
        {
            while (sequence.IsPlaying() && !sequence.IsComplete()) await Awaitable.NextFrameAsync();

            currentPlayAction = null;
            currentCompleteAction = () => canvas.enabled = false;

            sequence.PlayBackwards();
            await Awaitable.WaitForSecondsAsync(sequence.Duration());

            canvas.enabled = false;
        }

        public void InitialState()
        {
            //_showSequence.Complete();

            canvas.enabled = false;
            foreach (var cell in closedMainCells)
            {
                cell.transform.localScale = Vector3.zero;
            }
            foreach (var cell in openedMainCells)
            {
                cell.transform.localScale = Vector3.zero;
            }
            foreach (var content in mainCellsContent)
            {
                content.transform.localScale = Vector3.zero;
            }
            foreach (var cell in otherCells)
            {
                cell.transform.localScale = Vector3.zero;
            }
        }
    }
}
