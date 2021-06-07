using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Assets.Enums;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.Battle.WinLoseCondition;
using Assets.Scripts.CameraScripts;
using Assets.Scripts.EditorScripts;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using Assets.Scripts.Menus;
using Assets.Scripts.Menus.Battle;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle {
    public class BattleController : MonoBehaviour {
        public bool IsActive;

        private Unit _currentUnit;
        private Character _targetToSwapWith;
        private Queue<Unit> _turnOrder;
        private Vector3 _originalPosition;
        private LinkedList<Vector3> _linkedListTargetUnits;
        private LinkedListNode<Vector3> _llNodeCurrentTarget;
        private List<WinLoseConditionBase> _winLoseConditions;

        private RuntimeAnimatorController _animatorAttackButton;
        private RuntimeAnimatorController _animatorMagicButton;
        private RuntimeAnimatorController _animatorStayButton;
        private RuntimeAnimatorController _animatorItemButton;

        private RuntimeAnimatorController _animatorUseButton;
        private RuntimeAnimatorController _animatorGiveButton;
        private RuntimeAnimatorController _animatorDropButton;
        private RuntimeAnimatorController _animatorEquipButton;

        private readonly List<Unit> _force = new List<Unit>();
        private readonly List<Unit> _enemies = new List<Unit>();

        private Tilemap _terrainTileMap;
        private readonly List<GameObject> _movementSquareSprites = new List<GameObject>();
        private readonly List<Vector3> _movementSparesVector = new List<Vector3>();
        
        private EnumBattleState _currentBattleState;
        private DirectionType _inputDirection;
        private DirectionType _lastInputDirection;
        private EnumCurrentBattleMenu _enumCurrentMenuType = EnumCurrentBattleMenu.none;
        private EnumCurrentBattleMenu _previousMenuTypeState = EnumCurrentBattleMenu.none;
        private EnumCurrentMenu _currentItemMenu = EnumCurrentMenu.none;

        private LayerMask _layerMaskForce;

        private Transform _healthBars;
        private Transform _miniMapDots;
        private Transform _miniMapOverlay;
        private Transform _miniMapCamera;
        private MovementGrid _movementGrid;
        private Cursor _cursor;
        private Player _player;
        private OverviewCameraMovement _overviewCameraMovement;
        private FourWayButtonMenu _fourWayButtonMenu;
        private FourWayMagicMenu _fourWayMagicMenu;
        private QuickStats _quickStats;
        private AudioManager _audioManager;
        private DialogManager _dialogManager;
        private CharacterDetailUI _characterDetailUI;
        private AiController _aiController;
        private Inventory _inventory;
        private AttackPhase _attackPhase;
        private Menu _menu;
        private Magic _magicToAttack;
        private GameItem _selectedItem;
        private GameItem _itemToSwap;
        private GameObject _battleObjects;
        private FadeInOut _fadeInOut;
        private int _magicLevelToAttack;
        private string _previousTrackName;

        private bool _magicSelected = false;
        private bool _battleHasEnded = false;
        private bool _nextUnit = false;
        private bool _doWarp = false;

        public static BattleController Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            IsActive = false;

            _animatorAttackButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonAttack);
            _animatorMagicButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonMagic);
            _animatorStayButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonStay);
            _animatorItemButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonItem);

            _animatorUseButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonUse);
            _animatorGiveButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonGive);
            _animatorDropButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonDrop);
            _animatorEquipButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonEquip);

            _healthBars = transform.Find("HealthBars").GetComponent<Transform>();
            _miniMapDots = transform.Find("MiniMapDots").GetComponent<Transform>();
            _miniMapOverlay = transform.Find("MiniMap").GetComponent<Transform>();
            _miniMapCamera = GameObject.Find("MiniMapCamera").GetComponent<Transform>();

            _layerMaskForce = Constants.LayerMaskForce;
        }

        void Start() {
            _cursor = Cursor.Instance;
            _audioManager = AudioManager.Instance;
            _fourWayButtonMenu = FourWayButtonMenu.Instance;
            _fourWayMagicMenu = FourWayMagicMenu.Instance;
            _dialogManager = DialogManager.Instance;
            _player = Player.Instance;
            _overviewCameraMovement = OverviewCameraMovement.Instance;
            _characterDetailUI = CharacterDetailUI.Instance;
            _menu = Menu.Instance;
            _inventory = Inventory.Instance;
            _aiController = AiController.Instance;
            _quickStats = QuickStats.Instance;
            _attackPhase = AttackPhase.Instance;
            _fadeInOut = FadeInOut.Instance;

            transform.gameObject.SetActive(false);
            _healthBars.gameObject.SetActive(false);
            _miniMapOverlay.gameObject.SetActive(false);
            _miniMapCamera.gameObject.SetActive(false);
        }

        public void BeginBattle() {
            _fadeInOut.FadeIn(1);
            _battleObjects = GameObject.Find("BattleObjects");
            if (_battleObjects != null) {
                foreach (Transform t in _battleObjects.GetComponentsInChildren<Transform>(true)) {
                    if (t.name == "BattleObjectsEnable") {
                        t.gameObject.SetActive(true);
                    }
                }
            }

            LoadBattleData();
            _battleHasEnded = false;
            _menu.ObjectMenu.SetActive(false);
            _player?.gameObject.SetActive(false);
            _cursor.gameObject.SetActive(true);

            _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
            _movementGrid = new MovementGrid(_terrainTileMap);
            _aiController.SetMovementGrid(_movementGrid);
            _cursor.BeginBattle(_terrainTileMap);
            IsActive = true;

            _overviewCameraMovement.SetPlayerObject(_cursor.gameObject);
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
                var unit = child.GetComponent<EnemyUnit>();
                if (unit != null) {
                    _enemies.Add(unit);
                }
            }

            LoadPartyMembersAsForce();
            CreateAndAssignQuickHealthBars();
            SetNewTurnOrder();
            NextUnit();
        }

        public void SetEndBattle() {
            _battleHasEnded = true;
        }

        public void SetDoWarp() {
            _doWarp = true;
        }

        private void EndBattle() {
            StartCoroutine(EndBattleCoroutine());
        }

        public Unit GetCurrentUnit() {
            return _currentUnit;
        }

        public void SetNextUnit() {
            _nextUnit = true;
        }

        private void DoWarp() {
            var warpGameObject = new GameObject();
            warpGameObject.AddComponent<WarpToScene>();
            var warp = warpGameObject.GetComponent<WarpToScene>();
            //TODO Add sade jingle
            warp.sceneToWarpTo = _inventory.GetWarpSceneName();
            warp.DoWarp();
        }

        private void NextUnit() {
            if (CheckEndCondition()) {
                _battleHasEnded = true;
            }
            _nextUnit = false;
            if (_currentUnit != null) {
                _currentUnit.SetAnimatorDirection(DirectionType.down);
                _currentUnit.ClearUnitFlicker();
            }
            _enumCurrentMenuType = EnumCurrentBattleMenu.none;
            _previousMenuTypeState = EnumCurrentBattleMenu.none;
            _currentItemMenu = EnumCurrentMenu.none;
            DestroyMovementSquareSprites();
            //TODO also check for events, like respawn, other events
            if (_battleHasEnded) {
                EndBattle();
                return;
            }
            // Don't do this IF the Battle should already have ended.
            // so after EndBattle check.
            DoPoisonCheckAndDamage();

            if (_turnOrder.Count <= 0) {
                SetNewTurnOrder();
            }
            _currentUnit = _turnOrder.Dequeue();
            if (_currentUnit == null) {
                NextUnit();
                return;
            }
            _currentUnit.SetUnitFlicker();
            _originalPosition = _currentUnit.transform.position;
            _cursor.ReturnToUnit(_originalPosition);
            SetSelectedUnit(_currentUnit);
            if (_currentUnit is EnemyUnit enemyUnit) {
                _aiController.AiTurn(enemyUnit, _movementSparesVector);
            } else if (_currentUnit.GetCharacter().StatusEffects.HasFlag(EnumStatusEffect.confused)) {
                _aiController.ConfusedTurn(_currentUnit, _movementSparesVector);
            }
        }

        public List<Unit> GetEnemies() {
            return _enemies;
        }
        public List<Unit> GetForce() {
            return _force;
        }

        void Update() {
            if (Player.InputDisabledInDialogue || Player.IsInDialogue || Player.InputDisabledInEvent || Player.InputDisabledAiBattle
                || Player.InputDisabledInAttackPhase || !_cursor.UnitReached || Player.InWarp || Player.PlayerIsInMenu == EnumMenuType.pause) {
                _healthBars.gameObject.SetActive(false);
                _miniMapOverlay.gameObject.SetActive(false);
                _miniMapCamera.gameObject.SetActive(false);
                return;
            }

            if (_nextUnit) {
                NextUnit();
            }

            if (_battleHasEnded) {
                return;
            }

            if (Input.GetButtonDown("Special")) {
                _healthBars.gameObject.SetActive(true);
            }
            if (Input.GetButtonUp("Special")) {
                _healthBars.gameObject.SetActive(false);
            }
            if (Input.GetButtonDown("Select")) {
                _miniMapOverlay.gameObject.SetActive(true);
                _miniMapCamera.gameObject.SetActive(true);
            }
            if (Input.GetButtonUp("Select")) {
                _miniMapOverlay.gameObject.SetActive(false);
                _miniMapCamera.gameObject.SetActive(false);
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
                        HandleMagicMenu();
                        break;
                    case EnumCurrentBattleMenu.stay:
                        //Should not happen
                        break;
                    case EnumCurrentBattleMenu.item:
                        HandleObjectButtonMenu();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }

            if (Input.GetButtonUp("Back")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.ReturnToUnit(_originalPosition);
                        SetSelectedUnit(_currentUnit);
                        break;
                    case EnumBattleState.unitSelected:
                        DestroyMovementSquareSprites();
                        _cursor.ReturnToPosition(_movementSparesVector, _originalPosition, 20f);
                        _cursor.ClearControlUnit();
                        _currentBattleState = EnumBattleState.freeCursor;
                        break;
                    case EnumBattleState.characterDetails:
                        Player.PlayerIsInMenu = EnumMenuType.none;
                        _audioManager.PlaySFX(Constants.SfxMenuSwish);
                        _characterDetailUI.CloseCharacterDetailsUi();
                        _cursor.ClearControlUnit();
                        _currentBattleState = EnumBattleState.freeCursor;
                        break;
                }
            }

            if (Input.GetButtonUp("Interact")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        _cursor.CheckIfCursorIsOverUnit(out var unit,
                            LayerMask.GetMask("Force", "Enemies"));
                        if (_currentUnit == unit) {
                            SetSelectedUnit(_currentUnit);
                            _cursor.SetControlUnit(_currentUnit);
                        }

                        break;
                    case EnumBattleState.unitSelected:
                        var collider2D = _currentUnit.GetComponent<Collider2D>();
                        if (collider2D.IsTouchingLayers(LayerMask.GetMask("Force", "Enemies"))) {
                            _audioManager.PlaySFX(Constants.SfxError);
                            break;
                        }

                        DestroyMovementSquareSprites();
                        _audioManager.PlaySFX(Constants.SfxMenuSwish);
                        Player.PlayerIsInMenu = EnumMenuType.battleMenu;
                        _fourWayButtonMenu.InitializeButtons(_animatorAttackButton, _animatorMagicButton,
                            _animatorStayButton, _animatorItemButton,
                            "Attack", "Magic", "Stay", "Item");
                        if (!TryGetTargetsInRange(_currentUnit.GetCharacter().GetAttackRange())) {
                            _fourWayButtonMenu.SetDirection(DirectionType.down);
                        }

                        _currentBattleState = EnumBattleState.unitMenu;
                        break;
                }
            }

            if (Input.GetButtonUp("Menu")) {
                switch (_currentBattleState) {
                    case EnumBattleState.freeCursor:
                        if (_cursor.CheckIfCursorIsOverUnit(out var unit,
                            LayerMask.GetMask("Force", "Enemies"))) {
                            Player.PlayerIsInMenu = EnumMenuType.battleMenu;
                            _audioManager.PlaySFX(Constants.SfxMenuSwish);
                            _characterDetailUI.LoadCharacterDetails(unit.GetCharacter(), unit.GetFieldAnimationTexture2D());
                            _currentBattleState = EnumBattleState.characterDetails;
                        }
                        break;
                }
            }
        }

        private void HandleObjectButtonMenu() {
            if (_currentItemMenu != EnumCurrentMenu.none) {
                HandleItemMenu();
                return;
            }
            GetInputDirection();
            var currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);
            if (Input.GetButtonUp("Interact")) {
                if (!currentlyAnimatedButton.Equals("Give")) {
                    var items = _currentUnit.GetCharacter().GetInventory();
                    if (items.All(x => x.IsEmpty())) {
                        _dialogManager.EvokeSingleSentenceDialogue("Unit does not have any Items.");
                        return;
                    }
                }

                switch (currentlyAnimatedButton) {
                    case "Use":
                        _currentItemMenu = EnumCurrentMenu.use;
                        break;
                    case "Give":
                        _currentItemMenu = EnumCurrentMenu.give;
                        GenerateMovementSquaresForAction(_currentUnit.transform.position, EnumAttackRange.Melee);
                        break;
                    case "Drop":
                        _currentItemMenu = EnumCurrentMenu.drop;
                        break;
                    case "Equip":
                        _currentItemMenu = EnumCurrentMenu.equip;
                        _quickStats.ShowQuickInfo(_currentUnit.GetCharacter(), _currentUnit.GetCharacter().GetInventory()[0]);
                        break;
                }
                _fourWayMagicMenu.LoadMemberInventory(_currentUnit.GetCharacter());
                _fourWayButtonMenu.CloseButtons();
            }

            if (Input.GetButtonUp("Back")) {
                _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                _fourWayButtonMenu.InitializeButtons(_animatorAttackButton, _animatorMagicButton,
                    _animatorStayButton, _animatorItemButton,
                    "Attack", "Magic", "Stay", "Item");
            }
        }

        private void HandleMenu() {
            GetInputDirection();
            var currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

            if (Input.GetButtonUp("Interact")) {
                switch (currentlyAnimatedButton) {
                    case "Attack":
                        var character = _currentUnit.GetCharacter();
                        _magicToAttack = null;
                        if (TryInitializeAttack(character.GetAttackRange(), character.GetAttackAreaOfEffect())) {
                            _fourWayButtonMenu.CloseButtons();
                            _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                            _previousMenuTypeState = EnumCurrentBattleMenu.none;
                        }
                        break;
                    case "Magic":
                        var magic = _currentUnit.GetCharacter().GetMagic();
                        if (magic.All(x => x.IsEmpty())) {
                            _dialogManager.EvokeSingleSentenceDialogue("Unit does not have any Magic.");
                            return;
                        }
                        _enumCurrentMenuType = EnumCurrentBattleMenu.magic;
                        _fourWayMagicMenu.LoadMemberMagic(_currentUnit.GetCharacter());
                        var selectedMagic = _fourWayMagicMenu.GetSelectedMagic();
                        if (!selectedMagic.IsEmpty()) {
                            GenerateMovementSquaresForAction(_currentUnit.transform.position, selectedMagic.AttackRange[selectedMagic.CurrentLevel-1]);
                        }
                        _fourWayButtonMenu.CloseButtons();
                        break;
                    case "Stay":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                        _cursor.EndTurn();
                        DestroyMovementSquareSprites();
                        _fourWayButtonMenu.CloseButtons();
                        _currentBattleState = EnumBattleState.freeCursor;
                        Player.PlayerIsInMenu = EnumMenuType.none;
                        break;
                    case "Item":
                        _enumCurrentMenuType = EnumCurrentBattleMenu.item;
                        _fourWayButtonMenu.InitializeButtons(_animatorUseButton, _animatorGiveButton,
                            _animatorDropButton, _animatorEquipButton,
                            "Use", "Give", "Drop", "Equip");
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
                    _cursor.SelectNextTarget(_llNodeCurrentTarget.Value);
                    UpdateUnitDirectionToTarget();
                    break;
                case DirectionType.up:
                case DirectionType.left:
                    _llNodeCurrentTarget = _llNodeCurrentTarget.Previous ?? _llNodeCurrentTarget.List.Last;
                    _cursor.SelectNextTarget(_llNodeCurrentTarget.Value);
                    UpdateUnitDirectionToTarget();
                    break;
            }

            if (Input.GetButtonUp("Back")) {
                _enumCurrentMenuType = _previousMenuTypeState;
                if (_previousMenuTypeState == EnumCurrentBattleMenu.none) {
                    _fourWayButtonMenu.OpenButtons();
                } else {
                    _fourWayMagicMenu.OpenButtons();
                }
                _cursor.ClearAttackArea();
                _cursor.SetControlUnit(_currentUnit);
            }

            if(Input.GetButtonUp("Interact")) {
                _magicSelected = false;
                if (!_cursor.IsTargetSelected()) {
                    return;
                }

                if (_itemToSwap != null) {
                    _targetToSwapWith = _cursor.GetTargetsInAreaOfEffect(_layerMaskForce).FirstOrDefault().GetCharacter();
                    _fourWayMagicMenu.LoadMemberInventory(_targetToSwapWith);
                    _enumCurrentMenuType = EnumCurrentBattleMenu.item;
                    _cursor.ClearAttackArea();
                    _cursor.SetControlUnit(_currentUnit);
                    return;
                }

                var isReversedTargetSearch = IsReverseTarget(_magicToAttack);

                var targets = _cursor.GetTargetsInAreaOfEffect(
                    _movementGrid.GetOpponentLayerMask(_currentUnit, isReversedTargetSearch));

                var attackOption = new AttackOption(targets, null, null, 0, _magicToAttack, _magicLevelToAttack);
                ExecuteAttack(attackOption, _previousMenuTypeState);
            }
        }

        private void HandleMagicMenu() {
            GetInputDirection();
            if (_magicSelected) {
                HandleSelectedMagicMenu();
                return;
            }
            UpdateSelectedMagic();
            if (Input.GetButtonUp("Interact")) {
                _fourWayMagicMenu.SetItemNameOrange();
                _magicSelected = true;
            }

            if (Input.GetButtonUp("Back")) {
                DestroyMovementSquareSprites();
                _enumCurrentMenuType = EnumCurrentBattleMenu.none;
                _fourWayMagicMenu.CloseButtons();
                _fourWayButtonMenu.OpenButtons();
            }
        }

        private void HandleSelectedMagicMenu() {
            _fourWayMagicMenu.UpdateMagicLevel(_inputDirection);
            UpdateSelectedMagic();
            if (Input.GetButtonUp("Interact")) {
                _magicToAttack = _fourWayMagicMenu.GetSelectedMagic();
                _magicLevelToAttack = _fourWayMagicMenu.GetSelectedMagicLevel();
                if (_currentUnit.GetCharacter().CharStats.CurrentMp < _magicToAttack.ManaCost[_magicLevelToAttack-1]) {
                    _dialogManager.EvokeSingleSentenceDialogue("Not enough Mana!");
                    return;
                }
                if (TryInitializeAttack(_magicToAttack.AttackRange[_magicLevelToAttack-1], 
                    _magicToAttack.AreaOfEffect[_magicLevelToAttack-1], IsReverseTarget(_magicToAttack))) {
                    _fourWayMagicMenu.CloseButtons();
                    _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                    _previousMenuTypeState = EnumCurrentBattleMenu.magic;
                }
                _fourWayMagicMenu.UnSetItemNameOrange();
            }

            if (Input.GetButtonUp("Back")) {
                _fourWayMagicMenu.UnSetItemNameOrange();
                _magicSelected = false;
            }
        }

        private void UpdateSelectedMagic() {
            if (_inputDirection == DirectionType.none) {
                return;
            }
            // We don't need to update the whole Magic-object, that would reset the selectedMagicLevel to currentMax.
            if (!_magicSelected) {
                _fourWayMagicMenu.SelectMagic(_inputDirection);
            }
            var selectedMagic = _fourWayMagicMenu.GetSelectedMagic();
            var selectedMagicLevel = _fourWayMagicMenu.GetSelectedMagicLevel();
            if (!selectedMagic.IsEmpty()) {
                GenerateMovementSquaresForAction(_currentUnit.transform.position,
                    selectedMagic.AttackRange[selectedMagicLevel - 1]);
            }
        }

        private void HandleItemMenu() {
            GetInputDirection();
            _fourWayMagicMenu.SelectObject(_inputDirection);
            if (_inputDirection != DirectionType.none && _currentItemMenu == EnumCurrentMenu.use) {
                DestroyMovementSquareSprites();
                var selectedItem = _fourWayMagicMenu.GetSelectedGameItem();
                if (selectedItem is Consumable consumable) {
                    GenerateMovementSquaresForAction(_currentUnit.transform.position, consumable.Magic.AttackRange[consumable.MagicLevel-1]);
                }
            } else if (_inputDirection != DirectionType.none && _currentItemMenu == EnumCurrentMenu.equip) {
                var selectedItem = _fourWayMagicMenu.GetSelectedGameItem();
                _quickStats.ShowQuickInfo(_currentUnit.GetCharacter(), selectedItem);
            }

            if (Input.GetButtonUp("Interact")) {
                _selectedItem = _fourWayMagicMenu.GetSelectedGameItem();
                if (_currentItemMenu != EnumCurrentMenu.give && (_selectedItem == null || _selectedItem.IsEmpty())) {
                    _dialogManager.EvokeSingleSentenceDialogue($"Select an item.");
                    return;
                }
                switch (_currentItemMenu) {
                    case EnumCurrentMenu.use:
                        if (_selectedItem is Consumable consumable) {
                            _magicToAttack = consumable.Magic;
                            _magicLevelToAttack = consumable.MagicLevel;
                            if (TryInitializeAttack(_magicToAttack.AttackRange[_magicLevelToAttack - 1],
                                _magicToAttack.AreaOfEffect[_magicLevelToAttack - 1], IsReverseTarget(_magicToAttack))) {

                                _fourWayMagicMenu.CloseButtons();
                                _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                                _previousMenuTypeState = EnumCurrentBattleMenu.item;
                            }
                        } else {
                            _dialogManager.EvokeSingleSentenceDialogue($"{_selectedItem.ItemName} cannot be used.");
                        }
                        break;
                    case EnumCurrentMenu.give:
                        if (_itemToSwap == null) {
                            _itemToSwap = _selectedItem;
                            if (TryInitializeAttack(EnumAttackRange.Melee, EnumAreaOfEffect.Single, true)) {
                                _fourWayMagicMenu.CloseButtons();
                                _previousMenuTypeState = EnumCurrentBattleMenu.item;
                                _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                            }
                        } else {
                            if (_selectedItem == null) {
                                _selectedItem = Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
                            } 
                            _inventory.SwapItems((PartyMember)_currentUnit.GetCharacter(), (PartyMember)_targetToSwapWith,
                                    _itemToSwap, _selectedItem);
                            
                            DestroyMovementSquareSprites();
                            _quickStats.CloseQuickInfo();
                            _currentItemMenu = EnumCurrentMenu.none;
                            _fourWayMagicMenu.CloseButtons();
                            _fourWayButtonMenu.OpenButtons();
                            _targetToSwapWith = null;
                            _itemToSwap = null;
                        }
                        break;
                    case EnumCurrentMenu.drop:
                        if (_selectedItem.EnumItemType == EnumItemType.none) {
                            _dialogManager.EvokeSingleSentenceDialogue("Select an item to drop ...");
                            return;
                        }

                        var dropItemCallback = new QuestionCallback {
                            Name = "DropText",
                            Sentences = new List<string>() {
                                $"Do you want really to drop {_selectedItem.ItemName.AddColor(Color.green)}"
                            },
                            DefaultSelectionForQuestion = YesNo.No,
                            OnAnswerAction = DropItemAnswer,
                        };
                        _dialogManager.StartDialogue(dropItemCallback);
                        break;
                    case EnumCurrentMenu.equip:
                        if (_selectedItem is Equipment equipment) {
                            var character = _currentUnit.GetCharacter();
                            if (equipment.EquipmentForClass.Any(x => x == character.ClassType)) {
                                var oldEquipment = (Equipment)character.GetInventory().FirstOrDefault(
                                    x => x.EnumItemType == EnumItemType.equipment
                                         && ((Equipment)x).EquipmentType == equipment.EquipmentType
                                         && ((Equipment)x).IsEquipped);

                                if (equipment == oldEquipment) {
                                    character.CharStats.UnEquip(equipment);
                                    _dialogManager.EvokeSingleSentenceDialogue(
                                        $"{equipment.ItemName.AddColor(Color.green)} successfully unequipped.");
                                    _fourWayMagicMenu.ReloadSelection();
                                    _quickStats.ShowQuickInfo(character, equipment);
                                } else {
                                    character.CharStats.Equip(equipment);
                                    character.CharStats.UnEquip(oldEquipment);
                                    _dialogManager.EvokeSingleSentenceDialogue(
                                        $"{equipment.ItemName.AddColor(Color.green)} successfully equipped.");
                                    _fourWayMagicMenu.ReloadSelection();
                                    _quickStats.ShowQuickInfo(character, equipment);
                                }
                            } else {
                                _dialogManager.EvokeSingleSentenceDialogue(
                                    $"{character.Name.AddColor(Constants.Orange)} cannot equip " +
                                    $"{equipment.ItemName.AddColor(Color.green)}");
                            }
                        } else {
                            _dialogManager.EvokeSingleSentenceDialogue(
                                $"{_selectedItem.ItemName.AddColor(Color.green)} is not an Equipment");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (Input.GetButtonUp("Back")) {
                _itemToSwap = null;
                DestroyMovementSquareSprites();
                _quickStats.CloseQuickInfo();
                _currentItemMenu = EnumCurrentMenu.none;
                _fourWayMagicMenu.CloseButtons();
                _fourWayButtonMenu.OpenButtons();
            }
        }

        private void DropItemAnswer(bool answer) {
            if (answer) {
                _currentUnit.GetCharacter().RemoveItem(_selectedItem);
                _selectedItem = null;
                _fourWayMagicMenu.LoadMemberInventory(_currentUnit.GetCharacter());
            }
        }

        public void ExecuteAttack(AttackOption attackOption, EnumCurrentBattleMenu attackType) {
            _previousMenuTypeState = attackType;
            var itemName = "";
            if (attackType == EnumCurrentBattleMenu.item) {
                itemName = _selectedItem.ItemName;
                if (_selectedItem is Consumable) {
                    _currentUnit.GetCharacter().RemoveItem(_selectedItem);
                }
            }

            var bossAttack = false;
            if (_currentUnit is EnemyUnit enemyUnit) {
                bossAttack = enemyUnit.IsBoss;
            }
            _attackPhase.ExecuteAttackPhase(attackType, attackOption, _currentUnit, itemName, bossAttack);

            _enumCurrentMenuType = EnumCurrentBattleMenu.none;
            _cursor.ClearAttackArea();
            Player.PlayerIsInMenu = EnumMenuType.none;
            _cursor.EndTurn();
        }

        public void HealWholeForce() {
            var leaders = _inventory.GetParty().Where(x => x.partyLeader);
            foreach (var leader in leaders) {
                leader.StatusEffects = leader.StatusEffects.Remove(EnumStatusEffect.dead);
                leader.FullyHeal();
            }
            foreach (var unit in _force) {
                unit.GetCharacter().FullyHeal();
            }
        }

        private void DestroyAllSprites() {
            foreach (var unit in _force) {
                if (unit != null && unit.gameObject != null) {
                    Destroy(unit.gameObject);
                }
            }
            foreach (var unit in _enemies) {
                if (unit != null && unit.gameObject != null) {
                    Destroy(unit.gameObject);
                }
            }
        }

        public void RemoveUnitFromBattle(Unit target) {
            _enemies.Remove(target);
            _force.Remove(target);
            _turnOrder = new Queue<Unit>(_turnOrder.Where(x => x != target));
            target.KillUnit();
        }

        private void LoadPartyMembersAsForce() {
            var unitsToRemove = new List<Unit>();
            var activeAlivePartyMembersQueue = new Queue<PartyMember>(_inventory.GetParty().Where(
                x => x.activeParty && 
                     !x.StatusEffects.HasFlag(EnumStatusEffect.dead)));
            foreach (var forceUnit in _force) {
                if (activeAlivePartyMembersQueue.Count != 0) {
                    forceUnit.SetCharacter(activeAlivePartyMembersQueue.Dequeue());
                } else {
                    unitsToRemove.Add(forceUnit);
                    Destroy(forceUnit.gameObject);
                }
            }
            foreach (var removeUnit in unitsToRemove) {
                _force.Remove(removeUnit);
            }
        }

        private bool TryInitializeAttack(EnumAttackRange attackRange, EnumAreaOfEffect areaOfEffect, bool reverseTargets = false) {
            GenerateMovementSquaresForAction(_currentUnit.transform.position, attackRange);
            if (!TryGetTargetsInRange(attackRange, reverseTargets)) {
                _dialogManager.EvokeSingleSentenceDialogue("There are no Targets in Range.");
                return false;
            }
            _cursor.ClearControlUnit(true);
            _llNodeCurrentTarget = _linkedListTargetUnits.First;
            _cursor.SetAttackArea(_llNodeCurrentTarget.Value, areaOfEffect);
            UpdateUnitDirectionToTarget();
            return true;
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

        private bool TryGetTargetsInRange(EnumAttackRange attackRange, bool reverseTargets = false) {
            var reachableSquares = _movementGrid.GetMovementPointsAreaOfEffect(
                _currentUnit.transform.position, attackRange).ToList();
            var reachableSquares2 = reachableSquares.Select(x => {
                var vector3 = new Vector3 {
                    x = x.x + 0.5f,
                    y = x.y + 0.5f
                };
                return vector3;
            }).ToList();
            var opponentLayerMask = _movementGrid.GetOpponentLayerMask(_currentUnit, reverseTargets);
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
            SetSelectedUnit(_currentUnit);
            _fourWayButtonMenu.CloseButtons();
            _currentBattleState = EnumBattleState.unitSelected;
            Player.PlayerIsInMenu = EnumMenuType.none;
        }

        private void SetNewTurnOrder() {
            var unitList = new List<Tuple<Unit, float>>();
            foreach (var enemy in _enemies) {
                //TODO Bosses double turn?
                var agility = GetRandomAgilityValue(enemy);
                unitList.Add(new Tuple<Unit, float>(enemy,agility));
                if (enemy is EnemyUnit enemyUnit) {
                    if (enemyUnit.IsBoss) {
                        unitList.Add(new Tuple<Unit, float>(enemy, agility / 2));
                    }
                }
            }
            foreach (var force in _force) {
                //TODO Bosses double turn?
                var agility = GetRandomAgilityValue(force);
                unitList.Add(new Tuple<Unit, float>(force, agility));
            }
            unitList.Sort((x,y) => -(x.Item2.CompareTo(y.Item2)));
            _turnOrder?.Clear();
            _turnOrder = new Queue<Unit>(unitList.Select(x => x.Item1));
        }

        private float GetRandomAgilityValue(Unit unit) {
            return (unit.GetCharacter().CharStats.Agility.GetModifiedValue() * Random.Range(0.875f, 1.125f)) 
                   + Random.Range(-1, 2); //Max is exclusive... so -1, 0, or 1
        }

        private void SetSelectedUnit(Unit selectedUnit) {
            _currentUnit = selectedUnit;
            GenerateMovementSquaresForUnit();
            _currentBattleState = EnumBattleState.unitSelected;
        }

        private void GenerateMovementSquaresForUnit() {
            DestroyMovementSquareSprites();
            var reachableSquares = _movementGrid.GetMovementPointsOfUnit(_currentUnit, _originalPosition).ToList();
            ShowMovementSquareSprites(reachableSquares);
            _cursor.SetMoveWithinBattleSquares();
            _currentUnit.ClearUnitFlicker();
        }

        public void GenerateMovementSquaresForAction(Vector3 currentPosition, EnumAttackRange attackRange) {
            DestroyMovementSquareSprites();
            var reachableSquares = _movementGrid.GetMovementPointsAreaOfEffect(currentPosition, attackRange).ToList();
            ShowMovementSquareSprites(reachableSquares);
            _cursor.SetMoveWithinBattleSquares();
        }

        private void ShowMovementSquareSprites(IEnumerable<Vector3Int> movementSquares) {
            _movementSquareSprites.Clear();
            _movementSparesVector.Clear();
            var square = Resources.Load<GameObject>(Constants.PrefabMovementSquare);
            foreach (var movementSquare in movementSquares) {
                _movementSquareSprites.Add(Instantiate(square,
                    new Vector3(movementSquare.x + .5f, movementSquare.y + .5f), Quaternion.identity));
                _movementSparesVector.Add(new Vector3(movementSquare.x + .5f, movementSquare.y + .75f));
            }
        }
        private void DestroyMovementSquareSprites() {
            foreach (var sprite in _movementSquareSprites) {
                Destroy(sprite);
            }
            _cursor.UnsetMoveWithinBattleSquares();
        }

        public EnumBattleState GetCurrentState() {
            return _currentBattleState;
        }

        public bool IsReverseTarget(Magic magic) {
            if (magic == null) {
                return false;
            }
            return magic.MagicType == EnumMagicType.Heal ||
                   magic.MagicType == EnumMagicType.Special ||
                   magic.MagicType == EnumMagicType.Buff ||
                   magic.MagicType == EnumMagicType.Cure ||
                   magic.MagicType == EnumMagicType.RestoreMP ||
                   magic.MagicType == EnumMagicType.RestoreBoth;
        }

        private void CreateAndAssignQuickHealthBars() {
            foreach (var enemy in _enemies) {
                var healthBar = Instantiate(Resources.Load<HealthbarTracker>(Constants.PrefabQuickHealthBar));
                var miniMapDot = Instantiate(Resources.Load<MiniMapDot>(Constants.PrefabMiniMapDot));
                healthBar.transform.SetParent(_healthBars);
                miniMapDot.transform.SetParent(_miniMapDots);
                healthBar.SetTarget(enemy);
                miniMapDot.SetTargetAndColor(enemy, Color.red);
            }
            foreach (var forceUnit in _force) {
                var healthBar = Instantiate(Resources.Load<HealthbarTracker>(Constants.PrefabQuickHealthBar));
                var miniMapDot = Instantiate(Resources.Load<MiniMapDot>(Constants.PrefabMiniMapDot));
                healthBar.transform.SetParent(_healthBars);
                miniMapDot.transform.SetParent(_miniMapDots);
                healthBar.SetTarget(forceUnit);
                miniMapDot.SetTargetAndColor(forceUnit, Color.cyan);
            }
        }

        private void LoadBattleData() {
            var battleData = GameObject.Find("BattleData");
            _previousTrackName = _audioManager.GetCurrentTrackName();
            if (battleData) {
                var winLoseCondition = battleData.transform.Find("WinLoseCondition");
                var soundFile = battleData.transform.Find("BattleAudioFile");
                if (winLoseCondition) {
                    _winLoseConditions = winLoseCondition.GetComponents<WinLoseConditionBase>().ToList();
                    if (_winLoseConditions.Count == 0) {
                        LoadFallbackWinLoseCondition();
                    }
                } else {
                    LoadFallbackWinLoseCondition();
                }

                if (soundFile) {
                    var soundFileName = string.IsNullOrEmpty(soundFile.GetChild(0)?.name) ? "battle1" : soundFile.GetChild(0).name;
                    _audioManager.StopAll();
                    var returnValue = _audioManager.Play(soundFileName);
                    if (Math.Abs(returnValue) < 0.0001f) {
                        _audioManager.Play("battle1");
                    }
                } else {
                    LoadFallbackAudioFile();
                }
                return;
            }
            LoadFallbackWinLoseCondition();
            LoadFallbackAudioFile();
        }

        private void LoadFallbackWinLoseCondition() {
            _winLoseConditions = new List<WinLoseConditionBase>() {
                new WinLoseConditionBase()
            };
        }

        private void LoadFallbackAudioFile() {
            _audioManager.StopAll();
            _audioManager.Play("battle1");
        }

        private bool CheckEndCondition() {
            var endBattle = false;
            foreach (var winLose in _winLoseConditions) {
                endBattle |= winLose.CheckLoseCondition();
                endBattle |= winLose.CheckWinCondition();
            }
            return endBattle;
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
                    _audioManager.PlaySFX(Constants.SfxMenuDing);
                }
            }

            if (Input.GetButtonUp("Back") || Input.GetButtonUp("Interact")) {
                _audioManager.PlaySFX(Constants.SfxMenuSwish);
            }
        }

        private void DoPoisonCheckAndDamage() {
            if (_currentUnit != null) {
                var character = _currentUnit.GetCharacter();
                if (character.StatusEffects.HasFlag(EnumStatusEffect.poisoned)) {
                    var poisonDamage = character.CharStats.MaxHp() / 10;
                    poisonDamage = poisonDamage <= 2 ? 2 : poisonDamage;
                    var isDead = character.CharStats.CurrentHp <= poisonDamage;
                    var sentences = new List<string>();
                    sentences.Add(
                        $"{character.Name.AddColor(Constants.Orange)} suffered {poisonDamage} poison damage.");
                    if (isDead) {
                        sentences.Add($"{character.Name.AddColor(Constants.Orange)} died!");
                        RemoveUnitFromBattle(_currentUnit);
                    }
                    else {
                        character.CharStats.CurrentHp -= poisonDamage;
                    }

                    _dialogManager.EvokeSentenceDialogue(sentences);
                }
            }
        }


        IEnumerator EndBattleCoroutine() {
            if (_doWarp) {
                _doWarp = false;
                DoWarp();
            } else { 
                _fadeInOut.FadeOutAndThenBackIn(2.75f);
            }

            yield return new WaitForSeconds(0.5f);
            HealWholeForce();
            DestroyAllSprites();
            _nextUnit = false;
            Player.InputDisabledAiBattle = false;

            BattleAnimationUi.Instance.EndAttackAnimation();
            _turnOrder.Clear();
            _force.Clear();
            _enemies.Clear();

            _currentUnit = null;
            _menu.ObjectMenu.SetActive(false);
            IsActive = false;
            _battleHasEnded = false;
            _fourWayButtonMenu.CloseButtons();
            _fourWayMagicMenu.CloseButtons();
            _cursor.EndBattle();
            _player.gameObject.SetActive(true);
            _cursor.gameObject.SetActive(false);
            _overviewCameraMovement.SetPlayerObject(_player.gameObject);
            _audioManager.StopAll();
            var audioFileToPlay = string.IsNullOrEmpty(_previousTrackName) ? "Town" : _previousTrackName;
            _audioManager.Play(audioFileToPlay);

            if (_battleObjects != null) {
                foreach (Transform t in _battleObjects.GetComponentsInChildren<Transform>(true)) {
                    if (t.name == "BattleObjectsEnable") {
                        t.gameObject.SetActive(false);
                    }
                }
            }
            gameObject.SetActive(false);
        }
    }

    public enum EnumBattleState {
        freeCursor,
        unitSelected,
        unitMenu,
        attack,
        magic,
        item,
        characterDetails,
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
