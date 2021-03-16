using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterStatistics {
    public int Level;
    public int Exp;
    public int MaxHp;
    public int CurrentHp;
    public int MaxMp;
    public int CurrentMp;
    public Stat Attack;
    public Stat Defense;
    public Stat Agility;
    public Stat Movement;
    public int Kills = 0;
    public int Defeats = 0;


    public CharacterStatistics() {
        Level = 1;
        Exp = 0;
        MaxHp = CurrentHp = 10;
        MaxMp = CurrentMp = 0;
        Attack = new Stat(5);
        Defense = new Stat(5);
        Agility = new Stat(5);
        Movement = new Stat(5);
    }

    public CharacterStatistics(int level, int maxHp, int maxMp, Stat attack, Stat defense, Stat agility, Stat movement,
        int exp = 0) {
        Level = level;
        MaxHp = CurrentHp = maxHp;
        MaxMp = CurrentMp = maxMp;
        Attack = attack;
        Defense = defense;
        Agility = agility;
        Movement = movement;
        Exp = exp;
    }

    public void Equip(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        equipment.IsEquipped = true;
        Attack.AddModifiert(equipment.AttackModifier);
        Defense.AddModifiert(equipment.DefenseModifier);
        Agility.AddModifiert(equipment.AgilityModifier);
        Movement.AddModifiert(equipment.MovementModifier);
    }

    public void UnEquip(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        equipment.IsEquipped = false;
        Attack.RemoveModifier(equipment.AttackModifier);
        Defense.RemoveModifier(equipment.DefenseModifier);
        Agility.RemoveModifier(equipment.AgilityModifier);
        Movement.RemoveModifier(equipment.MovementModifier);
    }

    public int CalculateNewAttack(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Attack.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Attack.GetModifiedValue() + newEquipment.AttackModifier - oldEquipment.AttackModifier;
        }
        return Attack.GetModifiedValue() + newEquipment.AttackModifier;
    }
    public int CalculateNewDefense(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Defense.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Defense.GetModifiedValue() + newEquipment.DefenseModifier - oldEquipment.DefenseModifier;
        }
        return Defense.GetModifiedValue() + newEquipment.DefenseModifier;
    }
    public int CalculateNewAgility(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Agility.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Agility.GetModifiedValue() + newEquipment.AgilityModifier - oldEquipment.AgilityModifier;
        }
        return Agility.GetModifiedValue() + newEquipment.AgilityModifier;
    }
    public int CalculateNewMovement(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Movement.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Movement.GetModifiedValue() + newEquipment.MovementModifier - oldEquipment.MovementModifier;
        }
        return Movement.GetModifiedValue() + newEquipment.MovementModifier;
    }

    public void AddKill() {
        Kills += 1;
    }

    public void AddDefeat() {
        Defeats += 1;
    }
}
