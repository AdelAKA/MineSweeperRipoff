using UnityEngine;
using System.Collections.Generic;
using System;

namespace MineSweeperRipeoff
{
    [CreateAssetMenu(fileName = "Numbers", menuName = "Scriptable Objects/Numbers")]
    public class NumbersCollection : ScriptableObject
    {
        public List<NumberSpritePair> numbersList;

        [Serializable]
        public struct NumberSpritePair
        {
            public int number;
            public Sprite icon;
        }

        public Sprite GetSpriteOfNumber(int number) => numbersList[number].icon;
    }
}