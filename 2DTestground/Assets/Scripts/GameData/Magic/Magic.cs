using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Magic : ScriptableObject {
    public string SpellName;
    public Sprite SpellSprite;
    public int CurrentLevel = 1;
    [SerializeField]
    private int _maxLevel = 4;

    // TODO: how to deal with different levels? -> Enable flag?
    // MP cost? 
}

