﻿using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

[CreateAssetMenu(fileName="GameItem", menuName = "Inventory/GameItem")]
public class GameItem : ScriptableObject {
    public string itemName;
    public Sprite ItemSprite;
    public int buyValue;
    public DirectionType positionInInventory = DirectionType.none;
    public EnumItemType EnumItemType = EnumItemType.none;

    public bool IsSet() {
        return EnumItemType != EnumItemType.none;
    }
}
