﻿using System;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

namespace Assets.Scripts.GameData.Magic {
    [CreateAssetMenu(fileName = "Magic", menuName = "Magic/NewMagic")]
    [Serializable]
    public class Magic : ScriptableObject {
        public string SpellName;
        public Sprite SpellSprite;
        public int CurrentLevel = 1;
        public DirectionType PositionInInventory;
        public int MaxLevel = 4; 

        public bool IsEmpty() {
            return SpellName.Equals("");
        }
        // TODO: how to deal with different levels? -> Enable flag?
        // MP cost? 
    }
}

