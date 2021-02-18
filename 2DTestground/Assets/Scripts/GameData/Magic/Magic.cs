using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

[CreateAssetMenu(fileName = "Magic", menuName = "Magic/NewMagic")]
public class Magic : ScriptableObject {
    public string SpellName;
    public Sprite SpellSprite;
    public int CurrentLevel = 1;
    public DirectionType PositionInInventory;
    public int MaxLevel = 4; 

    public bool IsEmpty() {
        return SpellName.Equals("empty");
    }
    // TODO: how to deal with different levels? -> Enable flag?
    // MP cost? 
}

