using System;
using Assets.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle {
    class BattleCalculator {

        public float GetMaxDamage(int attack, int defense, int landEffect) {
            var result = (attack - defense) * (1f - ((float)landEffect / 100));
            return result <= 0 ? 1 : result;
        }

        public int GetBaseDamageWeaponAttack(Character attacker, Character attacked, int landEffect) {
            int attack = attacker.CharStats.Attack.GetModifiedValue();
            int defense = attacked.CharStats.Defense.GetModifiedValue();
            float maxDamage = GetMaxDamage(attack, defense, landEffect);
            float minDamage = maxDamage * 0.8f;
            var result = (int)Math.Round(Random.Range(minDamage, maxDamage), MidpointRounding.AwayFromZero);
            return result <= 0 ? 1 : result;
        }

        public bool RollForDodge(Character attacker, Character target) {
            var dodgeEnum = target.DodgeBaseChance;
            if (target.MovementType == EnumMovementType.fly || target.MovementType == EnumMovementType.floating) {
                if (attacker.MovementType != EnumMovementType.fly ||
                    attacker.MovementType != EnumMovementType.floating) {
                    if (attacker.GetAttackRange() == EnumAttackRange.Melee ||
                        attacker.GetAttackRange() == EnumAttackRange.Spear) {
                        dodgeEnum = EnumChance.OneIn8;
                    }
                }
            }
            var baseDodge = ConvertEnumChanceIntoFloat(dodgeEnum);
            var diffAgility = target.CharStats.Agility.GetModifiedValue() - attacker.CharStats.Agility.GetModifiedValue();
            var dodgeChance = baseDodge + (diffAgility / 100f); //diff equals added %
            var dodgeRoll = Random.Range(0f, 1f);
            Debug.Log($"DodgeChance: {dodgeChance}, DodgeRoll: {dodgeRoll}");
            return dodgeRoll <= dodgeChance;
        }

        public bool RollForDoubleAttack(Character attacker) {
            var doubleChance = ConvertEnumChanceIntoFloat(attacker.DoubleAttackChance);
            var doubleRoll = Random.Range(0f, 1f);
            Debug.Log($"doubleChance: {doubleChance}, doubleRoll: {doubleRoll}");
            return doubleRoll <= doubleChance;
        }

        public bool RollForCrit(Character attacker) {
            var critChance = ConvertEnumChanceIntoFloat(attacker.CritChance);
            var critRole = Random.Range(0f, 1f);
            Debug.Log($"critChance: {critChance}, critRole: {critRole}");
            return critRole <= critChance;
        }

        public int GetCritDamage(Character attacker, int baseDamage) {
            var critMuliplier = attacker.CritDamageMultiplier;
            var result = baseDamage * critMuliplier;
            return (int)Math.Round(result, MidpointRounding.AwayFromZero);
        }

        public bool RollForCounter(Character counter, int spacesApart) {
            if (!IsCounterPossible(counter.GetAttackRange(), spacesApart)) {
                return false;
            }

            var counterChance = ConvertEnumChanceIntoFloat(counter.CounterChance);
            var counterRoll = Random.Range(0f, 1f);
            Debug.Log($"counterChance: {counterChance}, counterRoll: {counterRoll}");
            return counterRoll <= counterChance;
        }

        public int GetModifiedDamageBasedOnElement(ref int damage, EnumElementType magicElement, Character character) {
            switch (magicElement) {
                case EnumElementType.Fire:
                    switch (character.ElementalResistance) {
                        case EnumElementalResistanceWeakness.Fire25:
                            damage =  (int)(damage * 0.75f);
                            return 1;
                        case EnumElementalResistanceWeakness.Fire50:
                            damage = (int)(damage * 0.5f);
                            return 1;
                        case EnumElementalResistanceWeakness.Fire75:
                            damage = (int)(damage * 0.25f);
                            return 1;
                        default:
                            switch (character.ElementalWeakness) {
                                case EnumElementalResistanceWeakness.Fire25:
                                    damage = (int)(damage * 1.25f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Fire50:
                                    damage = (int)(damage * 1.5f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Fire75:
                                    damage = (int)(damage * 1.75f);
                                    return 2;
                                default:
                                    return 0;
                            }
                    }
                case EnumElementType.Ice:
                    switch (character.ElementalResistance) {
                        case EnumElementalResistanceWeakness.Cold25:
                            damage = (int)(damage * 0.75f);
                            return 1;
                        case EnumElementalResistanceWeakness.Cold50:
                            damage = (int)(damage * 0.5f);
                            return 1;
                        case EnumElementalResistanceWeakness.Cold75:
                            damage = (int)(damage * 0.25f);
                            return 1;
                        default:
                            switch (character.ElementalWeakness) {
                                case EnumElementalResistanceWeakness.Cold25:
                                    damage = (int)(damage * 1.25f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Cold50:
                                    damage = (int)(damage * 1.5f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Cold75:
                                    damage = (int)(damage * 1.75f);
                                    return 2;
                                default:
                                    return 0;
                            }
                    }
                case EnumElementType.Wind:
                    switch (character.ElementalResistance) {
                        case EnumElementalResistanceWeakness.Wind25:
                            damage = (int)(damage * 0.75f);
                            return 1;
                        case EnumElementalResistanceWeakness.Wind50:
                            damage = (int)(damage * 0.5f);
                            return 1;
                        case EnumElementalResistanceWeakness.Wind75:
                            damage = (int)(damage * 0.25f);
                            return 1;
                        default:
                            switch (character.ElementalWeakness) {
                                case EnumElementalResistanceWeakness.Wind25:
                                    damage = (int)(damage * 1.25f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Wind50:
                                    damage = (int)(damage * 1.5f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Wind75:
                                    damage = (int)(damage * 1.75f);
                                    return 2;
                                default:
                                    return 0;
                            }
                    }
                case EnumElementType.Lightning:
                    switch (character.ElementalResistance) {
                        case EnumElementalResistanceWeakness.Bolt25:
                            damage = (int)(damage * 0.75f);
                            return 1;
                        case EnumElementalResistanceWeakness.Bolt50:
                            damage = (int)(damage * 0.5f);
                            return 1;
                        case EnumElementalResistanceWeakness.Bolt75:
                            damage = (int)(damage * 0.25f);
                            return 1;
                        default:
                            switch (character.ElementalWeakness) {
                                case EnumElementalResistanceWeakness.Bolt25:
                                    damage = (int)(damage * 1.25f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Bolt50:
                                    damage = (int)(damage * 1.5f);
                                    return 2;
                                case EnumElementalResistanceWeakness.Bolt75:
                                    damage = (int)(damage * 1.75f);
                                    return 2;
                                default:
                                    return 0;
                            }
                    }
                default:
                    return 0;
            }
        }

        private float ConvertEnumChanceIntoFloat(EnumChance enumChance) {
            float chance;
            switch (enumChance) {
                case EnumChance.OneIn32:
                    chance = 0.03125f;
                    break;
                case EnumChance.OneIn16:
                    chance = 0.0625f;
                    break;
                case EnumChance.OneIn8:
                    chance = 0.125f;
                    break;
                case EnumChance.OneIn4:
                    chance = 0.25f;
                    break;
                case EnumChance.OneIn2:
                    chance = 0.5f;
                    break;
                case EnumChance.Always:
                    chance = 1f;
                    break;
                case EnumChance.Never:
                    chance = -1f;
                    break;
                default:
                    chance = -1f;
                    break;
            }

            return chance;
        }

        private bool IsCounterPossible(EnumAttackRange range1, int spacesApart) {
            switch (spacesApart) {
                case 0:
                    return false;
                case 1:
                    return range1 != EnumAttackRange.ShortBow && range1 != EnumAttackRange.LongBow;
                case 2:
                    return range1 == EnumAttackRange.ShortBow || range1 == EnumAttackRange.LongBow ||
                           range1 == EnumAttackRange.Spear || range1 == EnumAttackRange.Range3 ||
                           range1 == EnumAttackRange.Range4 || range1 == EnumAttackRange.Range5;

                case 3:
                    return range1 == EnumAttackRange.LongBow || range1 == EnumAttackRange.Range3 ||
                           range1 == EnumAttackRange.Range4 || range1 == EnumAttackRange.Range5;
                case 4:
                    return range1 == EnumAttackRange.Range4 || range1 == EnumAttackRange.Range5;
                case 5:
                    return range1 == EnumAttackRange.Range5;
                default:
                    return false;
            }
        }
    }
}
