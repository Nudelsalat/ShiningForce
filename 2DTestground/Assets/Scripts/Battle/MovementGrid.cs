using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Characters;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Battle {
    public class MovementGrid {

        private List<Vector3Int> _movementGrid = new List<Vector3Int>();
        private Dictionary<Vector3Int, float> _movementGridWithRestMovement = new Dictionary<Vector3Int, float>();
        private Tilemap _terrainTileMap;
        private LayerMask _oppositeLayerMask;
        private EnumMovementType _currentUnitMovementType;
        private float _z;
        private readonly List<Tuple<Vector3, string>> _tileMapDictionary = new List<Tuple<Vector3, string>>();

        public MovementGrid(Tilemap terrainTileMap) {
            _terrainTileMap = terrainTileMap;
            _tileMapDictionary.Clear();
            foreach (var pos in _terrainTileMap.cellBounds.allPositionsWithin) {
                var worldPosition = _terrainTileMap.WorldToCell(new Vector3(pos.x, pos.y, pos.z));
                var sprite = _terrainTileMap.GetSprite(worldPosition);
                if (sprite == null) {
                    continue;
                }
                _tileMapDictionary.Add(new Tuple<Vector3, string>(
                    new Vector3(worldPosition.x+0.5f, worldPosition.y+0.75f), sprite.name));
            }
        }

        public List<Vector3> GetWholeTerrainTileMapWithoutCertainSprites(List<string> spriteNames, LayerMask opponentLayerMask) {
            var rejectList = _tileMapDictionary.Where(x => spriteNames.Contains(x.Item2));
            var result = _tileMapDictionary.Except(rejectList);
            result = result.Where(x => !IsOccupiedByOpponent(x.Item1.x, x.Item1.y, opponentLayerMask));
            var vector3List = result.Select(x => x.Item1);
            return vector3List.ToList();
        }

        public IEnumerable<Vector3Int> GetMovementPointsOfUnit(Unit currentUnit, Vector3 origPosition) {
            _z = origPosition.z;
            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            _currentUnitMovementType = currentUnit.GetCharacter().MovementType;

            _oppositeLayerMask = GetOpponentLayerMask(currentUnit);

            return MoveFunction(origPosition.x, origPosition.y,
                currentUnit.GetCharacter().CharStats.Movement.GetModifiedValue());
        }

        public IEnumerable<Vector3Int> GetMovementPointsAreaOfEffect(Vector3 startPoint, EnumAttackRange attackRange) {
            IEnumerable<Vector3Int> result = new List<Vector3Int>();
            switch (attackRange) {
                case EnumAttackRange.Self:
                    result = GetMovementPointsAreaOfEffect(startPoint, 0, 0);
                    break;
                case EnumAttackRange.Melee:
                    result = GetMovementPointsAreaOfEffect(startPoint, 1, 0);
                    break;
                case EnumAttackRange.Spear:
                    result = GetMovementPointsAreaOfEffect(startPoint, 2, 0);
                    break;
                case EnumAttackRange.ShortBow:
                    result = GetMovementPointsAreaOfEffect(startPoint, 2, 1);
                    break;
                case EnumAttackRange.LongBow:
                    result = GetMovementPointsAreaOfEffect(startPoint, 3, 1);
                    break;
                case EnumAttackRange.StraightLine:
                    Debug.LogError("Straight line attack not yet implemented.");
                    break;
                case EnumAttackRange.Range3:
                    result = GetMovementPointsAreaOfEffect(startPoint, 3, 0);
                    break;
                case EnumAttackRange.Range4:
                    result = GetMovementPointsAreaOfEffect(startPoint, 4, 0);
                    break;
                case EnumAttackRange.Range5:
                    result = GetMovementPointsAreaOfEffect(startPoint, 5, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackRange), attackRange, null);
            }
            return result;
        }

        // antiRange is used for e.g.: archer, where the space right next to them is not included within the 
        // range grid.
        public IEnumerable<Vector3Int> GetMovementPointsAreaOfEffect(Vector3 startPoint, int range, int antiRange) {
            _oppositeLayerMask = 0;
            _z = startPoint.z;
            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            var completeRange = new List<Vector3Int>();
            completeRange.AddRange(MoveFunction(startPoint.x, startPoint.y, range, false));

            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            if (antiRange > 0) {
                var antiRangeList = MoveFunction(startPoint.x, startPoint.y, antiRange, false);

                foreach (var point in antiRangeList) {
                    completeRange.Remove(point);
                }
            }

            return completeRange;
        }

        private IEnumerable<Vector3Int> MoveFunction(float x, float y, float movement, bool isUnit = true) {
            var point = _terrainTileMap.WorldToCell(new Vector3(x, y, _z));
            if (_movementGridWithRestMovement.TryGetValue(point, out var restMovement)) {
                //There was a better way to this point
                if (restMovement >= movement) {
                    return _movementGrid;
                }

                // This is a better way to this point
                _movementGridWithRestMovement[point] = movement;
            }
            else {
                // This point is new.
                _movementGrid.Add(point);
                _movementGridWithRestMovement.Add(point, movement);
            }

            var movementCost = isUnit ? GetMovementCost(x - 1, y) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x - 1, y, _oppositeLayerMask)) {
                    MoveFunction(x - 1, y, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ? GetMovementCost(x, y - 1) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x, y - 1, _oppositeLayerMask)) {
                    MoveFunction(x, y - 1, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ? GetMovementCost(x + 1, y) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x + 1, y, _oppositeLayerMask)) {
                    MoveFunction(x + 1, y, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ? GetMovementCost(x, y + 1) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x, y + 1, _oppositeLayerMask)) {
                    MoveFunction(x, y + 1, movement - movementCost, isUnit);
                }
            }

            return _movementGrid;
        }

        public bool IsOccupiedByOpponent(float x, float y, LayerMask opponentLayerMask) {
            var collision = Physics2D.OverlapCircle(new Vector2(x, y), 0.2f, opponentLayerMask);
            if (collision != null) {
                Debug.Log($"GameObjectName: {collision.name}");
                return true;
            }

            return false;
        }

        public Unit GetOccupyingOpponent(float x, float y, LayerMask opponentLayerMask) {
            var collision = Physics2D.OverlapCircle(new Vector2(x, y), 0.2f, opponentLayerMask);
            var unit = collision?.GetComponent<Unit>();
            if (unit != null) {
                Debug.Log($"GameObjectName: {collision.name}");
                return unit;
            }
            return null;
        }

        public LayerMask GetOpponentLayerMask(Unit currentUnit, bool reverse = false) {
            if (currentUnit.gameObject.layer == LayerMask.NameToLayer("Force")) {
                return reverse ? LayerMask.GetMask("Force")  : LayerMask.GetMask("Enemies");
            } else if (currentUnit.gameObject.layer == LayerMask.NameToLayer("Enemies")) {
                return reverse ? LayerMask.GetMask("Enemies") : LayerMask.GetMask("Force");
            } else {
                return 0;
            }
        }

        private float GetMovementCost(float x, float y) {
            var worldPosition = _terrainTileMap.WorldToCell(new Vector3(x, y, _z));
            var sprite = _terrainTileMap.GetSprite(worldPosition);

            float terrainCost = TerrainEffects.GetMovementCost(_currentUnitMovementType,
                TerrainEffects.GetTerrainTypeByName(sprite.name));

            return terrainCost;
        }

        public float GetMovementCost(float x, float y, EnumMovementType movementType) {
            var worldPosition = _terrainTileMap.WorldToCell(new Vector3(x, y, _z));
            var sprite = _terrainTileMap.GetSprite(worldPosition);

            float terrainCost = TerrainEffects.GetMovementCost(movementType,
                TerrainEffects.GetTerrainTypeByName(sprite.name));

            return terrainCost;
        }
    }
}
