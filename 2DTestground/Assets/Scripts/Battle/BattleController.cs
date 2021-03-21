using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Characters;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Battle {
    public class BattleController : MonoBehaviour {

        private Unit _selectedUnit;
        private Unit _currentUnit;
        private EnumMovementType _currentUnitMovementType;
        private Queue<Unit> _turnOrder;
        private Vector3Int _originalPosition;
        private List<Vector3Int> _movementGrid = new List<Vector3Int>();
        private Dictionary<Vector3Int,float> _movementGridWithRestMovement = new Dictionary<Vector3Int, float>();

        private Tilemap _terrainTileMap;
        private float _z;
        private List<GameObject> _movementSquareSprites = new List<GameObject>();

        public static BattleController Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }
        }

        void Start() {
            _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
        }

        public void SetSelectedUnit(Unit selectedUnit) {
            _selectedUnit = selectedUnit;

            //TODO REMOVE FOR TESTING
            _currentUnit = selectedUnit;
            _currentUnitMovementType = selectedUnit.Character.MovementType;
            GenerateMovementSquares();
        }

        private void GenerateMovementSquares() {
            _z = _currentUnit.transform.position.z;
            _originalPosition = _terrainTileMap.WorldToCell(_currentUnit.transform.position);
            _movementGrid.Clear();
            _movementGridWithRestMovement.Clear();

            var reachableSquares = MoveFunction(_currentUnit.transform.position.x, _currentUnit.transform.position.y,
                _currentUnit.Character.CharStats.Movement.GetValue());
            ShowMovementSquareSprites(reachableSquares);
        }

        private IEnumerable<Vector3Int> MoveFunction(float x, float y, float movement) {
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

            var movementCost = GetMovementCost(x - 1,y);
            if (movement - movementCost >= 0) {
                MoveFunction(x - 1, y, movement - movementCost);
            }

            movementCost = GetMovementCost(x, y - 1);
            if (movement - movementCost >= 0) {
                MoveFunction(x, y - 1, movement - movementCost);
            }

            movementCost = GetMovementCost(x + 1, y);
            if (movement - movementCost >= 0) {
                MoveFunction(x + 1, y, movement - movementCost);
            }

            movementCost = GetMovementCost(x, y + 1);
            if (movement - movementCost >= 0) {
                MoveFunction(x, y + 1, movement - movementCost);
            }

            return _movementGrid;
        }

        private void ShowMovementSquareSprites(IEnumerable<Vector3Int> movementSquares) {
            var square = Resources.Load<GameObject>(Constants.PrefabMovementSquare);
            foreach (var movementSquare in movementSquares) {
                _movementSquareSprites.Add(Instantiate(square,
                    new Vector3(movementSquare.x + .5f, movementSquare.y + .5f), Quaternion.identity));
            }
        }

        public void DestroyMovementSquareSprites() {
            foreach (var sprite in _movementSquareSprites) {
                Destroy(sprite);
            }
        }

        private float GetMovementCost(float x, float y) {
            var worldPosition = _terrainTileMap.WorldToCell(new Vector3(x,y,_z));
            var sprite = _terrainTileMap.GetSprite(worldPosition);

            float terrainCost = TerrainEffects.GetMovementCost(_currentUnitMovementType,
                TerrainEffects.GetTerrainTypeByName(sprite.name));

            return terrainCost;
        }
    }
}
