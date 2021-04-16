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
        private readonly bool _isFixedDamage;

        private readonly BattleCalculator _battleController = new BattleCalculator();

        public AttackOption(List<Unit> listTargets, Vector3? attackPosition, Vector3? targetPosition,
            int attack, EnumElementType elementType, bool isFixedDamage) {
            _listTargets = listTargets;
            _positionFromWhichToAttack = attackPosition;
            _targetPosition = targetPosition;
            _attack = attack;
            _elementType = elementType;
            _isFixedDamage = isFixedDamage;
        }

        public float GetScore() {
            if (_isFixedDamage) {
                //TODO: multiply if elementalWeakness
                return _attack * _listTargets.Count();
            }

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

        public Vector3? GetAttackPosition() {
            return _positionFromWhichToAttack;
        }

        public Vector3? GetMainTargetPosition() {
            return _targetPosition;
        }
        public List<Unit> GetTargetList() {
            return _listTargets;
        }
    }
}
