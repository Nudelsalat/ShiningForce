using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEngine;

namespace Assets.Scripts.GameData.Magic {
    [CreateAssetMenu(fileName = "Magic", menuName = "Magic/NewMagic")]
    [Serializable]
    public class Magic : ScriptableObject {
        public string SpellName;
        public Sprite SpellSprite;
        public int CurrentLevel = 1;
        public DirectionType PositionInInventory;
        public EnumAttackRange[] AttackRange = new EnumAttackRange[4];
        public EnumAreaOfEffect[] AreaOfEffect = new EnumAreaOfEffect[4];
        public EnumMagicType MagicType;
        public EnumElementType ElementType;
        public int[] Damage = new int[4];
        public int[] ManaCost = new int[4];
        public RuntimeAnimatorController[] SpellAnimatorController = new RuntimeAnimatorController[4];

        protected DialogManager _dialogManager;
        protected BattleCalculator _battleCalculator;

        public bool IsEmpty() {
            return SpellName.Equals("");
        }

        public virtual int ExecuteMagicAtLevel(Unit casterUnit, List<Unit> targets, int magicLevel) {
            _dialogManager = DialogManager.Instance;
            _battleCalculator = new BattleCalculator();
            var caster = casterUnit.GetCharacter();
            var damage = Damage[magicLevel-1];
            damage = casterUnit.GetCharacter().IsPromoted ? (int)(damage * 1.25) : damage;
            var expScore = 0;
            var sentence = new List<string>();
            foreach (var targetUnit in targets) {
                var target = targetUnit.GetCharacter();
                var levelDifference = caster.CharStats.Level -
                                      target.CharStats.Level;
                var expBase = levelDifference <= 0 ? 50 : levelDifference >= 5 ? 0 : 50 - 10 * levelDifference;

                var isCrit = _battleCalculator.RollForCrit(caster);
                var critString = "";
                //TODO is elemental resistance vulnerability?
                if (isCrit) {
                    damage = _battleCalculator.GetCritDamage(caster, damage);
                    critString = "Critical hit!\n";
                }
                switch (MagicType) {
                    case EnumMagicType.Damage:
                        sentence.Add(
                            $"{critString}{target.Name.AddColor(Constants.Orange)} suffered {damage} " +
                            $"points of damage.");
                        if (target.CharStats.CurrentHp <= damage) {
                            sentence.Add($"{target.Name.AddColor(Constants.Orange)}" +
                                         $" was defeated!");
                            target.CharStats.CurrentHp = 0;
                            BattleController.Instance.RemoveUnitFromBattle(targetUnit);
                            if (targetUnit is EnemyUnit enemyUnit) {
                                var inventory = Inventory.Instance;
                                inventory.AddGold(enemyUnit.GetGoldDrop());
                                var itemDrop = enemyUnit.GetDropItem();
                                if (itemDrop != null) {
                                    sentence.Add($"{caster.Name.AddColor(Constants.Orange)} found {itemDrop.ItemName.AddColor(Color.green)}");
                                    if (!caster.TryAddItem(itemDrop)) {
                                        inventory.AddToBackBag(itemDrop);
                                        sentence.Add($"{itemDrop.ItemName.AddColor(Color.green)} was added to the back bag");
                                    }
                                }
                            }
                            expScore += (int)((expBase * (float)damage / target.CharStats.MaxHp()) +
                                              expBase);
                        } else {
                            target.CharStats.CurrentHp -= damage;
                            expScore += (int)(expBase * (float)damage / target.CharStats.MaxHp());
                        }

                        break;
                    case EnumMagicType.Heal:
                        var diff = target.CharStats.MaxHp() -
                                   target.CharStats.CurrentHp;
                        var pointsToHeal = diff < damage ? diff : damage;

                        target.CharStats.CurrentHp += pointsToHeal;
                        sentence.Add(
                            $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                            $"points.");
                        var expPoints = 25 * (float)pointsToHeal / target.CharStats.MaxHp();
                        expScore += (int) expPoints;
                        break;
                    case EnumMagicType.RestoreMP:
                        diff = target.CharStats.MaxMp() -
                                   target.CharStats.CurrentMp;
                        pointsToHeal = diff < damage ? diff : damage;
                        target.CharStats.CurrentMp += damage;
                        sentence.Add(
                            $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                            $"points.");

                        expPoints = 25 * (float)pointsToHeal / target.CharStats.MaxMp();
                        expScore += (int)expPoints;
                        break;
                    case EnumMagicType.RestoreBoth:
                        diff = target.CharStats.MaxHp() -
                                   target.CharStats.CurrentHp;
                        pointsToHeal = diff < damage ? diff : damage;

                        target.CharStats.CurrentHp += pointsToHeal;
                        sentence.Add(
                            $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                            $"points.");
                        expPoints = 12 * (float)pointsToHeal / target.CharStats.MaxHp();
                        expScore += (int)expPoints;

                        diff = target.CharStats.MaxMp() -
                               target.CharStats.CurrentMp;
                        pointsToHeal = diff < damage ? diff : damage;
                        target.CharStats.CurrentMp += damage;
                        sentence.Add(
                            $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                            $"points.");

                        expPoints = 12 * (float)pointsToHeal / target.CharStats.MaxMp();
                        expScore += (int)expPoints;
                        break;
                    case EnumMagicType.Cure:
                        var statusEffectsToCure = new List<EnumStatusEffect>();
                        switch (magicLevel) {
                            case 1:
                                statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                                break;
                            case 2:
                                statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                                statusEffectsToCure.Add(EnumStatusEffect.asleep);
                                break;
                            case 3:
                                statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                                statusEffectsToCure.Add(EnumStatusEffect.asleep);
                                statusEffectsToCure.Add(EnumStatusEffect.confused);
                                break;
                            case 4:
                                statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                                statusEffectsToCure.Add(EnumStatusEffect.asleep);
                                statusEffectsToCure.Add(EnumStatusEffect.confused);
                                statusEffectsToCure.Add(EnumStatusEffect.paralyzed);
                                break;
                        }

                        foreach (var statusEffect in statusEffectsToCure) {
                            if (target.StatusEffects.HasFlag(statusEffect)) {
                                target.StatusEffects = target.StatusEffects.Remove(statusEffect);
                                sentence.Add(
                                    $"{target.Name.AddColor(Constants.Orange)} is no longer " +
                                    $"{Enum.GetName(typeof(EnumStatusEffect), statusEffect).AddColor(Color.gray)}");

                                expScore += 10;
                            }
                        }

                        break;
                    case EnumMagicType.Special:
                        return 0;
                    default:
                        return 0;
                }
            }
            _dialogManager.EvokeSentenceDialogue(sentence);
            return expScore;
        }
    }
}

