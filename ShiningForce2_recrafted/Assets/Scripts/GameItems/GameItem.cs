using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameData;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

[CreateAssetMenu(fileName="GameItem", menuName = "Inventory/GameItem")]
public class GameItem : ScriptableObject {
    public string ItemName;
    public int Price;
    public bool IsUnique = false;
    public Sprite ItemSprite;
    public string Description = "";
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

    public virtual List<string> GetDescription() {
        var sentence = new List<string>();
        sentence.Add(string.IsNullOrEmpty(Description)
            ? "Unknown what this item does..."
            : Description);
        sentence.Add($"Item can be sold for { Price / 2} Gold.");
        return sentence;
    }
}
