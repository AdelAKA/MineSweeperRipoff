using System;
using UnityEngine;
using System.Collections.Generic;

namespace MineSweeperRipeoff
{
    public class TabsManager : MonoBehaviour
    {
        [Serializable]
        public struct TabNamePair { public TabName tabName; public Tab tabReference;}

        public static TabsManager Instance { get; set; }

        [SerializeField] List<TabNamePair> tabNamePairs;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void ShowTab(TabName target)
        {
            foreach(TabNamePair pair in tabNamePairs)
            {
                if(pair.tabName == target)
                    pair.tabReference.Show();
                else
                    pair.tabReference.Hide();
            }
        }
    }
}
