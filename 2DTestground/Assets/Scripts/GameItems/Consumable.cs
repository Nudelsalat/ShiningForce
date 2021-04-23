using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.HelperScripts;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Consumable")]
class Consumable : GameItem {
    public Magic Magic;
    public int MagicLevel;

    private DialogManager _dialogManager;


    public Consumable() {
        EnumItemType = EnumItemType.consumable;
    }

    public override string GetResourcePath() {
        return string.Concat("SharedObjects/Items/Consumables/", name.Replace("(Clone)",""));
    }

    public bool TryUseItem(Character user, Character usedOnCharacter) {
        _dialogManager = DialogManager.Instance;
        var toRestore = 0;
        var amountToRestore = Magic.Damage[MagicLevel - 1];
        switch (Magic.MagicType) {
            case EnumMagicType.Heal:
                toRestore = usedOnCharacter.CharStats.MaxHp - usedOnCharacter.CharStats.CurrentHp;
                if (toRestore == 0) {
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{usedOnCharacter.Name.AddColor(Constants.Orange)} does not need any healing!");
                    return false;
                }
                toRestore = toRestore >= amountToRestore ? amountToRestore : toRestore;
                usedOnCharacter.CharStats.CurrentHp += toRestore;
                _dialogManager.EvokeSingleSentenceDialogue(
                    $"{usedOnCharacter.Name.AddColor(Constants.Orange)} was healed for {toRestore} points!");
                return true;
            case EnumMagicType.RestoreMP:
                toRestore = usedOnCharacter.CharStats.MaxMp - usedOnCharacter.CharStats.CurrentMp;
                if (toRestore == 0) {
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{usedOnCharacter.Name.AddColor(Constants.Orange)} has full MP!");
                    return false;
                }

                toRestore = toRestore >= amountToRestore ? amountToRestore : toRestore;
                usedOnCharacter.CharStats.CurrentMp += toRestore;
                _dialogManager.EvokeSingleSentenceDialogue($"{usedOnCharacter.Name.AddColor(Constants.Orange)} MP was restored by {toRestore} points!");
                return true;
            case EnumMagicType.RestoreBoth:
                if (usedOnCharacter.CharStats.MaxHp == usedOnCharacter.CharStats.CurrentHp &&
                    usedOnCharacter.CharStats.MaxMp == usedOnCharacter.CharStats.CurrentMp) {
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{usedOnCharacter.Name.AddColor(Constants.Orange)} does not need any healing!");
                    return false;
                }
                toRestore = toRestore >= amountToRestore ? amountToRestore : toRestore;
                usedOnCharacter.CharStats.CurrentHp += toRestore;
                _dialogManager.EvokeSingleSentenceDialogue(
                    $"{usedOnCharacter.Name.AddColor(Constants.Orange)} was healed for {toRestore} points!");

                toRestore = toRestore >= amountToRestore ? amountToRestore : toRestore;
                usedOnCharacter.CharStats.CurrentMp += toRestore;
                _dialogManager.EvokeSingleSentenceDialogue($"{usedOnCharacter.Name.AddColor(Constants.Orange)} MP was restored by {toRestore} points!");
                return true;
            case EnumMagicType.Cure:
                var itemWasUsed = false;
                var statusEffectsToCure = new List<EnumStatusEffect>();
                switch (MagicLevel) {
                    case 1:
                        statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                        break;
                    case 2:
                        statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                        statusEffectsToCure.Add(EnumStatusEffect.sleep);
                        break;
                    case 3:
                        statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                        statusEffectsToCure.Add(EnumStatusEffect.sleep);
                        statusEffectsToCure.Add(EnumStatusEffect.confused);
                        break;
                    case 4:
                        statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                        statusEffectsToCure.Add(EnumStatusEffect.sleep);
                        statusEffectsToCure.Add(EnumStatusEffect.confused);
                        statusEffectsToCure.Add(EnumStatusEffect.paralyzed);
                        break;
                }
                foreach (var statusEffect in statusEffectsToCure) {
                    if (usedOnCharacter.StatusEffects.HasFlag(statusEffect)) {
                        usedOnCharacter.StatusEffects = usedOnCharacter.StatusEffects.Remove(statusEffect);
                        _dialogManager.EvokeSingleSentenceDialogue(
                            $"{usedOnCharacter.Name.AddColor(Constants.Orange)} is no longer " +
                            $"{Enum.GetName(typeof(EnumStatusEffect), statusEffect).AddColor(Color.gray)}");
                        itemWasUsed = true;
                    }
                }

                if (!itemWasUsed) {
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{usedOnCharacter.Name.AddColor(Constants.Orange)} " +
                        $"has no proper status effects that could be cured.");
                }
                return itemWasUsed;
            case EnumMagicType.Special:
                Magic.ExecuteMagicAtLevel(user, usedOnCharacter, MagicLevel);
                return true;
            default:
                return false;
        }
    }
}

