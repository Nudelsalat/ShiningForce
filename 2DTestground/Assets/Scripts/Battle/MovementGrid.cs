using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Battle;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Characters;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Menus.Battle {
    public class MovementGrid {

        private List<Vector3Int> _movementGrid = new List<Vector3Int>();
        private Dictionary<Vector3Int, float> _movementGridWithRestMovement = new Dictionary<Vector3Int, float>();
        private Tilemap _terrainTileMap;
        private LayerMask _oppositeLayerMask;
        private EnumMovementType _currentUnitMovementType;
        private float _z;

        public MovementGrid (Tilemap terrainTileMap) {
            _terrainTileMap = terrainTileMap;
        }

        public IEnumerable<Vector3Int> GetMovementPointsOfUnit(Unit currentUnit) {
            _z = currentUnit.transform.position.z;
            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            _currentUnitMovementType = currentUnit.Character.MovementType;

            if (currentUnit.gameObject.layer == LayerMask.NameToLayer("Force")) {
                _oppositeLayerMask = LayerMask.GetMask("Enemies");
            } else if (currentUnit.gameObject.layer == LayerMask.NameToLayer("Enemies")) {
                _oppositeLayerMask = LayerMask.GetMask("Force");
            } else {
                _oppositeLayerMask = 0;
            }

            return MoveFunction(currentUnit.transform.position.x, currentUnit.transform.position.y,
                currentUnit.Character.CharStats.Movement.GetModifiedValue());
        }
        // antiRange is used for e.g.: archer, where the space right next to them is not included within the 
        // range grid.
        public IEnumerable<Vector3Int> GetMovementPointsAreaOfEffect(Vector3 startPoint, int range, int antiRange) {
            _z = startPoint.z;
            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            var completeRange = new List<Vector3Int>();
            completeRange.AddRange(MoveFunction(startPoint.x, startPoint.y, range, false));

            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();
            var antiRangeList = MoveFunction(startPoint.x, startPoint.y, antiRange, false);

            foreach (var point in antiRangeList) {
                completeRange.Remove(point);
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
            } else {
                // This point is new.
                _movementGrid.Add(point);
                _movementGridWithRestMovement.Add(point, movement);
            }

            var movementCost = isUnit ? GetMovementCost(x - 1, y) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x - 1,y)) {
                    MoveFunction(x - 1, y, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ?  GetMovementCost(x, y - 1) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x, y - 1)) {
                    MoveFunction(x, y - 1, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ? GetMovementCost(x + 1, y) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x + 1, y)) {
                    MoveFunction(x + 1, y, movement - movementCost, isUnit);
                }
            }

            movementCost = isUnit ? GetMovementCost(x, y + 1) : 1;
            if (movement - movementCost >= 0) {
                if (!IsOccupiedByOpponent(x,y + 1)) {
                    MoveFunction(x, y + 1, movement - movementCost, isUnit);
                }
            }

            return _movementGrid;
        }

        private bool IsOccupiedByOpponent(float x, float y) {
            var collision = Physics2D.OverlapCircle(new Vector2(x, y), 0.2f, _oppositeLayerMask);
            if (collision != null) {
                Debug.Log($"GameObjectName: {collision.name}");
                return true;
            }
            return false;
        }

        private float GetMovementCost(float x, float y) {
            var worldPosition = _terrainTileMap.WorldToCell(new Vector3(x, y, _z));
            var sprite = _terrainTileMap.GetSprite(worldPosition);

            float terrainCost = TerrainEffects.GetMovementCost(_currentUnitMovementType,
                TerrainEffects.GetTerrainTypeByName(sprite.name));

            return terrainCost;
        }
    }
}
