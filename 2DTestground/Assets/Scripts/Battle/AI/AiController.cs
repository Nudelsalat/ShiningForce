using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.Menus.Battle;
using UnityEngine;

namespace Assets.Scripts.Battle.AI {
    public class AiController : MonoBehaviour {
        public static AiController Instance;

        private AiData _currentAiData;
        private EnemyUnit _currentUnit;
        private Character _currentCharacter;
        private List<Vector3> _currentMovementSquares;

        private BattleController _battleController;
        private Cursor _cursor;
        private MovementGrid _movementGrid;
        private AttackOption _selectedAttackOption;

        private Magic _magicToAttack;
        private int _magicLevelToAttack;

        private EnumAiState _state = EnumAiState.None;

        private LayerMask _forceLayerMask;
        private LayerMask _enemyLayerMask;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }
            _forceLayerMask = Constants.LayerMaskForce;
            _enemyLayerMask = Constants.LayerMaskEnemies;
        }

        void Start() {
            _battleController = BattleController.Instance;
            _cursor = Cursor.Instance;
        }

        void Update() {
            switch (_state) {
                case EnumAiState.None:
                    break;
                case EnumAiState.MoveCursorToUnit:
                    if (_cursor.UnitReached && !Player.IsInDialogue) {
                        if (_selectedAttackOption?.GetAttackPosition() == null) {
                            EndAiTurn();
                            return;
                        }
                        _cursor.ReturnToPosition(_currentMovementSquares, (Vector3)_selectedAttackOption.GetAttackPosition());
                        StartCoroutine(WaitSeconds(0.75f, EnumAiState.MoveUnit));
                    }
                    break;
                case EnumAiState.MoveUnit:
                    if (_cursor.UnitReached) {
                        if (_selectedAttackOption.GetMainTargetPosition() == null) {
                            EndAiTurn();
                            return;
                        }
                        _cursor.SelectNextTarget((Vector3) _selectedAttackOption.GetMainTargetPosition());
                        //TODO Menu stuff
                        StartCoroutine(WaitSeconds(0.75f, EnumAiState.SelectTarget));
                    }
                    break;
                case EnumAiState.SelectTarget:
                    _state = EnumAiState.None;
                    Player.InputDisabledAiBattle = false;
                    _battleController.ExecuteAttack(_selectedAttackOption.GetTargetList(), _magicToAttack, _magicLevelToAttack);
                    EndAiTurn();
                    break;
                case EnumAiState.ExecuteAction:
                    break;
            }
        }

        public void SetMovementGrid(MovementGrid movementGrid) {
            _movementGrid = movementGrid;
        }

        public void AiTurn(EnemyUnit unit, List<Vector3> movementSquares) {
            Player.InputDisabledAiBattle = true;
            _currentUnit = unit;
            _currentCharacter = unit.GetCharacter();
            _currentMovementSquares = movementSquares;
            _currentUnit.CheckTrigger();
            _currentAiData = _currentUnit.GetAiData();

            //TODO: cursor move and stuff...

            ExecuteTurn();
        }

        private void ExecuteTurn() {
            if (CheckPreconditions(_currentAiData.PrimaryAiType)) {
                ExecuteAi(_currentAiData.PrimaryAiType);
            } else if (CheckPreconditions(_currentAiData.SecondaryAiType)) {
                ExecuteAi(_currentAiData.PrimaryAiType);
            } else {
                _battleController.NextUnit();
            }
        }

        private bool CheckPreconditions(EnumAiType aiType) {
            switch (aiType) {
                case EnumAiType.Idle:
                case EnumAiType.InRangeAttack:
                case EnumAiType.Aggressive:
                    return true;
                case EnumAiType.Patrole:
                    //TODO....
                    return false;
                case EnumAiType.InRangeMage:
                case EnumAiType.AggressiveMage:
                    var damageMagic = _currentAiData.DamageMagic;
                    if (damageMagic.IsEmpty()) {
                        return false;
                    }
                    var manaForDamage = _currentUnit.GetCharacter().CharStats.CurrentMp;
                    if (manaForDamage < _currentAiData.DamageMagic.ManaCost[0]) {
                        return false;
                    }
                    return true;
                case EnumAiType.Healer:
                    var healMagic = _currentAiData.HealingMagic;
                    if (healMagic.IsEmpty()) {
                        return false;
                    }
                    var mana = _currentUnit.GetCharacter().CharStats.CurrentMp;
                    if (mana < _currentAiData.HealingMagic.ManaCost[0]) {
                        return false;
                    }
                    var enemiesToHeal = _battleController.GetEnemies();
                    if (enemiesToHeal.Exists(x =>
                        x.GetCharacter().CharStats.CurrentHp <= x.GetCharacter().CharStats.MaxHp / 2)) {
                        return true;
                    }
                    return false;
                case EnumAiType.Follow:
                case EnumAiType.MoveTowardTarget:
                    return _currentAiData.TargetUnit != null;
                case EnumAiType.MoveTowardPoint:
                    return _currentAiData.TargetPoint != null;
                default:
                    Debug.LogError("Unknown AiType! return preconditions not met.");
                    return false;
            }
        }

        private void ExecuteAi(EnumAiType aiType) {
            switch (aiType) {
                case EnumAiType.Idle:
                    ExecuteAttackOption(null);
                    break;
                case EnumAiType.Patrole:
                    //TODO
                    break;
                case EnumAiType.InRangeAttack:
                    _magicToAttack = null;
                    if (!TryExecuteAttack(_currentCharacter.CharStats.Attack.GetModifiedValue(),
                        EnumElementType.None, false)) {
                        ExecuteAttackOption(null);
                    }
                    break;
                case EnumAiType.Aggressive:
                    _magicToAttack = null;
                    if (!TryExecuteAttack(_currentCharacter.CharStats.Attack.GetModifiedValue(), 
                        EnumElementType.None, false)) {
                        ExecuteAttackOption(null);
                        //MoveTowardsClosestTarget();
                    }
                    //TODO
                    break;
                case EnumAiType.InRangeMage:
                    //TODO
                    break;
                case EnumAiType.AggressiveMage:
                    //TODO
                    break;
                case EnumAiType.Healer:
                    //TODO
                    break;
                case EnumAiType.Follow:
                    //TODO
                    break;
                case EnumAiType.MoveTowardPoint:
                    if (_currentAiData.TargetPoint != null) {
                        ExecuteMoveTowardPoint((Vector3) _currentAiData.TargetPoint);
                    }
                    break;
                case EnumAiType.MoveTowardTarget:
                    //TODO
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aiType), aiType, null);
            }
        }

        private bool TryExecuteAttack(int attack, EnumElementType elementType, bool fixedDamage) {
            var attackOptionsDict = new Dictionary<Vector3, AttackOption>();
            foreach (var movementSquare in _currentMovementSquares) {
                // This square is occupied by another unit.
                var anotherEnemyUnit =
                    _movementGrid.GetOccupyingOpponent(movementSquare.x, movementSquare.y, _enemyLayerMask);
                if (anotherEnemyUnit != null && anotherEnemyUnit != _currentUnit) {
                    continue;
                }
                var moveOption = _movementGrid.GetMovementPointsAreaOfEffect(
                    movementSquare, _currentCharacter.GetAttackRange()).ToList();
                var moveOption2 = moveOption.Select(x => {
                    var vector3 = new Vector3 {
                        x = x.x + 0.5f,
                        y = x.y + 0.5f
                    };
                    return vector3;
                }).ToList();
                foreach (var vector3 in moveOption2) {
                    // AttackOption already checked.
                    if (attackOptionsDict.ContainsKey(vector3)) {
                        continue;
                    } 
                    if (_movementGrid.IsOccupiedByOpponent(vector3.x, vector3.y, _forceLayerMask)) {
                        var targetList = GetAllTargetsInAreaOfEffect(vector3, 
                            _currentCharacter.GetAttackAreaOfEffect(), _forceLayerMask);
                        var attackOption = new AttackOption(targetList, movementSquare, 
                            vector3, attack, elementType, fixedDamage);
                        attackOptionsDict.Add(vector3, attackOption);
                    } else {
                        attackOptionsDict.Add(vector3, null);
                    }
                }
            }

            var allAttackOptions = attackOptionsDict.Values.Where(x => x != null).ToList();
            if (!allAttackOptions.Any()) {
                return false;
            }

            var bestAttackOption = PickBestAttackOption(allAttackOptions);
            ExecuteAttackOption(bestAttackOption);
            return true;
        }

        private void MoveTowardsClosestTarget() {
            //TODO :-> is this target reachable, with the movement of the current unit?
        }

        private void ExecuteMoveTowardPoint(Vector3 point) {
            if (_currentMovementSquares.Contains(point)) {
                _cursor.ReturnToPosition(_currentMovementSquares, point);
                return;
            }

            var closestPoint = _currentMovementSquares.Aggregate(
                (first, second) => Math.Abs(first.x - point.x) + Math.Abs(first.y - point.y) <
                                   Math.Abs(second.x - point.x) + Math.Abs(second.y - point.y)
                    ? first
                    : second);

            var attackOption = new AttackOption(null, closestPoint, null, 0, EnumElementType.None, false);
            ExecuteAttackOption(attackOption);
        }

        private void ExecuteAttackOption(AttackOption attackOption) {
            _selectedAttackOption = attackOption;
            _state = EnumAiState.MoveCursorToUnit;
        }

        private AttackOption PickBestAttackOption(List<AttackOption> attackOptions) {
            AttackOption bestOption = null;
            var bestScore = -1f;
            foreach (var attackOption in attackOptions) {
                var score = attackOption.GetScore();
                if (bestScore < score) {
                    bestScore = score;
                    bestOption = attackOption;
                }
            }
            return bestOption;
        }

        private void EndAiTurn() {
            _state = EnumAiState.None;
            Player.InputDisabledAiBattle = false;
            _cursor.EndTurn();
        }

        private List<Unit> GetAllTargetsInAreaOfEffect(Vector3 point, EnumAreaOfEffect aoe, LayerMask targetLayer) {
            var aoeSquares = new List<Vector3>();
            var aoeTargets = new List<Unit>();
            switch (aoe) {
                case EnumAreaOfEffect.Single:
                    aoeTargets.Add(_movementGrid.GetOccupyingOpponent(point.x, point.y, targetLayer));
                    return aoeTargets;
                case EnumAreaOfEffect.Area1:
                    aoeSquares = _movementGrid.GetMovementPointsAreaOfEffect(point, EnumAttackRange.Melee).
                        Select(x => {
                        var vector3 = new Vector3 {
                            x = x.x + 0.5f,
                            y = x.y + 0.5f
                        };
                        return vector3;
                    }).ToList();
                    break;
                case EnumAreaOfEffect.Area2:
                    aoeSquares = _movementGrid.GetMovementPointsAreaOfEffect(point, EnumAttackRange.Spear).
                        Select(x => {
                            var vector3 = new Vector3 {
                                x = x.x + 0.5f,
                                y = x.y + 0.5f
                            };
                            return vector3;
                        }).ToList();
                    break;
                case EnumAreaOfEffect.AllAllies:
                    if (targetLayer == Constants.LayerMaskForce) {
                        return _battleController.GetForce();
                    } else {
                        return _battleController.GetEnemies();
                    }
            }

            foreach (var square in aoeSquares) {
                var target = _movementGrid.GetOccupyingOpponent(square.x, square.y, targetLayer);
                if (target != null) {
                    aoeTargets.Add(target);
                }
            }
            return aoeTargets;
        }
        IEnumerator WaitSeconds(float seconds, EnumAiState nextState) {
            _state = EnumAiState.None;
            yield return new WaitForSeconds(seconds);
            _state = nextState;
        }
    }

    internal enum EnumAiState {
        None, 
        MoveCursorToUnit,
        MoveUnit,
        SelectTarget,
        ExecuteAction,
    }
}
