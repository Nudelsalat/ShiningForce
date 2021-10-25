using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Assets.Enums;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.HelperScripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle.AI {
    public class AiController : MonoBehaviour {
        public static AiController Instance;

        private AiData _currentAiData;
        private Unit _currentUnit;
        private Character _currentCharacter;
        private List<Vector3> _currentMovementSquares;

        private BattleController _battleController;
        private Cursor _cursor;
        private MovementGrid _movementGrid;
        private AttackOption _selectedAttackOption;
        private Player _player;

        private Magic _magicToAttack;
        private int _magicLevelToAttack;

        private EnumAiState _state = EnumAiState.None;

        private LayerMask _forceLayerMask;
        private LayerMask _enemyLayerMask;
        private LayerMask _layerMaskOfCurrentUnit;

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
            _player = Player.Instance;
        }

        void Update() {
            switch (_state) {
                case EnumAiState.None:
                    break;
                case EnumAiState.MoveCursorToUnit:
                    if (_cursor.UnitReached && !Player.IsInDialogue) {
                        _state = EnumAiState.None;
                        var currentCharacter = _battleController.GetCurrentUnit().GetCharacter();
                        if (currentCharacter.StatusEffects.HasFlag(EnumStatusEffect.asleep)) {
                            if (currentCharacter.CheckWakeup()) {
                                DialogManager.Instance.EvokeSingleSentenceDialogue($"{currentCharacter.Name.AddColor(Constants.Orange)} " +
                                                                                   $"woke up!");
                            } else {
                                DialogManager.Instance.EvokeSingleSentenceDialogue($"{currentCharacter.Name.AddColor(Constants.Orange)} " +
                                                                                   $"is fast {"asleep".AddColor(Color.grey)}.");
                                EndAiTurn();
                                return;
                            }
                        }
                        if (currentCharacter.StatusEffects.HasFlag(EnumStatusEffect.paralyzed)) {
                            var battleCalc = new BattleCalculator();
                            if (battleCalc.RollForStatusEffect(EnumChance.OneIn3)) {
                                DialogManager.Instance.EvokeSingleSentenceDialogue($"{currentCharacter.Name.AddColor(Constants.Orange)} " +
                                                                                   $"is no longer {"paralyzed".AddColor(Color.grey)}!\n" +
                                                                                   $"But wasted its turn in the process.");
                                EndAiTurn();
                                return;
                            } else {
                                DialogManager.Instance.EvokeSingleSentenceDialogue($"{currentCharacter.Name.AddColor(Constants.Orange)} " +
                                                                                   $"is still {"paralyzed".AddColor(Color.grey)}.");
                                EndAiTurn();
                                return;
                            }
                        }
                        if (_selectedAttackOption?.GetAttackPosition() == null) {
                            StartCoroutine(WaitSeconds(0.75f, EnumAiState.ExecuteAction));
                            return;
                        }
                        _cursor.ReturnToPosition(_currentMovementSquares, (Vector3)_selectedAttackOption.GetAttackPosition(), 8f);
                        StartCoroutine(WaitSeconds(0.75f, EnumAiState.MoveUnit));
                    }
                    break;
                case EnumAiState.MoveUnit:
                    if (_cursor.UnitReached && !Player.IsInDialogue) {
                        if (_selectedAttackOption.GetMainTargetPosition() == null) {
                            EndAiTurn();
                            return;
                        }

                        _battleController.GenerateMovementSquaresForAction(_currentUnit.transform.position,
                            _selectedAttackOption.GetAttackRange());
                        _cursor.SetAttackArea((Vector3) _selectedAttackOption.GetMainTargetPosition(),
                            _selectedAttackOption.GetAoe());
                        StartCoroutine(WaitSeconds(0.75f, EnumAiState.SelectTarget));
                    }
                    break;
                case EnumAiState.SelectTarget:
                    _state = EnumAiState.None;
                    Player.InputDisabledAiBattle = false;
                    _battleController.ExecuteAttack(_selectedAttackOption, _selectedAttackOption.GetAttackType());
                    //ExecuteAttack does already end the turn
                    _state = EnumAiState.None;
                    break;
                case EnumAiState.ExecuteAction:
                    EndAiTurn();
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
            unit.CheckTrigger();
            _currentAiData = unit.GetAiData();
            _currentMovementSquares = movementSquares;
            _layerMaskOfCurrentUnit = _enemyLayerMask;
            if (_currentCharacter.StatusEffects.HasFlag(EnumStatusEffect.confused)) {
                SetConfusedRandomizedBehaviour();
            }

            ExecuteTurn();
        }

        public void ConfusedTurn(Unit unit, List<Vector3> movementSquares) {
            Player.InputDisabledAiBattle = true;
            _currentUnit = unit;
            _currentCharacter = unit.GetCharacter();
            _currentMovementSquares = movementSquares;
            _currentAiData = new AiData();
            _currentAiData.SetCharacter(_currentCharacter);
            _layerMaskOfCurrentUnit = _forceLayerMask;
            if (_currentCharacter.StatusEffects.HasFlag(EnumStatusEffect.confused)) {
                SetConfusedRandomizedBehaviour();
            }

            ExecuteTurn();
        }

        private void ExecuteTurn() {
            if (CheckPreconditions(_currentAiData.PrimaryAiType)) {
                ExecuteAi(_currentAiData.PrimaryAiType);
            } else if (CheckPreconditions(_currentAiData.SecondaryAiType)) {
                ExecuteAi(_currentAiData.SecondaryAiType);
            } else {
                _battleController.SetNextUnit();
            }
        }

        private bool CheckPreconditions(EnumAiType aiType) {
            switch (aiType) {
                case EnumAiType.Idle:
                case EnumAiType.InRangeAttack:
                case EnumAiType.Aggressive:
                case EnumAiType.InRangeMage:
                case EnumAiType.AggressiveMage:
                case EnumAiType.InRangeDebuff:
                case EnumAiType.InRangeAttackOrMoveTwo:
                case EnumAiType.InRangeMageOrMoveTwo:
                    return true;
                case EnumAiType.Patrole:
                    //TODO....
                    return false;
                case EnumAiType.Healer:
                    var healMagic = _currentAiData.HealingMagic;
                    if (healMagic == null || healMagic.IsEmpty()) {
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
                            x.GetCharacter().CharStats.CurrentHp <= x.GetCharacter().CharStats.MaxHp() * _currentAiData.PercentChance)) {
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
                case EnumAiType.MoveTowardTargetBerserk:
                    return _currentAiData.TargetPoint != null;
                default:
                    Debug.LogError("Unknown AiType! return preconditions not met.");
                    return false;
            }
        }

        private void ExecuteAi(EnumAiType aiType) {
            var atLeastOneOption = false;
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
                        return;
                    } 
                    break;
                case EnumAiType.Aggressive:
                case EnumAiType.InRangeAttackOrMoveTwo:
                    _magicToAttack = null;
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                        return;
                    } else {
                        if (aiType == EnumAiType.InRangeAttackOrMoveTwo) {
                            MoveTowardsClosestTarget(2);
                        } else {
                            MoveTowardsClosestTarget();
                        }
                        return;
                    }
                case EnumAiType.InRangeMage:
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    atLeastOneOption = TryExecuteAttack(out var allMagicAttackOptions);
                    _magicToAttack = null;
                    atLeastOneOption = TryExecuteAttack(out var allPhysicalAttackOptions) || atLeastOneOption;
                    if (atLeastOneOption) {
                        allPhysicalAttackOptions.AddRange(allMagicAttackOptions);
                        ExecuteBestAttackOption(allPhysicalAttackOptions);
                        return;
                    } 
                    break;
                case EnumAiType.AggressiveMage:
                case EnumAiType.InRangeMageOrMoveTwo:
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    atLeastOneOption = TryExecuteAttack(out allMagicAttackOptions);
                    _magicToAttack = null;
                    atLeastOneOption = TryExecuteAttack(out allPhysicalAttackOptions) || atLeastOneOption;
                    if (atLeastOneOption) {
                        allPhysicalAttackOptions.AddRange(allMagicAttackOptions);
                        ExecuteBestAttackOption(allPhysicalAttackOptions);
                        return;
                    } else {
                        if (aiType == EnumAiType.InRangeMageOrMoveTwo) {
                            MoveTowardsClosestTarget(2);
                        } else {
                            MoveTowardsClosestTarget();
                        }
                        return;
                    }
                case EnumAiType.InRangeDebuff:
                    SetUpMagicSpell(_currentAiData.StatusEffectMagic);
                    atLeastOneOption = TryExecuteAttack(out allMagicAttackOptions);
                    _magicToAttack = null;
                    atLeastOneOption = TryExecuteAttack(out allPhysicalAttackOptions) || atLeastOneOption;
                    if (atLeastOneOption) {
                        allPhysicalAttackOptions.AddRange(allMagicAttackOptions);
                        ExecuteBestAttackOption(allPhysicalAttackOptions);
                        return;
                    } 
                    break;
                case EnumAiType.Healer:
                    SetUpMagicSpell(_currentAiData.HealingMagic);
                    if (TryExecuteAttack(out allAttackOptions)) {
                        ExecuteBestAttackOption(allAttackOptions);
                        return;
                    } 
                    break;
                case EnumAiType.Follow:
                    //Use follow target as Secondary action.
                    if (_currentAiData.TargetUnit) {
                        ExecuteMoveTowardPoint(_currentAiData.TargetUnit.transform.position);
                        return;
                    } 
                    break;
                case EnumAiType.MoveTowardPoint:
                    //Use moveToward Point as Secondary action...
                    if (_currentAiData.TargetPoint != null) {
                        if (_currentAiData.TargetPoint == _currentUnit.transform.position 
                            && _currentAiData.SecondaryAiType != aiType) {
                            ExecuteAi(_currentAiData.SecondaryAiType);
                            return;
                        }
                        ExecuteMoveTowardPoint((Vector3) _currentAiData.TargetPoint);
                        return;
                    }
                    break;
                case EnumAiType.MoveTowardTarget:
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    atLeastOneOption = TryExecuteAttack(out allMagicAttackOptions);
                    _magicToAttack = null;
                    atLeastOneOption = TryExecuteAttack(out allPhysicalAttackOptions) || atLeastOneOption;
                    if (atLeastOneOption) {
                        allPhysicalAttackOptions.AddRange(allMagicAttackOptions);
                        var targetReachable = allPhysicalAttackOptions.Where(x => x.GetTargetList().Contains(_currentAiData.TargetUnit)).ToList();
                        ExecuteBestAttackOption(targetReachable.Count > 0 ? targetReachable : allPhysicalAttackOptions);
                        return;
                    } else {
                        ExecuteMoveTowardPoint(_currentAiData.TargetUnit.transform.position);
                        return;
                    }
                case EnumAiType.MoveTowardTargetBerserk:
                    // will try to get to target, even if other targets are next to berserker unit
                    SetUpMagicSpell(_currentAiData.DamageMagic);
                    TryExecuteAttack(out allMagicAttackOptions);
                    _magicToAttack = null;
                    TryExecuteAttack(out allPhysicalAttackOptions);
                    allPhysicalAttackOptions.AddRange(allMagicAttackOptions);
                    allPhysicalAttackOptions = allPhysicalAttackOptions.Where(x => x.GetTargetList().Contains(_currentAiData.TargetUnit)).ToList();
                    if (allPhysicalAttackOptions.Count > 0) {
                        ExecuteBestAttackOption(allPhysicalAttackOptions);
                        return;
                    } else {
                        ExecuteMoveTowardPoint(_currentAiData.TargetUnit.transform.position);
                        return;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(aiType), aiType, null);
            }
            if (aiType != _currentAiData.SecondaryAiType) {
                ExecuteAi(_currentAiData.SecondaryAiType);
            } else {
                ExecuteAttackOption(null);
            }
        }

        private bool TryExecuteAttack(out List<AttackOption> attackOptions) {
            int attack;
            var aoe = EnumAreaOfEffect.Single;
            var attackRange = EnumAttackRange.Melee;
            //who is the opponent?
            var target = _layerMaskOfCurrentUnit == _enemyLayerMask ? _forceLayerMask : _enemyLayerMask;
            EnumMagicType? magicType = null;
            if (_magicToAttack != null) {
                attack = _magicToAttack.Damage[_magicLevelToAttack - 1];
                aoe = _magicToAttack.AreaOfEffect[_magicLevelToAttack - 1];
                attackRange = _magicToAttack.AttackRange[_magicLevelToAttack - 1];
                if (_battleController.IsReverseTarget(_magicToAttack)) {
                    target = _enemyLayerMask;
                }
                magicType = _magicToAttack.MagicType;
            } else {
                attack = _currentCharacter.CharStats.Attack.GetModifiedValue();
                aoe = _currentCharacter.GetAttackAreaOfEffect();
                attackRange = _currentCharacter.GetAttackRange();
            }

            if (_currentCharacter.StatusEffects.HasFlag(EnumStatusEffect.confused) 
                && _enemyLayerMask == (_enemyLayerMask | (1 << _currentUnit.gameObject.layer))) {
                target = target == _enemyLayerMask ? _forceLayerMask : _enemyLayerMask;
            }
            var canTargetItself = target == _layerMaskOfCurrentUnit;
            var attackOptionsDict = new Dictionary<Vector3, AttackOption>();
            foreach (var movementSquare in _currentMovementSquares) {
                // This square is occupied by another unit.
                var anotherEnemyUnit =
                    _movementGrid.GetOccupyingOpponent(movementSquare.x, movementSquare.y, _enemyLayerMask);
                if (anotherEnemyUnit != null && anotherEnemyUnit != _currentUnit) {
                    continue;
                }
                var attackPoints = _movementGrid.GetMovementPointsAreaOfEffect(
                    movementSquare, attackRange).ToList();
                var attackPointsWithOffset = attackPoints.Select(x => {
                    var vector3 = new Vector3 {
                        x = x.x + 0.5f,
                        y = x.y + 0.5f
                    };
                    return vector3;
                }).ToList();
                foreach (var vector3 in attackPointsWithOffset) {
                    // AttackOption already checked. But better positioning?
                    if (attackOptionsDict.ContainsKey(vector3)) {
                        attackOptionsDict.TryGetValue(vector3, out var currentAttackOption);
                        //if the attack can target itself his position may change the score of an already calculated square, because of AOE.
                        if (canTargetItself) {
                            var newAttackOption = CalculateAttackOption(vector3, aoe, target, canTargetItself, 
                                movementSquare, attack, attackRange);
                            if (newAttackOption == null) {
                                continue;
                            }
                            var oldScore = currentAttackOption?.GetScore() ?? 0;
                            var newScore = newAttackOption.GetScore();
                            // if the old score is better, don't change anything.
                            if (oldScore > newScore) {
                                continue; 
                            // if the newScore is better, change it! ELSE: check the landeffect value.
                            } else if (newScore > oldScore) {
                                attackOptionsDict[vector3] = newAttackOption;
                                continue;
                            }
                        }
                        var currentAttackPosition = currentAttackOption?.GetAttackPosition();
                        if (currentAttackPosition != null) {
                            var currentLandEffect =
                                _cursor.GetLandEffect((Vector3)currentAttackPosition);
                            var newLandEffect = _cursor.GetLandEffect(movementSquare);
                            if (newLandEffect > currentLandEffect) {
                                currentAttackOption.SetAttackPosition(movementSquare);
                                attackOptionsDict[vector3] = currentAttackOption;
                            }
                        }
                        continue;
                    }

                    var attackOption = CalculateAttackOption(vector3, aoe, target, canTargetItself,
                        movementSquare, attack, attackRange);
                    attackOptionsDict.Add(vector3, attackOption);
                }
            }

            attackOptions = attackOptionsDict.Values.Where(x => x != null).ToList();
            return attackOptions.Any();
        }

        private AttackOption CalculateAttackOption(Vector3 pointToAttack, EnumAreaOfEffect aoe, LayerMask targetLayerMask, bool canTargetItself,
            Vector3 positionOfAttacker, int attack, EnumAttackRange attackRange) {

            //First part: don't target attacks original position if attacker can target himself,
            //second part: however let him target himself, if he will move to this point (even if original position)
            if ((_currentUnit.transform.position + new Vector3(0, -0.25f) != pointToAttack 
                    && _movementGrid.IsOccupiedByOpponent(pointToAttack.x, pointToAttack.y, targetLayerMask)) ||
                (canTargetItself && positionOfAttacker + new Vector3(0, -0.25f) == pointToAttack)) {
                var targetList = GetAllTargetsInAreaOfEffect(pointToAttack, aoe, targetLayerMask, canTargetItself,
                    positionOfAttacker + new Vector3(0, -0.25f));
                var attackOption = new AttackOption(targetList, positionOfAttacker,
                    pointToAttack, attack, _magicToAttack, _magicLevelToAttack,
                    aoe, attackRange);
                return attackOption;
            } else {
                return null;
            }
        }

        private void MoveTowardsClosestTarget(int maxSquare = 999) {
            var movementTyp = _currentCharacter.MovementType;
            var tilesNotToUse = TerrainEffects.GetImpassableTerrainNameOfMovementType(movementTyp);
            var listOfUsableSquares = _movementGrid.GetWholeTerrainTileMapWithoutCertainSprites(tilesNotToUse, 0);
            var pathfinder = new Pathfinder(listOfUsableSquares, _currentUnit.transform.position, _forceLayerMask,
                _movementGrid, movementTyp);
            ExecuteShortestPathAi(pathfinder, EnumAiType.Idle, maxSquare);
        }

        // Try to get to point with the most direct way, calculating landEffects
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
            if (!listOfUsableSquares.Contains(point)) {
                //If you want to get to a specific target, add that point to have a chance to find a path.
                listOfUsableSquares.Add(point);
            }
            var pathfinder = new Pathfinder(listOfUsableSquares, _currentUnit.transform.position, point,
                _movementGrid, movementTyp);
            ExecuteShortestPathAi(pathfinder, EnumAiType.Aggressive);
        }

        private void ExecuteShortestPathAi(Pathfinder pathfinder, EnumAiType followUpAi, int maxSquares = 999) {
            var shortestPath = pathfinder.GetShortestPath();
            if (shortestPath == null || !shortestPath.Any()) {
                ExecuteAi(followUpAi);
                return;
            }
            maxSquares = maxSquares < shortestPath.Count() ? maxSquares : shortestPath.Count - 1;
            shortestPath = shortestPath.GetRange(0, maxSquares);
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
            var bestLandEffect = -1;
            var bestScore = -1f;
            foreach (var attackOption in attackOptions) {
                var score = attackOption.GetScore();
                if (bestScore < score) {
                    bestScore = score;
                    bestLandEffect = _cursor.GetLandEffect((Vector3)attackOption.GetAttackPosition());
                    bestOption = attackOption;
                } else if (Math.Abs(bestScore - score) < 0.0001f) {
                    var landEffect = _cursor.GetLandEffect((Vector3) attackOption.GetAttackPosition());
                    if (landEffect > bestLandEffect) {
                        bestScore = score;
                        bestLandEffect = landEffect;
                        bestOption = attackOption;
                    }
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
            if (spell == null || spell.IsEmpty()) {
                _magicToAttack = null;
                return;
            }
            _magicToAttack = spell;
            _magicLevelToAttack = _magicToAttack.CurrentLevel;
            while (_magicLevelToAttack > 0 && 
                   _magicToAttack.ManaCost[_magicLevelToAttack - 1] > _currentCharacter.CharStats.CurrentMp) {
                _magicLevelToAttack--;
            }

            if (_magicLevelToAttack <= 0) {
                _magicToAttack = null;
            }
        }

        private void SetConfusedRandomizedBehaviour() {
            var diceRoll = Random.Range(1, 101);
            switch (diceRoll) {
                case int n when (n >= 75):
                    _currentAiData.PrimaryAiType = EnumAiType.InRangeMage;
                    _currentAiData.SecondaryAiType = EnumAiType.InRangeAttack;
                    break;
                case int n when (75 > n && n >= 50):
                    _currentAiData.PrimaryAiType = EnumAiType.InRangeDebuff;
                    _currentAiData.SecondaryAiType = EnumAiType.InRangeAttack;
                    break;
                case int n when (50 > n && n >= 25):
                    _currentAiData.PrimaryAiType = EnumAiType.Healer;
                    _currentAiData.SecondaryAiType = EnumAiType.Idle;
                    break;
                case int n when (25 > n && n > 0):
                    _currentAiData.PrimaryAiType = EnumAiType.Idle;
                    break;
            }
        }

        private List<Unit> GetAllTargetsInAreaOfEffect(Vector3 point, EnumAreaOfEffect aoe, LayerMask targetLayer, 
            bool canTargetItSelf, Vector3 positionOfAttacker) {
            var aoeSquares = new List<Vector3>();
            var aoeTargets = new List<Unit>();
            switch (aoe) {
                case EnumAreaOfEffect.Single:
                    if (canTargetItSelf && point == positionOfAttacker) {
                        aoeTargets.Add(_currentUnit);
                        return aoeTargets;
                    }
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
                    return targetLayer == Constants.LayerMaskForce ? _battleController.GetForce() : _battleController.GetEnemies();
            }

            foreach (var square in aoeSquares) {
                var target = _movementGrid.GetOccupyingOpponent(square.x, square.y, targetLayer);
                //Don't add attacking unit itself, because it will move before attacking and therefor won't be at this point anymore.
                if (target != null && target != _currentUnit) {
                    aoeTargets.Add(target);
                }
                //If attacker can target itself, and is in the Areaofeffect after moving for the attack, add Attacker.
                if (canTargetItSelf && square == positionOfAttacker) {
                    aoeTargets.Add(_currentUnit);
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
