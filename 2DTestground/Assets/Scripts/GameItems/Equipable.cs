using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Equipable")]
class Equipable : GameItem {
    public int AttackModifier;
    public int DefenseModifier;
    public int AgilityModifier;
    public int MovementModifier;

    public List<EnumClassType> EquipableByClasses;

    public Equipable() {
        itemType = ItemType.equipment;
    }
}

