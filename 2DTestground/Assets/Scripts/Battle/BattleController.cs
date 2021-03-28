using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.Menus;
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

        private RuntimeAnimatorController _animatorAttackButton;
        private RuntimeAnimatorController _animatorMagicButton;
        private RuntimeAnimatorController _animatorStayButton;
        private RuntimeAnimatorController _animatorItemButton;

        private readonly List<Unit> _force = new List<Unit>();
        private readonly List<Unit> _enemies = new List<Unit>();

        private Tilemap _terrainTileMap;
        private float _z;
        private readonly List<GameObject> _movementSquareSprites = new List<GameObject>();
        private bool _inBattle = false;

        private MovementGrid _movementGrid;
        private Cursor _cursor;
        private EnumBattleState _currentBattleState;
        private DirectionType _inputDirection;
        private DirectionType _lastInputDirection;
        private EnumCurrentBattleMenu _enumCurrentMenuType = EnumCurrentBattleMenu.none;
        private AudioClip _menuDing;
        private AudioClip _menuSwish;
        private AudioClip _error;

        private FourWayButtonMenu _fourWayButtonMenu;
        private AudioManager _audioManager;

        public static BattleController Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _animatorAttackButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonAttack);
            _animatorMagicButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonMagic);
            _animatorStayButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonStay);
            _animatorItemButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonItem);

            _menuSwish = Resources.Load<AudioClip>(Constants.SoundMenuSwish);
            _menuDing = Resources.Load<AudioClip>(Constants.SoundMenuDing);
            _error = Resources.Load<AudioClip>(Constants.SoundError);
        }

        void Start() {
            _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
            _movementGrid = new MovementGrid(_terrainTileMap);
            _cursor = Cursor.Instance;
            _audioManager = AudioManager.Instance;
            _fourWayButtonMenu = FourWayButtonMenu.Instance;

            _terrainTileMap.color = Constants.Invisible;
            transform.gameObject.SetActive(false);
            //TODO BeginBattle will be evoke via event, not automatically.
            BeginBattle();
        }

        void Update() {
            if (_currentBattleState == EnumBattleState.unitMenu) {
                HandleMenu();
                return;
            }


            if (Input.GetButtonUp("Back")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.ReturnToUnit(_originalPosition, _currentUnit);
                        SetSelectedUnit(_currentUnit);
                        break;
                    case EnumBattleState.unitSelected:
                        DestroyMovementSquareSprites();
                        _cursor.ReturnToPosition(_movementSquareSprites, _originalPosition);
                        _cursor.ClearControlUnit();
                        _currentBattleState = EnumBattleState.freeCursor;
                        break;
                }
            }
            if (Input.GetButtonUp("Interact")) {
                Unit unit;
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.CheckIfCursorIsOverUnit(out unit,
                            LayerMask.GetMask("Force", "Enemies"));
                        if (_currentUnit == unit) {
                            SetSelectedUnit(_currentUnit);
                            _cursor.SetControlUnit(_currentUnit);
                        }
                        break;
                    case EnumBattleState.unitSelected:
                        var collider2D = _currentUnit.GetComponent<Collider2D>();
                        if (collider2D.IsTouchingLayers(LayerMask.GetMask("Force", "Enemies"))) {
                            _audioManager.PlaySFX(_error);
                            break;
                        }
                        _audioManager.PlaySFX(_menuSwish);
                        _cursor.PlayerIsInMenu = EnumMenuType.battleMenu;
                        _fourWayButtonMenu.InitializeButtons(_animatorAttackButton, _animatorMagicButton,
                            _animatorStayButton, _animatorItemButton,
                            "Attack", "Magic", "Stay", "Item");
                        _currentBattleState = EnumBattleState.unitMenu;
                        break;
                }
            }
        }

        private void HandleMenu() {
            GetInputDirection();
            var _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

            if (Input.GetButtonUp("Interact")) {
                switch (_currentlyAnimatedButton) {
                    case "Attack":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                        //Todo
                        _fourWayButtonMenu.CloseButtons();
                        break;
                    case "Magic":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.magic;
                        //TODO
                        _fourWayButtonMenu.CloseButtons();
                        break;
                    case "Stay":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                        _cursor.EndTurn();
                        DestroyMovementSquareSprites();
                        _fourWayButtonMenu.CloseButtons();
                        _currentBattleState = EnumBattleState.freeCursor;
                        _cursor.PlayerIsInMenu = EnumMenuType.none;
                        break;
                    case "Item":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.item;
                        //TODO
                        _fourWayButtonMenu.CloseButtons();
                        break;
                }
            }

            if (Input.GetButtonUp("Back")) {
                CloseMenu();
            }
        }

        private void CloseMenu() {
            _fourWayButtonMenu.CloseButtons();
            _currentBattleState = EnumBattleState.unitSelected;
            _cursor.PlayerIsInMenu = EnumMenuType.none;
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

        public Unit GetCurrentUnit() {
            return _currentUnit;
        }

        public void NextUnit() {
            _currentUnit?.ClearUnitFlicker();
            //TODO CHECK win condition. (here?)
            //TODO also check for events, like respawn, other events

            //TODO remove player agency
            if (_turnOrder.Count <= 0) {
                SetNewTurnOrder();
            }
            _currentUnit = _turnOrder.Dequeue();
            _currentUnit.SetUnitFlicker();
            _originalPosition = _currentUnit.transform.position;
            _cursor.ReturnToUnit(_originalPosition, _currentUnit);
            SetSelectedUnit(_currentUnit);
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
            _selectedUnit.ClearUnitFlicker();
            _currentUnit = selectedUnit;
            GenerateMovementSquares();
            _currentBattleState = EnumBattleState.unitSelected;
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

        private void GetInputDirection() {
            var currentDirection = DirectionType.none;
            if (Input.GetAxisRaw("Vertical") > 0.05f) {
                currentDirection = DirectionType.up;
            } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
                currentDirection = DirectionType.left;
            } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
                currentDirection = DirectionType.down;
            } else if (Input.GetAxisRaw("Horizontal") > 0.05f) {
                currentDirection = DirectionType.right;
            } else {
                _inputDirection = DirectionType.none;
            }

            if (currentDirection == _lastInputDirection) {
                _inputDirection = DirectionType.none;
            } else {
                _lastInputDirection = _inputDirection = currentDirection;
                if (_inputDirection != DirectionType.none) {
                    _audioManager.PlaySFX(_menuDing);
                }
            }

            if (Input.GetButtonUp("Back") || Input.GetButtonUp("Interact")) {
                _audioManager.PlaySFX(_menuSwish);
            }

        }

    }

    public enum EnumBattleState {
        freeCursor,
        unitSelected,
        unitMenu,
        attack,
        magic,
        item,
        overworldMenu,
    }

    public enum EnumCurrentBattleMenu {
        none,
        attack,
        magic,
        stay,
        item
    }
}
