using System;
using Assets.Enums;
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
        public EnumAttackRange[] AttackRange = new EnumAttackRange[4];
        public EnumAreaOfEffect[] AreaOfEffect = new EnumAreaOfEffect[4];
        public EnumMagicType MagicType;
        public EnumElementType ElementType;
        public int[] Damage = new int[4];
        public int[] ManaCost = new int[4];

        public bool IsEmpty() {
            return SpellName.Equals("");
        }
        // TODO: how to deal with different levels? -> Enable flag?
        // MP cost? 
    }
}

