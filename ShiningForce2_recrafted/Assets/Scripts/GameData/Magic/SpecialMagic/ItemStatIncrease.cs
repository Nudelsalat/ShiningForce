using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle;
using UnityEngine;

namespace Assets.Scripts.GameData.Magic.SpecialMagic {
    [CreateAssetMenu(fileName = "ItemStatIncrease", menuName = "Magic/ItemStatIncrease")]
    [Serializable]
    public class ItemStatIncrease : Magic {

        public ItemStatIncrease() {
            MagicType = EnumMagicType.Special;
        }

        public override int ExecuteMagicAtLevel(Unit caster, List<Unit> target, int magicLevel) {
            _dialogManager = DialogManager.Instance;
            _battleCalculator = new BattleCalculator();
            var singleTarget = target.FirstOrDefault().GetCharacter();
            if (singleTarget == null) {
                return 0;
            }
            switch (magicLevel) {
                case 1:
                    singleTarget.CharStats.Hp.AddToBaseValue(4);
                    _dialogManager.EvokeSingleSentenceDialogue($"Max HP of {singleTarget.Name.AddColor(Constants.Orange)} increased by 4!");
                    break;
                case 2:
                    singleTarget.CharStats.Mp.AddToBaseValue(4);
                    _dialogManager.EvokeSingleSentenceDialogue($"Max MP of {singleTarget.Name.AddColor(Constants.Orange)} increased by 4!");
                    break;
                case 3:
                    singleTarget.CharStats.Attack.AddToBaseValue(2);
                    _dialogManager.EvokeSingleSentenceDialogue($"Attack of {singleTarget.Name.AddColor(Constants.Orange)} increased by 2!");
                    break;
                case 4:
                    singleTarget.CharStats.Defense.AddToBaseValue(2);
                    _dialogManager.EvokeSingleSentenceDialogue($"Defense of {singleTarget.Name.AddColor(Constants.Orange)} increased by 2!");
                    break;
                case 5:
                    singleTarget.CharStats.Agility.AddToBaseValue(2);
                    _dialogManager.EvokeSingleSentenceDialogue($"Agility of {singleTarget.Name.AddColor(Constants.Orange)} increased by 2!");
                    break;
                case 6:
                    singleTarget.CharStats.Movement.AddToBaseValue(1);
                    _dialogManager.EvokeSingleSentenceDialogue($"Movement of {singleTarget.Name.AddColor(Constants.Orange)} increased by 1!");
                    break;
            }
            return 0;
        }   
    }
}

