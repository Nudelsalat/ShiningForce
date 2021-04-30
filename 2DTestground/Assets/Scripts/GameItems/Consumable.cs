using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.GameData.Magic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Consumable")]
class Consumable : GameItem {
    public Magic Magic;
    public int MagicLevel;


    public Consumable() {
        EnumItemType = EnumItemType.consumable;
    }

    public override string GetResourcePath() {
        return string.Concat("SharedObjects/Items/Consumables/", name.Replace("(Clone)",""));
    }

    public bool TryUseItem(Unit user, List<Unit> usedOnCharacter, bool usedInBattle = true) {
        if (!CheckIfUsefull(usedOnCharacter, usedInBattle)) {
            return false;
        }
        Magic.ExecuteMagicAtLevel(user, usedOnCharacter, MagicLevel);
        return true;
    }

    private bool CheckIfUsefull(List<Unit> targets, bool useInBattle = true) {
        var dialogManager = DialogManager.Instance;
        var canBeUsed = false;
        var sentences = new List<string>();
        foreach (var target in targets) {
            var usedOnCharacter = target.GetCharacter();
            var toRestore = 0;
            switch (Magic.MagicType) {
                case EnumMagicType.Heal:
                    toRestore = usedOnCharacter.CharStats.MaxHp() - usedOnCharacter.CharStats.CurrentHp;
                    if (toRestore == 0) {
                        sentences.Add($"{usedOnCharacter.Name.AddColor(Constants.Orange)} does not need any healing!");
                    } else {
                        canBeUsed = true;
                    }
                    break;
                case EnumMagicType.RestoreMP:
                    toRestore = usedOnCharacter.CharStats.MaxMp() - usedOnCharacter.CharStats.CurrentMp;
                    if (toRestore == 0) {
                        sentences.Add($"{usedOnCharacter.Name.AddColor(Constants.Orange)} has full MP!");
                    }
                    else {
                        canBeUsed = true;
                    }

                    break;
                case EnumMagicType.RestoreBoth:
                    if (usedOnCharacter.CharStats.MaxHp() == usedOnCharacter.CharStats.CurrentHp &&
                        usedOnCharacter.CharStats.MaxMp() == usedOnCharacter.CharStats.CurrentMp) {
                        sentences.Add($"{usedOnCharacter.Name.AddColor(Constants.Orange)} does not need any healing!");
                        return false;
                    }
                    else {
                        canBeUsed = true;
                    }

                    break;
                case EnumMagicType.Cure:
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
                            canBeUsed = true;
                        }
                    }

                    if (!canBeUsed) {
                        sentences.Add($"{usedOnCharacter.Name.AddColor(Constants.Orange)} " +
                            $"has no proper status effects that could be cured.");
                    }

                    break;
                case EnumMagicType.Special:
                    return true;
                case EnumMagicType.Damage:
                    if (!useInBattle) {
                        sentences.Add($"This item can only be used during combat.");
                    }
                    canBeUsed = useInBattle;
                    break;
                default:
                    return false;
            }
        }

        if (!canBeUsed) {
            dialogManager.EvokeSentenceDialogue(sentences);
        }
        return canBeUsed;
    }
}

