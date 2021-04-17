using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.Menus.Battle;
using UnityEditorInternal;
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
                        _cursor.ReturnToPosition(_currentMovementSquares, (Vector3)_selectedAttackOption.GetAttackPosition(), 8f);
                        StartCoroutine(WaitSeconds(0.75f, EnumAiState.MoveUnit));
                    }
                    break;
                case EnumAiState.MoveUnit:
                    if (_cursor.UnitReached) {
                        if (_selectedAttackOption.GetMainTargetPosition() == null) {
                            EndAiTurn();
                            return;
                        }

                        _battleController.GenerateMovementSquaresForAction(_currentUnit.transform.position,
                            _selectedAttackOption.GetAttackRange());
                        _cursor.SetAttackArea((Vector3) _selectedAttackOption.GetMainTargetPosition(),
                            _selectedAttackOption.GetAoe());
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
            
            ExecuteTurn();
        }

        private void ExecuteTurn() {
            if (CheckPreconditions(_currentAiData.PrimaryAiType)) {
                ExecuteAi(_currentAiData.PrimaryAiType);
            } else if (CheckPreconditions(_currentAiData.SecondaryAiType)) {
                ExecuteAi(_currentAiData.SecondaryAiType);
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

                    SetUpMagicSpell(_currentAiData.HealingMagic);
                    TryExecuteAttack(out var allAttackOptions);
                    foreach (var attackOption in allAttackOptions) {
                        if (attackOption.GetTargetList().Exists(x =>
                            x.GetCharacter().CharStats.CurrentHp <= x.GetCharacter().CharStats.MaxHp / 2)) {
                            ExecuteBestAttackOption(allAttackOptions);
                            return true;
                        }
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
                    if (TryExecuteAttack(out var allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                    } else {
                        ExecuteAttackOption(null);
                    }
                    break;
                case EnumAiType.Aggressive:
                    _magicToAttack = null;
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                    } else {
                        MoveTowardsClosestTarget();
                    }
                    break;
                case EnumAiType.InRangeMage:
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                    } else {
                        ExecuteAttackOption(null);
                    }
                    break;
                case EnumAiType.AggressiveMage:
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                    } else {
                        MoveTowardsClosestTarget();
                    }
                    break;
                case EnumAiType.Healer:
                    SetUpMagicSpell(_currentAiData.HealingMagic);
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                    } else {
                        ExecuteAi(EnumAiType.InRangeMage);
                    }
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

        private bool TryExecuteAttack(out List<AttackOption> attackOptions) {
            int attack;
            var elementType = EnumElementType.None;
            var fixedDamage = false;
            var aoe = EnumAreaOfEffect.Single;
            var attackRange = EnumAttackRange.Melee;
            var target = _forceLayerMask;
            EnumMagicType? magicType = null;
            if (_magicToAttack != null) {
                fixedDamage = true;
                attack = _magicToAttack.Damage[_magicLevelToAttack - 1];
                elementType = _magicToAttack.ElementType;
                aoe = _magicToAttack.AreaOfEffect[_magicLevelToAttack - 1];
                attackRange = _magicToAttack.AttackRange[_magicLevelToAttack - 1];
                if (_magicToAttack.MagicType == EnumMagicType.Heal || _magicToAttack.MagicType == EnumMagicType.Buff) {
                    target = _enemyLayerMask;
                }
                magicType = _magicToAttack.MagicType;
            } else {
                attack = _currentCharacter.CharStats.Attack.GetModifiedValue();
                aoe = _currentCharacter.GetAttackAreaOfEffect();
                attackRange = _currentCharacter.GetAttackRange();
            }
            var attackOptionsDict = new Dictionary<Vector3, AttackOption>();
            foreach (var movementSquare in _currentMovementSquares) {
                // This square is occupied by another unit.
                var anotherEnemyUnit =
                    _movementGrid.GetOccupyingOpponent(movementSquare.x, movementSquare.y, _enemyLayerMask);
                if (anotherEnemyUnit != null && anotherEnemyUnit != _currentUnit) {
                    continue;
                }
                var moveOption = _movementGrid.GetMovementPointsAreaOfEffect(
                    movementSquare, attackRange).ToList();
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
                    if (_movementGrid.IsOccupiedByOpponent(vector3.x, vector3.y, target)) {
                        var targetList = GetAllTargetsInAreaOfEffect(vector3, aoe, target);
                        var attackOption = new AttackOption(targetList, movementSquare, 
                            vector3, attack, aoe, attackRange, elementType, fixedDamage, magicType);
                        attackOptionsDict.Add(vector3, attackOption);
                    } else {
                        attackOptionsDict.Add(vector3, null);
                    }
                }
            }

            attackOptions = attackOptionsDict.Values.Where(x => x != null).ToList();
            return attackOptions.Any();
        }

        private void MoveTowardsClosestTarget() {
            var movementTyp = _currentCharacter.MovementType;
            var tilesNotToUse = TerrainEffects.GetImpassableTerrainNameOfMovementType(movementTyp);
            var listOfUsableSquares = _movementGrid.GetWholeTerrainTileMapWithoutCertainSprites(tilesNotToUse, 0);
            var pathfinder = new Pathfinder(listOfUsableSquares, _currentUnit.transform.position, _forceLayerMask,
                _movementGrid, movementTyp);
            ExecuteShortestPathAi(pathfinder, EnumAiType.Idle);
        }

        // Try to get to point with the most direct way, ignoring landEffects
        // if path is blocked, AI switches to aggressive attack mode.
        private void ExecuteMoveTowardPoint(Vector3 point) {
            if (_currentMovementSquares.Contains(point)) {
                if (!_movementGrid.IsOccupiedByOpponent(point.x, point.y, _enemyLayerMask)) {
                    var attackOption = new AttackOption(null, point, null, 0);
                    ExecuteAttackOption(attackOption);
                    return;
                } else {
                    ExecuteAi(EnumAiType.InRangeAttack);
                }
            }
            var movementTyp = _currentCharacter.MovementType;
            var tilesNotToUse = TerrainEffects.GetImpassableTerrainNameOfMovementType(movementTyp);
            var listOfUsableSquares = _movementGrid.GetWholeTerrainTileMapWithoutCertainSprites(tilesNotToUse, _forceLayerMask);
            var pathfinder = new Pathfinder(listOfUsableSquares, _currentUnit.transform.position, point,
                _movementGrid, movementTyp);
            ExecuteShortestPathAi(pathfinder, EnumAiType.Aggressive);
        }

        private void ExecuteShortestPathAi(Pathfinder pathfinder, EnumAiType followUpAi) {
            var shortestPath = pathfinder.GetShortestPath();
            if (shortestPath == null) {
                ExecuteAi(followUpAi);
                return;
            }
            shortestPath.Reverse();
            foreach (var pointInPath in shortestPath) {
                if (_currentMovementSquares.Contains(pointInPath) &&
                    !_movementGrid.IsOccupiedByOpponent(pointInPath.x, pointInPath.y, _enemyLayerMask)) {

                    var attackOption = new AttackOption(null, pointInPath,null, 0);
                    ExecuteAttackOption(attackOption);
                    return;
                }
            }
            ExecuteAi(followUpAi);
        }

        private void ExecuteBestAttackOption(List<AttackOption> allAttackOptions) {
            var bestAttackOption = PickBestAttackOption(allAttackOptions);
            ExecuteAttackOption(bestAttackOption);
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

        private void SetUpMagicSpell(Magic spell) {
            if (spell.IsEmpty()) {
                _magicToAttack = null;
                return;
            }
            _magicToAttack = spell;
            _magicLevelToAttack = _magicToAttack.CurrentLevel;
            while (_magicToAttack.ManaCost[_magicLevelToAttack - 1] > _currentCharacter.CharStats.CurrentMp
                   || _magicLevelToAttack <= 0) {
                _magicLevelToAttack--;
            }

            if (_magicLevelToAttack <= 0) {
                _magicToAttack = null;
            }
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
