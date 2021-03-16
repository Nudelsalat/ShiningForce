using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Equipment")]
public class Equipment : GameItem {
    public int AttackModifier;
    public int DefenseModifier;
    public int AgilityModifier;
    public int MovementModifier;

    public List<EnumClassType> EquipmentForClass;
    public EnumEquipmentType EquipmentType;

    public bool IsEquipped = false;

    public override string GetResourcePath() {
        var equipmentType = EquipmentType == EnumEquipmentType.weapon ? "Weapons" : "Rings";
        return string.Concat("SharedObjects/Items/Equipment/", equipmentType, "/", name.Replace("(Clone)", ""));
    }

    public Equipment() {
        EnumItemType = EnumItemType.equipment;
    }
}

