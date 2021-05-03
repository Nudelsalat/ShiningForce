using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterStatistics {
    public int Level;
    public int Exp;
    public int CurrentHp;
    public int CurrentMp;
    public Stat Hp;
    public Stat Mp;
    public Stat Attack;
    public Stat Defense;
    public Stat Agility;
    public Stat Movement;

    public BaseStats MinLevelStats;
    public BaseStats MaxLevelStats;
    public EnumStatGrowth AttackGrowth;
    public EnumStatGrowth DefenseGrowth;
    public EnumStatGrowth AgilityGrowth;
    public EnumStatGrowth HpGrowth;
    public EnumStatGrowth MpGrowth;

    public int Kills = 0;
    public int Defeats = 0;


    public CharacterStatistics() {
        Level = 1;
        Exp = 0;
        CurrentHp = 10;
        CurrentMp = 0;
        Hp = new Stat(10);
        Mp = new Stat(0);
        Attack = new Stat(5);
        Defense = new Stat(5);
        Agility = new Stat(5);
        Movement = new Stat(5);
    }

    public CharacterStatistics(int level, Stat maxHp, Stat maxMp, Stat attack, Stat defense, Stat agility, Stat movement,
        int exp = 0) {
        Level = level;
        Hp = maxHp;
        Mp = maxMp;
        CurrentHp = Hp.GetModifiedValue();
        CurrentMp = Mp.GetModifiedValue();
        Attack = attack;
        Defense = defense;
        Agility = agility;
        Movement = movement;
        Exp = exp;
    }

    public int MaxHp() {
        return Hp.GetModifiedValue();
    }

    public int MaxMp() {
        return Mp.GetModifiedValue();
    }

    public bool AddExp(int exp) {
        Exp += exp;
        if (Exp < 100) {
            return false;
        }
        Exp -= 100;
        return true;
    }

    public string LevelUp() {
        var sentence = "";

        var attackIncrease = Attack.LevelUp(MinLevelStats.Attack, MaxLevelStats.Attack, Level, AttackGrowth);
        var defenseIncrease = Defense.LevelUp(MinLevelStats.Defense, MaxLevelStats.Defense, Level, DefenseGrowth);
        var agilityIncrease = Agility.LevelUp(MinLevelStats.Agility, MaxLevelStats.Agility, Level, AgilityGrowth);
        var hpIncrease = Hp.LevelUp(MinLevelStats.Hp, MaxLevelStats.Hp, Level, HpGrowth);
        var mpIncrease = Mp.LevelUp(MinLevelStats.Mp, MaxLevelStats.Mp, Level, MpGrowth);

        Level += 1;

        if (attackIncrease > 0) {
            sentence += $"ATTACK increased by {attackIncrease}\n";
        }
        if (defenseIncrease > 0) {
            sentence += $"DEFENSE increased by {defenseIncrease}\n";
        }
        if (agilityIncrease > 0) {
            sentence += $"AGILITY increased by {agilityIncrease}\n";
        }
        if (hpIncrease > 0) {
            sentence += $"HP increased by {hpIncrease}\n";
            CurrentHp += hpIncrease;
        }
        if (mpIncrease > 0) {
            sentence += $"HP increased by {mpIncrease}\n";
            CurrentMp += mpIncrease;
        }

        return sentence;
    }


    public void Equip(Equipment equipment) {
        if (equipment == null) {
            return;
        }
        equipment.IsEquipped = true;
        Hp.AddModifiert(equipment.HpModifier);
        Mp.AddModifiert(equipment.MpModifier);
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
        Hp.RemoveModifier(equipment.HpModifier);
        Mp.RemoveModifier(equipment.MpModifier);
        Attack.RemoveModifier(equipment.AttackModifier);
        Defense.RemoveModifier(equipment.DefenseModifier);
        Agility.RemoveModifier(equipment.AgilityModifier);
        Movement.RemoveModifier(equipment.MovementModifier);
    }


    public int CalculateNewHp(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Hp.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Hp.GetModifiedValue() + newEquipment.HpModifier - oldEquipment.HpModifier;
        }
        return Hp.GetModifiedValue() + newEquipment.HpModifier;
    }

    public int CalculateNewMp(Equipment newEquipment, Equipment oldEquipment) {
        if (newEquipment == null) {
            return Mp.GetModifiedValue();
        }
        if (oldEquipment != null) {
            return Mp.GetModifiedValue() + newEquipment.MpModifier - oldEquipment.MpModifier;
        }
        return Mp.GetModifiedValue() + newEquipment.MpModifier;
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

    public void ClearModifiers() {
        Hp.ClearModifier();
        Mp.ClearModifier();
        Movement.ClearModifier();
        Attack.ClearModifier();
        Defense.ClearModifier();
        Agility.ClearModifier();
    }
}
