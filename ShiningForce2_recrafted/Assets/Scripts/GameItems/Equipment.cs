using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using Assets.Enums;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Equipment")]
public class Equipment : GameItem {
    public int HpModifier;
    public int MpModifier;
    public int AttackModifier;
    public int DefenseModifier;
    public int AgilityModifier;
    public int MovementModifier;

    public List<EnumClassType> EquipmentForClass;
    public EnumEquipmentType EquipmentType;
    public EnumAttackRange AttackRange;
    public EnumAreaOfEffect AreaOfEffect;
    public EnumChance InflictStatusEffectChance = EnumChance.Never;
    public EnumStatusEffect InflictStatusEffect = EnumStatusEffect.none;
    public Sprite BattleSprite;

    public bool IsEquipped = false;

    public override string GetResourcePath() {
        var equipmentType = EquipmentType == EnumEquipmentType.weapon ? "Weapons" : "Rings";
        return string.Concat("SharedObjects/Items/Equipment/", equipmentType, "/", name.Replace("(Clone)", ""));
    }

    public override List<string> GetDescription() {
        var sentence = base.GetDescription();
        if (HpModifier > 0 || MpModifier > 0 || AttackModifier > 0 || DefenseModifier > 0 || AgilityModifier > 0 ||
            MovementModifier > 0) {
            var increase = new StringBuilder("When worn it increases");
            if (HpModifier > 0) {
                increase.Append($"\nHP by {HpModifier}");
            }
            if (MpModifier > 0) {
                increase.Append($"\nMP by {MpModifier}");
            }
            if (AttackModifier > 0) {
                increase.Append($"\nAttack by {AttackModifier}");
            }
            if (DefenseModifier > 0) {
                increase.Append($"\nDefense by {DefenseModifier}");
            }
            if (AgilityModifier > 0) {
                increase.Append($"\nAgility by {AgilityModifier}");
            }
            if (MovementModifier > 0) {
                increase.Append($"\nAgility by {MovementModifier}");
            }
            sentence.Add(increase.ToString());
        }

        if (HpModifier < 0 || MpModifier < 0 || AttackModifier < 0 || DefenseModifier < 0 || AgilityModifier < 0 ||
            MovementModifier < 0) {
            var decrease = new StringBuilder("When worn it decreases");
            if (HpModifier < 0) {
                decrease.Append($"\nHP by {HpModifier}");
            }
            if (MpModifier < 0) {
                decrease.Append($"\nMP by {MpModifier}");
            }
            if (AttackModifier < 0) {
                decrease.Append($"\nAttack by {AttackModifier}");
            }
            if (DefenseModifier < 0) {
                decrease.Append($"\nDefense by {DefenseModifier}");
            }
            if (AgilityModifier < 0) {
                decrease.Append($"\nAgility by {AgilityModifier}");
            }
            if (MovementModifier < 0) {
                decrease.Append($"\nAgility by {MovementModifier}");
            }
            sentence.Add(decrease.ToString());
        }
        if (InflictStatusEffect != EnumStatusEffect.none) {
            var statusEffects = new StringBuilder("It can inflict:");
            foreach (EnumStatusEffect statusEffect in Enum.GetValues(typeof(EnumStatusEffect))) {
                if (statusEffect != EnumStatusEffect.none && InflictStatusEffect.HasFlag(statusEffect)) {
                    statusEffects.Append(
                        $"\n{Enum.GetName(typeof(EnumStatusEffect), statusEffect).AddColor(Color.gray)}");
                }
            }
        }

        var listOfClasses = EquipmentForClass.Select(x => Enum.GetName(typeof(EnumClassType), x));
        var finalName = string.Join(
            ", ", listOfClasses);
        sentence.Add($"Can be equipped by {finalName}.");
        return sentence;
    }

    public Equipment() {
        EnumItemType = EnumItemType.equipment;
    }
}

