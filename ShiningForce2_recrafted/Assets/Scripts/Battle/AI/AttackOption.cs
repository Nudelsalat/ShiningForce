using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.GameData.Magic;
using UnityEngine;

namespace Assets.Scripts.Battle.AI {
    public class AttackOption {
        private readonly List<Unit> _listTargets;
        private Vector3? _positionFromWhichToAttack;
        private readonly Vector3? _targetPosition;
        private readonly int _attack;
        private readonly EnumAreaOfEffect _areaOfEffect;
        private readonly EnumAttackRange _attackRange;
        private readonly Magic _magic;
        private readonly int _magicAttackLevel;

        private readonly BattleCalculator _battleCalculator = new BattleCalculator();

        public AttackOption(List<Unit> listTargets, Vector3? attackPosition, Vector3? targetPosition, int attack, 
            Magic magicAttack = null, int magicAttackLevel = 0, EnumAreaOfEffect aoe = EnumAreaOfEffect.Single, 
            EnumAttackRange attackRange = EnumAttackRange.Melee) {
            _listTargets = listTargets;
            _positionFromWhichToAttack = attackPosition;
            _targetPosition = targetPosition;
            _attack = attack;
            _areaOfEffect = aoe;
            _magic = magicAttack;
            _magicAttackLevel = magicAttackLevel;
            _attackRange = attackRange;
        }

        public float GetScore() {
            var result = 0f;
            var magicType = _magic?.MagicType;
            if (magicType == EnumMagicType.Heal || magicType == EnumMagicType.RestoreBoth) {
                result += GetHealScore();
            }
            if (magicType == EnumMagicType.RestoreMP || magicType == EnumMagicType.RestoreBoth) {
                result += GetRestoreMpScore();
            }
            if (magicType == EnumMagicType.Debuff || magicType == EnumMagicType.Buff || magicType == EnumMagicType.Special) {
                result += (5 * _listTargets.Count());
            }
            if (magicType == EnumMagicType.Cure) {
                var statusEffectsToCure = new List<EnumStatusEffect>();
                int count = 0;
                switch (_magicAttackLevel) {
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
                        statusEffectsToCure.Add(EnumStatusEffect.silent);
                        break;
                    case 4:
                        statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                        statusEffectsToCure.Add(EnumStatusEffect.asleep);
                        statusEffectsToCure.Add(EnumStatusEffect.confused);
                        statusEffectsToCure.Add(EnumStatusEffect.paralyzed);
                        statusEffectsToCure.Add(EnumStatusEffect.silent);
                        break;
                }
                
                foreach (var target in _listTargets) {
                    var character = target.GetCharacter();
                    foreach (var statusEffect in statusEffectsToCure) {
                        if (character.StatusEffects.HasFlag(statusEffect)) {
                            count++;
                        }
                    }
                }
                result += (5 * count);
            }
            if (magicType == EnumMagicType.Damage) {
                result += GetMagicDamageScore();
            }
            if (magicType == null) {
                result += GetDamageScore();
            }

            Debug.Log($"Ai TotalScore: {result}");
            return result;
        }

        public Vector3? GetAttackPosition() {
            return _positionFromWhichToAttack;
        }

        public void SetAttackPosition(Vector3 attackPosition) {
            _positionFromWhichToAttack = attackPosition;
        }

        public Vector3? GetMainTargetPosition() {
            return _targetPosition;
        }
        public List<Unit> GetTargetList() {
            return _listTargets;
        }

        public EnumAreaOfEffect GetAoe() {
            return _areaOfEffect;
        }

        public EnumAttackRange GetAttackRange() {
            return _attackRange;
        }

        public Magic GetMagic() {
            return _magic;
        }

        public int GetMagicLevel() {
            return _magicAttackLevel;
        }

        public EnumCurrentBattleMenu GetAttackType() {
            if (_magic == null || _magic.IsEmpty()) {
                return EnumCurrentBattleMenu.attack;
            } else {
                return EnumCurrentBattleMenu.magic;
            }
        }

        private float GetHealScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var charStats = target.GetCharacter().CharStats;
                var hpDiff = charStats.MaxHp() - charStats.CurrentHp;
                if (hpDiff < _attack) {
                    score += (float)hpDiff / charStats.MaxHp();
                } else {
                    score += (float)_attack / charStats.MaxHp();
                }
            }
            return score;
        }

        private float GetRestoreMpScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var charStats = target.GetCharacter().CharStats;
                var mpDiff = charStats.MaxMp() - charStats.CurrentMp;
                if (mpDiff < _attack) {
                    score += (float)mpDiff / charStats.MaxMp();
                } else {
                    score += (float)_attack / charStats.MaxMp();
                }
            }
            return score;
        }

        private float GetDamageScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var landEffect = Cursor.Instance.GetLandEffect(target.transform.position);
                var damage = _battleCalculator.GetMaxDamage(_attack,
                    target.GetCharacter().CharStats.Defense.GetModifiedValue(), landEffect);
                if (damage >= target.GetCharacter().CharStats.CurrentHp) {
                    score += 100;
                }
                score += damage / target.GetCharacter().CharStats.MaxHp();
                if (target.GetCharacter().StatusEffects.HasFlag(EnumStatusEffect.asleep)) {
                    score /= 2;
                }
            }
            return score;
        }

        private float GetMagicDamageScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var damage = _attack;
                _battleCalculator.GetModifiedDamageBasedOnElement(ref damage, _magic.ElementType, target.GetCharacter());
                if (damage >= target.GetCharacter().CharStats.CurrentHp) {
                    score += 100;
                }
                score += (float)damage / target.GetCharacter().CharStats.MaxHp();
            }
            return score;
        }
    }
}
