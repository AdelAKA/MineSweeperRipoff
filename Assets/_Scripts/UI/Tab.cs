using System.Collections.Generic;
using UnityEngine;

namespace MineSweeperRipeoff
{
    public class Tab : MonoBehaviour
    {
        [SerializeField] Canvas mainCanvas; 
        [SerializeField] List<Canvas> subCanvases;

        public virtual void Show()
        {
            mainCanvas.enabled = true;
            foreach (var subCanvas in subCanvases)
            {
                subCanvas.enabled = true;
            }
        }

        public virtual void Hide()
        {
            mainCanvas.enabled = false;
            foreach (var subCanvas in subCanvases)
            {
                subCanvas.enabled = false;
            }
        }
    }
}
