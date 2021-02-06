using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

[CreateAssetMenu(fileName="GameItem", menuName = "Inventory/GameItem")]
public class GameItem : ScriptableObject {
    public string itemName;
    public Sprite ItemSprite;
    public int buyValue;
    public int sellValue;
    public int healValue;
    public DirectionType positionInInventory = DirectionType.none;
    public ItemType itemType = ItemType.none;

    public void SetGameItem(GameItem gameItem, DirectionType direction) {
        if (!gameItem.IsSet()) {
            itemType = ItemType.none;
            itemName = "empty";
            ItemSprite = null;
            buyValue = 0;
            sellValue = 0;
            healValue = 0;
            return;
        }
        itemName = gameItem.itemName;
        ItemSprite = gameItem.ItemSprite;
        buyValue = gameItem.buyValue;
        sellValue = gameItem.sellValue;
        healValue = gameItem.healValue;
        itemType = gameItem.itemType;
        positionInInventory = direction;
    }

    public bool IsSet() {
        return itemType != ItemType.none;
    }
}
