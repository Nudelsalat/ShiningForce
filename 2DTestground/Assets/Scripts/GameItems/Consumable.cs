using System;
using System.Collections.Generic;
using Assets.Scripts.HelperScripts;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "GameItem", menuName = "Inventory/Consumable")]
class Consumable : GameItem {
    public EnumRestorse RestoreType;
    public List<EnumStatusEffect> StatusEffectsToCure;
    public int AmountToRestore;

    private Dialogue _tempDialogue = new Dialogue {
        Name = "Itemtext",
        Sentences = new List<string>() {
            "SENTENCE NOT REPLACED!"
        },
    };

    public Consumable() {
        EnumItemType = EnumItemType.consumable;
    }

    public bool TryUseItem(Character character) {
        var toRestore = 0;
        switch (RestoreType) {
            case EnumRestorse.hp:
                toRestore = character.CharStats.MaxHp - character.CharStats.CurrentHp;
                if (toRestore == 0) {
                    EvokeSingleSentenceDialogue($"{character.Name} does not need any healing!");
                    return false;
                }

                toRestore = toRestore >= AmountToRestore ? AmountToRestore : toRestore;
                character.CharStats.CurrentHp += toRestore;
                EvokeSingleSentenceDialogue($"{character.Name} was healed for {toRestore} points!");
                return true;
            case EnumRestorse.mp:
                toRestore = character.CharStats.MaxMp - character.CharStats.CurrentMp;
                if (toRestore == 0) {
                    EvokeSingleSentenceDialogue($"{character.Name} has full MP!");
                    return false;
                }

                toRestore = toRestore >= AmountToRestore ? AmountToRestore : toRestore;
                character.CharStats.CurrentMp += toRestore;
                EvokeSingleSentenceDialogue($"{character.Name} MP was restored by {toRestore} points!");
                return true;
            case EnumRestorse.both:
                //TODO
                Debug.LogError("THIS IS NOT YET IMPLEMENTED");
                return false;
            case EnumRestorse.statusEffect:
                var itemWasUsed = false;
                foreach (var statusEffect in StatusEffectsToCure) {
                    if (character.StatusEffects.HasFlag(statusEffect)) {
                        character.StatusEffects = character.StatusEffects.Remove(statusEffect);
                        EvokeSingleSentenceDialogue($"{character.Name} is no longer {Enum.GetName(typeof(EnumStatusEffect), statusEffect)}");
                        itemWasUsed = true;
                    }
                }

                if (!itemWasUsed) {
                    EvokeSingleSentenceDialogue($"{character.Name} has no proper status effects that could be cured.");
                }
                return itemWasUsed;
            default:
                return false;
        }
    }

    private void EvokeSingleSentenceDialogue(string sentence) {
        _tempDialogue.Sentences.Clear();
        _tempDialogue.Sentences.Add(sentence);
       FindObjectOfType<DialogManager>().StartDialogue(_tempDialogue);
    }

}

public enum EnumRestorse {
    hp,
    mp,
    statusEffect,
    both
}

