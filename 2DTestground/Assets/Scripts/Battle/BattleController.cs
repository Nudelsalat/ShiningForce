using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
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
        private LinkedList<Vector3> _linkedListTargetUnits;
        private LinkedListNode<Vector3> _llNodeCurrentTarget;

        private RuntimeAnimatorController _animatorAttackButton;
        private RuntimeAnimatorController _animatorMagicButton;
        private RuntimeAnimatorController _animatorStayButton;
        private RuntimeAnimatorController _animatorItemButton;

        private readonly List<Unit> _force = new List<Unit>();
        private readonly List<Unit> _enemies = new List<Unit>();

        private Tilemap _terrainTileMap;
        private readonly List<GameObject> _movementSquareSprites = new List<GameObject>();
        private bool _inBattle = false;
        private int _selectedTargetId = 0;

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
        private DialogManager _dialogManager;
        private BattleCalculator _battleCalculator;

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
            _dialogManager = DialogManager.Instance;
            _battleCalculator = new BattleCalculator();

            _terrainTileMap.color = Constants.Invisible;
            transform.gameObject.SetActive(false);
            //TODO BeginBattle will be evoke via event, not automatically.
            BeginBattle();
        }

        void Update() {
            if (Player.InputDisabledInDialogue) {
                return;
            }
            if (_currentBattleState == EnumBattleState.unitMenu) {
                switch (_enumCurrentMenuType) {
                    case EnumCurrentBattleMenu.none:
                        HandleMenu();
                        break;
                    case EnumCurrentBattleMenu.attack:
                        HandleAttackState();
                        break;
                    case EnumCurrentBattleMenu.magic:
                        //TODO
                        break;
                    case EnumCurrentBattleMenu.stay:
                        //Should not happen
                        break;
                    case EnumCurrentBattleMenu.item:
                        //TODO could/should reuse parts of ItemHandle <- refactor into own class
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                        if (!TryGetTargetsInRange()) {
                            _fourWayButtonMenu.SetDirection(DirectionType.down);
                        }
                        _currentBattleState = EnumBattleState.unitMenu;
                        break;
                }
            }
        }

        private void HandleMenu() {
            GetInputDirection();
            var currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

            if (Input.GetButtonUp("Interact")) {
                switch (currentlyAnimatedButton) {
                    case "Attack":
                        if (!TryGetTargetsInRange()) {
                            _dialogManager.EvokeSingleSentenceDialogue("There are no Targets in Range.");
                            break;
                        }
                        _cursor.ClearControlUnit(true);
                        _llNodeCurrentTarget = _linkedListTargetUnits.First;
                        _cursor.MovePoint.position = _llNodeCurrentTarget.Value;
                        UpdateUnitDirectionToTarget();
                        DestroyMovementSquareSprites();
                        GenerateMovementSquaresForAction(_currentUnit.transform.position,
                            _currentUnit.GetCharacter().GetAttackRange());
                        _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                        _fourWayButtonMenu.CloseButtons();
                        break;
                    case "Magic":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.magic;
                        //TODO spellSelector -> same as Attack
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
                        //TODO -> reuse Object menu, and introduce _inBattle flag?
                        _fourWayButtonMenu.CloseButtons();
                        break;
                }
            }

            if (Input.GetButtonUp("Back")) {
                CloseMenu();
            }
        }

        private void HandleAttackState() {
            GetInputDirection();
            switch (_inputDirection) {
                case DirectionType.down:
                case DirectionType.right:
                    _llNodeCurrentTarget = _llNodeCurrentTarget.Next ?? _llNodeCurrentTarget.List.First;
                    _cursor.MovePoint.position = _llNodeCurrentTarget.Value;
                    UpdateUnitDirectionToTarget();
                    break;
                case DirectionType.up:
                case DirectionType.left:
                    _llNodeCurrentTarget = _llNodeCurrentTarget.Previous ?? _llNodeCurrentTarget.List.Last;
                    _cursor.MovePoint.position = _llNodeCurrentTarget.Value;
                    UpdateUnitDirectionToTarget();
                    break;
            }

            if (Input.GetButtonUp("Back")) {
                _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                _cursor.SetControlUnit(_currentUnit);
                _fourWayButtonMenu.OpenButtons();
                DestroyMovementSquareSprites();
                GenerateMovementSquaresForUnit();
            }

            if(Input.GetButtonUp("Interact")) {
                //TODO
                Unit target;
                var sentence = new List<string>();
                _cursor.CheckIfCursorIsOverUnit(out target, LayerMask.GetMask("Force", "Enemies"));
                var damage = _battleCalculator.GetBaseDamageWeaponAttack(
                    _currentUnit, target, _cursor.GetLandEffect());
                var isCrit = _battleCalculator.RollForCrit(_currentUnit);
                if (isCrit) {
                    damage = _battleCalculator.GetCritDamage(_currentUnit, damage);
                    sentence.Add("Critical hit!");
                }
                sentence.Add($"{target.GetCharacter().Name.AddColor(Constants.Orange)} suffered {damage}" +
                             $"points of damage.");
                if (target.GetCharacter().CharStats.CurrentHp <= damage) {
                    sentence.Add($"{target.GetCharacter().Name.AddColor(Constants.Orange)}" +
                                 $" was defeated!");
                    target.GetCharacter().CharStats.CurrentHp = 0;
                    //TODO set to destroy. Give EXP if _current unit == force.
                }
                else {
                    target.GetCharacter().CharStats.CurrentHp -= damage;
                }
                _dialogManager.EvokeSentenceDialogue(sentence);
                _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                NextUnit();
            }

        }

        private void UpdateUnitDirectionToTarget() {
            var direction = DirectionType.down;
            var xValue = _currentUnit.transform.position.x - _cursor.MovePoint.position.x;
            var yValue = (_currentUnit.transform.position.y - 0.25) - _cursor.MovePoint.position.y;
            if (Math.Abs(xValue) < Math.Abs(yValue)) {
                direction = yValue > 0 ? DirectionType.down : DirectionType.up;
            } else {
                direction = xValue > 0 ? DirectionType.left : DirectionType.right;
            }
            _currentUnit.SetAnimatorDirection(direction);
        }

        private bool TryGetTargetsInRange() {
            var reachableSquares = _movementGrid.GetMovementPointsAreaOfEffect(
                _currentUnit.transform.position, _currentUnit.GetCharacter().GetAttackRange()).ToList();
            var reachableSquares2 = reachableSquares.Select(x => {
                var vector3 = new Vector3 {
                    x = x.x + 0.5f,
                    y = x.y + 0.5f
                };
                return vector3;
            }).ToList();
            var opponentLayerMask = _movementGrid.GetOpponentLayerMask(_currentUnit);
            _selectedTargetId = 0;
            _linkedListTargetUnits = new LinkedList<Vector3>();
            foreach (var vector3 in reachableSquares2) {
                if (_movementGrid.IsOccupiedByOpponent(vector3.x, vector3.y, opponentLayerMask)) {
                    _linkedListTargetUnits.AddLast(new LinkedListNode<Vector3>(vector3));
                }
            }

            if (_linkedListTargetUnits.Any()) {
                return true;
            }
            return false;
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
            DestroyMovementSquareSprites();
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
            return (unit.GetCharacter().CharStats.Agility.GetModifiedValue() * Random.Range(0.875f, 1.125f)) 
                   + Random.Range(-1, 2); //Max is exclusive... so -1, 0, or 1
        }

        public void SetSelectedUnit(Unit selectedUnit) {
            _selectedUnit = selectedUnit;
            _currentUnit = selectedUnit;
            GenerateMovementSquaresForUnit();
            _currentBattleState = EnumBattleState.unitSelected;
        }

        private void GenerateMovementSquaresForUnit() {
            var reachableSquares = _movementGrid.GetMovementPointsOfUnit(_currentUnit, _originalPosition).ToList();
            ShowMovementSquareSprites(reachableSquares);
            _cursor.SetMoveWithinBattleSquares();
            _currentUnit.ClearUnitFlicker();
        }

        private void GenerateMovementSquaresForAction(Vector3 currentPosition, EnumAttackRange attackRange) {
            var reachableSquares = _movementGrid.GetMovementPointsAreaOfEffect(currentPosition, attackRange).ToList();
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
