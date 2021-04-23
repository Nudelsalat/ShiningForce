using System;
using Assets.Scripts.GameData;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

[CreateAssetMenu(fileName="GameItem", menuName = "Inventory/GameItem")]
public class GameItem : ScriptableObject {
    public string ItemName;
    public int Price;
    public bool IsUnique = false;
    public Sprite ItemSprite;
    public DirectionType PositionInInventory = DirectionType.none;
    public EnumItemType EnumItemType = EnumItemType.none;

    public bool IsEmpty() {
        return EnumItemType == EnumItemType.none;
    }

    protected GameItem() {

    }

    public virtual string GetResourcePath() {
        return Constants.ItemEmptyItem;
    }
}
