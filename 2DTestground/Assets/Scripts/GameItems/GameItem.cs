using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameItem {
    public enum ItemType {
        Consumable,
        NoneConsumable,
        Ring,
        Gauntlet,
        Axe,
        Arrow,
        Sword,
        Spear,
        Lance,
        Rod,
        Knife
    }

    public ItemType itemType;
    public string itemName;
    public Sprite itemSprite;

}

