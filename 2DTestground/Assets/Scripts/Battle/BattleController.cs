using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Menus.Battle;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle {
    public class BattleController : MonoBehaviour {

        private Unit _selectedUnit;
        private Unit _currentUnit;
        private Queue<Unit> _turnOrder;
        private Vector3 _originalPosition;

        private readonly List<Unit> _force = new List<Unit>();
        private readonly List<Unit> _enemies = new List<Unit>();

        private Tilemap _terrainTileMap;
        private float _z;
        private readonly List<GameObject> _movementSquareSprites = new List<GameObject>();
        private bool _inBattle = false;

        private MovementGrid _movementGrid;
        private Cursor _cursor;
        private EnumBattleState _currentBattleState;

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
            _movementGrid = new MovementGrid(_terrainTileMap);
            _cursor = Cursor.Instance;

            _terrainTileMap.color = Constants.Invisible;
            transform.gameObject.SetActive(false);
            //TODO BeginBattle will be evoke via event, not automatically.
            BeginBattle();
        }

        void Update() {
            if (Input.GetButtonDown("Back")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.ReturnToPosition(null, _originalPosition);
                        break;
                    case EnumBattleState.unitSelected:
                        DestroyMovementSquareSprites();
                        _cursor.ReturnToPosition(_movementSquareSprites, _originalPosition);
                        _cursor.ClearControlUnit();
                        _currentBattleState = EnumBattleState.freeCursor;
                        break;
                }
            }
            if (Input.GetButtonDown("Interact")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.CheckIfCursorIsOverUnit(out var unit);
                        if (_currentUnit == unit) {
                            SetSelectedUnit(_currentUnit);
                            _cursor.SetControlUnit(_currentUnit);
                            _currentBattleState = EnumBattleState.unitSelected;
                        }
                        
                        break;
                    case EnumBattleState.unitSelected:
                        break;
                }
            }
        }

        public void BeginBattle() {
            _currentBattleState = EnumBattleState.freeCursor;
            transform.gameObject.SetActive(true);
            var force = GameObject.Find("Force").transform;
            var enemies = GameObject.Find("Enemies").transform;
            foreach (Transform child in force) {
                var unit = child.GetComponent<Unit>();
                if (unit != null) {
                    _force.Add(unit);
                }
            }
            foreach (Transform child in enemies) {
                var unit = child.GetComponent<Unit>();
                if (unit != null) {
                    _enemies.Add(unit);
                }
            }
            SetNewTurnOrder();
            NextUnit();
        }

        public void NextUnit() {
            //TODO CHECK win condition. (here?)
            //TODO also check for events, like respawn, other events

            //TODO remove player agency
            if (_turnOrder.Count <= 0) {
                SetNewTurnOrder();
            }
            _currentUnit = _turnOrder.Dequeue();
            _originalPosition = _currentUnit.transform.position;
            _cursor.ReturnToPosition(null, _originalPosition);
        }

        private void SetNewTurnOrder() {
            var unitList = new List<Tuple<Unit, float>>();
            foreach (var enemy in _enemies) {
                //TODO Bosses double turn?
                var agility = GetRandomAgilityValue(enemy);
                unitList.Add(new Tuple<Unit, float>(enemy,agility));
            }
            foreach (var force in _force) {
                //TODO Bosses double turn?
                var agility = GetRandomAgilityValue(force);
                unitList.Add(new Tuple<Unit, float>(force, agility));
            }
            unitList.Sort((x,y) => -(x.Item2.CompareTo(y.Item2)));
            _turnOrder = new Queue<Unit>(unitList.Select(x => x.Item1));
        }

        private float GetRandomAgilityValue(Unit unit) {
            return (unit.Character.CharStats.Agility.GetModifiedValue() * Random.Range(0.875f, 1.125f)) 
                   + Random.Range(-1, 2); //Max is exclusive... so -1, 0, or 1
        }

        public void SetSelectedUnit(Unit selectedUnit) {
            _selectedUnit = selectedUnit;

            //TODO REMOVE FOR TESTING
            _currentUnit = selectedUnit;
            GenerateMovementSquares();
        }

        private void GenerateMovementSquares() {
            //_originalPosition = _terrainTileMap.WorldToCell(_currentUnit.transform.position);
            var reachableSquares = _movementGrid.GetMovementPointsOfUnit(_currentUnit).ToList();
            ShowMovementSquareSprites(reachableSquares);
            _cursor.SetMoveWithinBattleSquares();
        }
        
        private void ShowMovementSquareSprites(IEnumerable<Vector3Int> movementSquares) {
            _movementSquareSprites.Clear();
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
            _cursor.UnsetMoveWithinBattleSquares();
        }
        public EnumBattleState GetCurrentState() {
            return _currentBattleState;
        }

    }

    public enum EnumBattleState {
        freeCursor,
        unitSelected,
        unitMenu,
        overworldMenu,
    }
}
