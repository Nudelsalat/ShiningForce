using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using UnityEngine;

namespace Assets.Scripts.Battle.AI {
    class AttackOption {
        private readonly List<Unit> _listTargets;
        private readonly Vector3? _positionFromWhichToAttack;
        private readonly Vector3? _targetPosition;
        private readonly int _attack;
        private EnumElementType _elementType;
        private EnumAreaOfEffect _areaOfEffect;
        private EnumAttackRange _attackRange;
        private readonly bool _isFixedDamage;
        private readonly EnumMagicType? _magicType;

        private readonly BattleCalculator _battleController = new BattleCalculator();

        public AttackOption(List<Unit> listTargets, Vector3? attackPosition, Vector3? targetPosition, int attack, 
            EnumAreaOfEffect aoe = EnumAreaOfEffect.Single, EnumAttackRange attackRange = EnumAttackRange.Melee, 
            EnumElementType elementType = EnumElementType.None, bool isFixedDamage = false, EnumMagicType? magicType = null) {
            _listTargets = listTargets;
            _positionFromWhichToAttack = attackPosition;
            _targetPosition = targetPosition;
            _attack = attack;
            _elementType = elementType;
            _areaOfEffect = aoe;
            _magicType = magicType;
            _attackRange = attackRange;
            _isFixedDamage = isFixedDamage;
        }

        public float GetScore() {
            if (_magicType == EnumMagicType.Heal) {
                return GetHealScore();
            }
            if (_magicType == EnumMagicType.Debuff || _magicType == EnumMagicType.Buff) {
                return 5 * _listTargets.Count();
            }
            if (_isFixedDamage) {
                //TODO: multiply if elementalWeakness
                return GetFixedDamageScore();
            }
            return GetDamageScore();
        }

        public Vector3? GetAttackPosition() {
            return _positionFromWhichToAttack;
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

        private float GetHealScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var charStats = target.GetCharacter().CharStats;
                var hpDiff = charStats.MaxHp - charStats.CurrentHp;
                if (hpDiff < _attack) {
                    score += (float)hpDiff / charStats.MaxHp;
                } else {
                    score += (float)_attack / charStats.MaxHp;
                }
            }
            return score;
        }

        private float GetDamageScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                var landEffect = Cursor.Instance.GetLandEffect(target.transform.position);
                var damage = _battleController.GetMaxDamage(_attack,
                    target.GetCharacter().CharStats.Defense.GetModifiedValue(), landEffect);
                if (damage >= target.GetCharacter().CharStats.CurrentHp) {
                    score += 100;
                }
                score += damage / target.GetCharacter().CharStats.MaxHp;
            }
            return score;
        }

        private float GetFixedDamageScore() {
            var score = 0f;
            foreach (var target in _listTargets) {
                if (_attack >= target.GetCharacter().CharStats.CurrentHp) {
                    score += 100;
                }
                score += (float)_attack / target.GetCharacter().CharStats.MaxHp;
            }
            return score;
        }
    }
}
