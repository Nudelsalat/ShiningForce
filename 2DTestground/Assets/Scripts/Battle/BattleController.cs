﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.Menus;
using Assets.Scripts.Menus.Battle;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle {
    public class BattleController : MonoBehaviour {
        public bool IsActive;

        private Unit _currentUnit;
        private Queue<Unit> _turnOrder;
        private Vector3 _originalPosition;
        private LinkedList<Vector3> _linkedListTargetUnits;
        private LinkedListNode<Vector3> _llNodeCurrentTarget;

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

        private MovementGrid _movementGrid;
        private Cursor _cursor;
        private Player _player;
        private OverviewCameraMovement _overviewCameraMovment;
        private FourWayButtonMenu _fourWayButtonMenu;
        private FourWayMagicMenu _fourWayMagicMenu;
        private AudioManager _audioManager;
        private DialogManager _dialogManager;
        private BattleCalculator _battleCalculator;
        private CharacterDetailUI _characterDetailUI;
        private AiController _aiController;
        private Inventory _inventory;
        private Menu _menu;
        private Magic _magicToAttack;
        private GameItem _selectedItem;
        private int _magicLevelToAttack;

        private bool _magicSelected = false;

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

            _layerMaskForce = Constants.LayerMaskForce;
        }

        void Start() {
            _cursor = Cursor.Instance;
            _audioManager = AudioManager.Instance;
            _fourWayButtonMenu = FourWayButtonMenu.Instance;
            _fourWayMagicMenu = FourWayMagicMenu.Instance;
            _dialogManager = DialogManager.Instance;
            _player = Player.Instance;
            _overviewCameraMovment = OverviewCameraMovement.Instance;
            _characterDetailUI = CharacterDetailUI.Instance;
            _menu = Menu.Instance;
            _inventory = Inventory.Instance;
            _aiController = AiController.Instance;
            _battleCalculator = new BattleCalculator();
            transform.gameObject.SetActive(false);
        }

        public void BeginBattle() {
            _menu.ObjectMenu.SetActive(false);
            _player?.gameObject.SetActive(false);
            _cursor.gameObject.SetActive(true);

            _terrainTileMap = GameObject.Find("Terrain").GetComponent<Tilemap>();
            _movementGrid = new MovementGrid(_terrainTileMap);
            _aiController.SetMovementGrid(_movementGrid);
            _terrainTileMap.color = Constants.Invisible;
            _cursor.BeginBattle(_terrainTileMap);
            IsActive = true;

            _overviewCameraMovment.SetPlayerObject(_cursor.gameObject);
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
            SetNewTurnOrder();
            NextUnit();
        }

        public void EndBattle() {
            HealWholeForce();

            _turnOrder.Clear();
            _force.Clear();
            _enemies.Clear();

            _currentUnit = null;
            _menu.ObjectMenu.SetActive(false);
            IsActive = false;
            _fourWayButtonMenu.CloseButtons();
            _fourWayMagicMenu.CloseButtons();
            _cursor.EndBattle();
            _player.gameObject.SetActive(true);
            _cursor.gameObject.SetActive(false);
            _overviewCameraMovment.SetPlayerObject(_player.gameObject);

            gameObject.SetActive(false);
        }

        public Unit GetCurrentUnit() {
            return _currentUnit;
        }

        public void NextUnit() {
            _currentUnit?.SetAnimatorDirection(DirectionType.down);
            _enumCurrentMenuType = EnumCurrentBattleMenu.none;
            _previousMenuTypeState = EnumCurrentBattleMenu.none;
            _currentItemMenu = EnumCurrentMenu.none;
            DestroyMovementSquareSprites();
            _currentUnit?.ClearUnitFlicker();
            //TODO CHECK win condition. (here?)
            //TODO also check for events, like respawn, other events

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
                || !_cursor.UnitReached || Player.PlayerIsInMenu == EnumMenuType.pause) {
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
                            _characterDetailUI.LoadCharacterDetails(unit.GetCharacter());
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
                var items = _currentUnit.GetCharacter().GetInventory();
                if (items.All(x => x.IsEmpty())) {
                    _dialogManager.EvokeSingleSentenceDialogue("Unit does not have any Items.");
                    return;
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
                        var items = _currentUnit.GetCharacter().GetInventory();
                        if (items.All(x => x.IsEmpty())) {
                            _dialogManager.EvokeSingleSentenceDialogue("Unit does not have any Items.");
                            return;
                        }
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

                var isReversedTargetSearch = IsReverseTarget(_magicToAttack);

                var targets = _cursor.GetTargetsInAreaOfEffect(
                    _movementGrid.GetOpponentLayerMask(_currentUnit, isReversedTargetSearch));
                ExecuteAttack(targets, _magicToAttack, _magicLevelToAttack);
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
            }
            if (Input.GetButtonUp("Interact")) {
                _selectedItem = _fourWayMagicMenu.GetSelectedGameItem();
                if (_selectedItem.IsEmpty()) {
                    _dialogManager.EvokeSingleSentenceDialogue($"{_selectedItem.ItemName} cannot be used.");
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
                        if (TryInitializeAttack(EnumAttackRange.Melee, EnumAreaOfEffect.Single, true)) {
                            _fourWayMagicMenu.CloseButtons();
                            _enumCurrentMenuType = EnumCurrentBattleMenu.attack;
                            _previousMenuTypeState = EnumCurrentBattleMenu.item;
                        }
                        break;
                    case EnumCurrentMenu.drop:
                        //TODO the whole drop mechanic.
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
                                    //TODO Stat-window change
                                } else {
                                    character.CharStats.Equip(equipment);
                                    character.CharStats.UnEquip(oldEquipment);
                                    _dialogManager.EvokeSingleSentenceDialogue(
                                        $"{equipment.ItemName.AddColor(Color.green)} successfully equipped.");
                                    _fourWayMagicMenu.ReloadSelection();
                                    //TODO Stat-window change
                                }
                            }
                            _dialogManager.EvokeSingleSentenceDialogue(
                                $"{character.Name.AddColor(Constants.Orange)} cannot equip " +
                                $"{equipment.ItemName.AddColor(Color.green)}");
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
                DestroyMovementSquareSprites();
                _currentItemMenu = EnumCurrentMenu.none;
                _fourWayMagicMenu.CloseButtons();
                _fourWayButtonMenu.OpenButtons();
            }
        }

        public void ExecuteAttack(List<Unit> targets, Magic magicToAttack, int magicLevelToAttack) {
            var sentence = new List<string>();
            var expScore = 1;
            var damage = 1;

            switch (_previousMenuTypeState) {
                case EnumCurrentBattleMenu.item:
                    if (!ExecuteItem(targets, out expScore)) {
                        return;
                    }
                    break;
                case EnumCurrentBattleMenu.magic:
                    _currentUnit.GetCharacter().CharStats.CurrentMp -= magicToAttack.ManaCost[magicLevelToAttack - 1]; 
                    damage = _currentUnit.GetCharacter().IsPromoted
                        ? (int)Math.Round(magicToAttack.Damage[magicLevelToAttack - 1] * 1.25f,
                            MidpointRounding.AwayFromZero)
                        : magicToAttack.Damage[magicLevelToAttack - 1];
                    
                    foreach (var target in targets) {
                        var levelDifference = _currentUnit.GetCharacter().CharStats.Level - target.GetCharacter().CharStats.Level;
                        var expBase = levelDifference <= 0 ? 50 : levelDifference >= 5 ? 0 : 50 - 10 * levelDifference;
                        
                        var isCrit = _battleCalculator.RollForCrit(_currentUnit);
                        var critString = "";
                        //TODO is elemental resistance vulnerability?
                        if (isCrit) {
                            damage = _battleCalculator.GetCritDamage(_currentUnit, damage);
                            critString = "Critical hit!\n";
                        }

                        switch (magicToAttack.MagicType) {
                            case EnumMagicType.Damage:
                                sentence.Add($"{critString}{target.GetCharacter().Name.AddColor(Constants.Orange)} suffered {damage} " +
                                             $"points of damage.");
                                if (target.GetCharacter().CharStats.CurrentHp <= damage) {
                                    sentence.Add($"{target.GetCharacter().Name.AddColor(Constants.Orange)}" +
                                                 $" was defeated!");
                                    target.GetCharacter().CharStats.CurrentHp = 0;
                                    RemoveUnitFromBattle(target);
                                    expScore += (int)((expBase * (float)damage / target.GetCharacter().CharStats.MaxHp) + expBase);
                                } else {
                                    target.GetCharacter().CharStats.CurrentHp -= damage;
                                    expScore += (int)(expBase * (float)damage / target.GetCharacter().CharStats.MaxHp);
                                }
                                break;
                            case EnumMagicType.Heal:
                                var diff = target.GetCharacter().CharStats.MaxHp -
                                           target.GetCharacter().CharStats.CurrentHp;
                                var pointsToHeal = diff < damage ? diff : damage;

                                target.GetCharacter().CharStats.CurrentHp += pointsToHeal;
                                sentence.Add($"{critString}{target.GetCharacter().Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                                             $"points.");
                                var expPoints = 50 * (float)pointsToHeal / target.GetCharacter().CharStats.MaxHp;
                                expScore = 10 > (int)expPoints ? 10 : (int)expPoints;

                                break;
                            case EnumMagicType.Special:
                            case EnumMagicType.Buff:
                            case EnumMagicType.Debuff:
                                expScore += 5;
                                magicToAttack.ExecuteMagicAtLevel(_currentUnit.GetCharacter(),
                                    target.GetCharacter(), magicLevelToAttack);
                                break;
                        }
                    }
                    break;
                case EnumCurrentBattleMenu.none:
                case EnumCurrentBattleMenu.attack:
                    foreach (var target in targets) {
                        var levelDifference = _currentUnit.GetCharacter().CharStats.Level -
                                              target.GetCharacter().CharStats.Level;
                        var expBase = levelDifference <= 0 ? 50 : levelDifference >= 5 ? 0 : 50 - 10 * levelDifference;
                        if (magicToAttack == null) {
                            damage = _battleCalculator.GetBaseDamageWeaponAttack(
                                _currentUnit, target, _cursor.GetLandEffect(_cursor.MovePoint.position));
                        }

                        var isCrit = _battleCalculator.RollForCrit(_currentUnit);
                        var critString = "";
                        //TODO is elemental resistance vulnerability?
                        if (isCrit) {
                            damage = _battleCalculator.GetCritDamage(_currentUnit, damage);
                            critString = "Critical hit!\n";
                        }
                        sentence.Add($"{critString}{target.GetCharacter().Name.AddColor(Constants.Orange)} suffered {damage} " +
                                     $"points of damage.");
                        if (target.GetCharacter().CharStats.CurrentHp <= damage) {
                            sentence.Add($"{target.GetCharacter().Name.AddColor(Constants.Orange)}" +
                                         $" was defeated!");
                            target.GetCharacter().CharStats.CurrentHp = 0;
                            RemoveUnitFromBattle(target);
                            expScore += (int)((expBase * (float)damage / target.GetCharacter().CharStats.MaxHp) + expBase);
                        } else {
                            target.GetCharacter().CharStats.CurrentHp -= damage;
                            expScore += (int)(expBase * (float)damage / target.GetCharacter().CharStats.MaxHp);
                        }
                    }
                    break;
            }
            
            if (expScore >= 50) {
                expScore = 49;
            }

            if(_layerMaskForce == (_layerMaskForce | (1 << _currentUnit.gameObject.layer))) {
                sentence.AddRange(_currentUnit.GetCharacter().AddExp(expScore));
            }

            _dialogManager.EvokeSentenceDialogue(sentence);

            _enumCurrentMenuType = EnumCurrentBattleMenu.none;
            _cursor.ClearAttackArea();
            Player.PlayerIsInMenu = EnumMenuType.none;
            _cursor.EndTurn();
        }

        private bool ExecuteItem(List<Unit> targets, out int expScore) {
            expScore = 0;
            switch (_currentItemMenu) {
                case EnumCurrentMenu.use:
                    var wasUsed = false;
                    if (_selectedItem is Consumable consumable) {
                        foreach (var target in targets) {
                            wasUsed = wasUsed || consumable.TryUseItem(_currentUnit.GetCharacter(), target.GetCharacter());
                        }
                        if (wasUsed) {
                            _currentUnit.GetCharacter().RemoveItem(_selectedItem);
                            expScore = targets.Count * 5;
                            return true;
                        }
                    }
                    break;
                case EnumCurrentMenu.give:
                    //TODO
                    break;
                case EnumCurrentMenu.drop:
                    Debug.LogWarning("_currentItemMenu = drop within method 'ExecuteItem', should NOT be reached");
                    break;
                case EnumCurrentMenu.equip:
                    Debug.LogWarning("_currentItemMenu = equip within method 'ExecuteItem', should NOT be reached");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        private void HealWholeForce() {
            foreach (var unit in _force) {
                unit.GetCharacter().FullyHeal();
            }
        }

        private void RemoveUnitFromBattle(Unit target) {
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
