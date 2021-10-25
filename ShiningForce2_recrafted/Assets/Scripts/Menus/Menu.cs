using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.GameData.Characters;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    #region public
    public GameObject PauseUI;
    public GameObject ObjectMenu;

    public GameObject ObjectsForSale;
    public GameObject Gold;
    #endregion

    #region private

    private float _speedup = 2.25f;

    private RuntimeAnimatorController _animatorMemberButton;
    private RuntimeAnimatorController _animatorMagicButton;
    private RuntimeAnimatorController _animatorItemButton;

    private RuntimeAnimatorController _animatorUseButton;
    private RuntimeAnimatorController _animatorGiveButton;
    private RuntimeAnimatorController _animatorDropButton;
    private RuntimeAnimatorController _animatorEquipButton;

    private RuntimeAnimatorController _animatorJoinButton;
    private RuntimeAnimatorController _animatorBackBagButton;
    private RuntimeAnimatorController _animatorPurgeButton;
    private RuntimeAnimatorController _animatorTalkButton;

    private RuntimeAnimatorController _animatorLookButton;
    private RuntimeAnimatorController _animatorDepositButton;
    private RuntimeAnimatorController _animatorDeriveButton;

    private Animator _animatorObjectsForSale;
    private Animator _animatorGold;
    
    private MemberInventoryUI _memberInventoryUI;
    private MemberOverviewUI _memberOverviewUI;
    private CharacterDetailUI _characterDetailUI;
    private Inventory _inventory;
    private DialogManager _dialogManager;
    private CharacterSelector _characterSelector;
    private Portrait _portrait;
    private AudioManager _audioManager;
    private FourWayButtonMenu _fourWayButtonMenu;
    private BattleController _battleController;
    
    private Dialogue _dialogueSearchButton;
    private Dialogue _tempDialogue = new Dialogue {
        Name = "Itemtext",
        Sentences = new List<string>() {
            "SENTENCE NOT REPLACED!"
        },
    };
    private string _itemDialogue = "Give #ITEMNAME# to whom?";
    private string _currentlyAnimatedButton = "";
    
    private List<PartyMember> _party;
    private List<GameItem> _backBagItems;
    private GameItem _currentBackBagItem;
    private GameObject _backBagItem;
    private int _currentBackBagItemSelected = 0;
    private int _currentBackBagItemPage = 0;
    private double _pageSizeBackBag = 0;
    private int _currentListItemSelected = 0;
    private PartyMember _firstSelectedPartyMember;
    private PartyMember _secondSelectedPartyMember;

    private static Sprite _blankSprite;

    private GameObject _itemsToTrade;
    private Image _itemsToTradeOne;
    private Image _itemsToTradeTwo;

    private EnumCurrentMenu _enumCurrentMenuType = EnumCurrentMenu.none;

    private bool _showUi = false;
    private bool _disableSounds = false;
    private bool _inInventoryMenu = false;
    private bool _currentlyShowingEquipmentList = false;
    private DirectionType _inputDirection;
    private DirectionType _lastInputDirection;
    private EnumMenuType _previousMenuType;
    private GameItem _firstSelectedItem;
    private GameItem _secondSelectedItem;
    #endregion

    public static Menu Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
        _backBagItem = Resources.Load<GameObject>(Constants.PrefabBuyItem);

        _itemsToTrade = ObjectMenu.transform.Find("ItemsToTrade").gameObject;
        _itemsToTradeOne = _itemsToTrade.transform.Find("ItemToTradeOne").GetComponent<Image>();
        _itemsToTradeTwo = _itemsToTrade.transform.Find("ItemToTradeTwo").GetComponent<Image>();
        _itemsToTrade.SetActive(false);
        
        _animatorMemberButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonMember);
        _animatorMagicButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonMagic);
        _animatorTalkButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonTalk);
        _animatorItemButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonItem);
        
        _animatorUseButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonUse);
        _animatorGiveButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonGive);
        _animatorDropButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonDrop);
        _animatorEquipButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonEquip);

        _animatorJoinButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonJoin);
        _animatorBackBagButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonBackBag);
        _animatorPurgeButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonPurge);

        _animatorLookButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonLook);
        _animatorDepositButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonDeposit);
        _animatorDeriveButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonDerive);

        _animatorObjectsForSale = ObjectsForSale.transform.GetComponent<Animator>();
        _animatorGold = Gold.transform.GetComponent<Animator>();

        _dialogueSearchButton = new Dialogue {
            Name = "SearchText",
            Sentences = new List<string>() {
                "This Button Function will be replaced with the 'Interact' Key. It's easier that way...",
                "And also:\n#LEADER# investigated the area.\nNothing was there."
            },
        };

    }

    void Start() {
        _memberInventoryUI = MemberInventoryUI.Instance;
        _memberOverviewUI = MemberOverviewUI.Instance;
        _characterDetailUI = CharacterDetailUI.Instance;
        _inventory = Inventory.Instance;
        _portrait = Portrait.Instance;
        _dialogManager = DialogManager.Instance;
        _characterSelector = CharacterSelector.Instance;
        _audioManager = AudioManager.Instance;
        _fourWayButtonMenu = FourWayButtonMenu.Instance;
        _battleController = BattleController.Instance;

        ObjectMenu.SetActive(false);
    }

    #region OverAllInput
    // Update is called once per frame
    void Update() {
        if (Input.GetKeyUp(KeyCode.Backspace) && Player.PlayerIsInMenu != EnumMenuType.pause) {
            Time.timeScale = Time.timeScale > 1f ? 1f : _speedup;
        } 
        if (Input.GetButtonUp("Cancel")) {
            if (Player.PlayerIsInMenu != EnumMenuType.pause) {
                Pause();
            }
        }
        if (Player.PlayerIsInMenu == EnumMenuType.pause) {
            if (Input.GetButtonUp("Back")) {
                Resume();
            }
            return;
        }
        if (Input.GetButtonUp("Menu")) {
            if (Player.PlayerIsInMenu == EnumMenuType.none && !_battleController.IsActive 
                       && !(Player.IsInDialogue || Player.InputDisabledInDialogue 
                           || Player.InWarp || Player.InputDisabledInEvent || Player.InputDisabledAiBattle)) {
                _audioManager.PlaySFX(Constants.SfxMenuSwish);
                OpenMainButtonMenu();
            }
        }

        if (Player.IsInDialogue || Player.InputDisabledInDialogue || Player.InputDisabledInEvent
            || Player.InWarp || Player.InputDisabledAiBattle) {
            if ((Input.GetButtonUp("Interact") || Input.GetButtonUp("Back")) 
                && !Player.InputDisabledInDialogue && !Player.InputDisabledInEvent && !Player.InWarp) {
                _dialogManager.DisplayNextSentence();
            }
            return;
        }

        if (Player.PlayerIsInMenu == EnumMenuType.mainMenu) {
            GetInputDirection();

            if (_enumCurrentMenuType == EnumCurrentMenu.objectMenu) {
                HandleObjectButtonMenu();
                return;
            }
            if (_enumCurrentMenuType == EnumCurrentMenu.caravan) {
                HandleCaravanButtonMenu();
                return;
            }
            if (_enumCurrentMenuType == EnumCurrentMenu.backBag) {
                HandleBackBagButtonMenu();
                return;
            }

            if (_enumCurrentMenuType == EnumCurrentMenu.dropBackBag || _enumCurrentMenuType == EnumCurrentMenu.derive
                || _enumCurrentMenuType == EnumCurrentMenu.look) {
                HandleBackBagItems();
                return;
            }

            if (_enumCurrentMenuType != EnumCurrentMenu.none) {
                if (_inInventoryMenu) {
                    HandleInventoryMenu();
                    return;
                }
                HandleCharacterSelectionMenu();
                return;
            }

            HandleMainMenu();
            return;
        }
    }
    #endregion
    
    #region CharacterSelection

    private void HandleCharacterSelectionMenu() {
        var previousSelected = _currentListItemSelected;
        PartyMember selectedMember = null;
        if (_inputDirection != DirectionType.none) {
            if (_inputDirection == DirectionType.down && _currentListItemSelected != _party.Count - 1) {
                _currentListItemSelected++;
                selectedMember = _party[_currentListItemSelected];
            }
            else if (_inputDirection == DirectionType.up && _currentListItemSelected != 0) {
                _currentListItemSelected--;
                selectedMember = _party[_currentListItemSelected];
            }

            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.use:
                case EnumCurrentMenu.drop:
                case EnumCurrentMenu.give:
                case EnumCurrentMenu.equip:
                case EnumCurrentMenu.deriveSelectMember:
                case EnumCurrentMenu.deposit:
                    if (selectedMember != null) {
                        LoadInventory(selectedMember);
                        _characterSelector.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
                    }
                    break;
                case EnumCurrentMenu.member:
                case EnumCurrentMenu.join:
                case EnumCurrentMenu.purge:
                case EnumCurrentMenu.talk:
                    if (selectedMember != null) {
                        LoadMemberOverview(selectedMember);
                        _characterSelector.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
                    }
                    break;
                case EnumCurrentMenu.magic:
                    if (selectedMember != null) {
                        LoadMagic(selectedMember);
                        _characterSelector.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
                    }

                    break;
            }
        }


        if (Input.GetButtonUp("Interact")) {
            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.use:
                    if (_firstSelectedItem == null) {
                        _itemsToTrade.SetActive(true);
                        _inInventoryMenu = true;
                        _memberInventoryUI.SelectObject(DirectionType.up);
                    }
                    else {
                        if (TryUseItemOnCharacter(_party[_currentListItemSelected])) {
                            RemoveCurrentItem(false);
                            _characterSelector.LoadCharacterList(_party, null, _currentListItemSelected);
                            ClearAllSelection();
                        }
                    }

                    break;
                case EnumCurrentMenu.drop:
                case EnumCurrentMenu.give:
                case EnumCurrentMenu.deposit:
                    _itemsToTrade.SetActive(true);
                    _inInventoryMenu = true;
                    _memberInventoryUI.SelectObject(DirectionType.up);
                    break;
                case EnumCurrentMenu.equip:
                    _itemsToTrade.SetActive(true);
                    _inInventoryMenu = true;
                    _memberInventoryUI.LoadMemberEquipmentInventory(_party[_currentListItemSelected]);
                    break;
                case EnumCurrentMenu.member:
                    _inInventoryMenu = true;
                    CloseMemberMenu();
                    OpenCharacterDetail(_party[_currentListItemSelected]);
                    break;
                case EnumCurrentMenu.join:
                    if (_inventory.GetParty().Count(x => x.activeParty) > 12) {
                        _dialogManager.EvokeSingleSentenceDialogue("Maximum of twelve active partymembers reached.");
                    } else {
                        _party[_currentListItemSelected].activeParty = true;
                        _dialogManager.EvokeSingleSentenceDialogue($"{_party[_currentListItemSelected].Name.AddColor(Constants.Orange)} " +
                                                                   $"joined the active party.");
                        OpenCharacterSelectMenu();
                    }
                    break;
                case EnumCurrentMenu.purge:
                    if (_party[_currentListItemSelected].partyLeader) {
                        _dialogManager.EvokeSingleSentenceDialogue("Your team needs the leader in the active party!");
                    } else {
                        _party[_currentListItemSelected].activeParty = false;
                        _dialogManager.EvokeSingleSentenceDialogue($"{_party[_currentListItemSelected].Name.AddColor(Constants.Orange)} " +
                                                                   $"left the active party.");
                        OpenCharacterSelectMenu();
                    }
                    break;
                case EnumCurrentMenu.talk:
                    var selection = _party[_currentListItemSelected];
                    var dialogue = new Dialogue();
                    if (!MemberDialogueStorage.Instance.TryGetDialogueForMember(selection.CharacterType,
                        out var sentences) || selection.StatusEffects.HasFlag(EnumStatusEffect.dead)) {
                        sentences = new List<string> {$"{selection.Name.AddColor(Constants.Orange)} seems to be ... elsewhere."};
                        dialogue = new Dialogue() {
                            Name = selection.Name,
                            Portrait = null,
                            Sentences = sentences,
                            VoicePitch = EnumVoicePitch.none,
                        };
                    } else {
                        dialogue = new Dialogue() {
                            Name = selection.Name,
                            Portrait = null,
                            Sentences = sentences,
                            VoicePitch = selection.Voice,
                        };
                    }
                    _dialogManager.StartDialogue(dialogue);
                    break;
                case EnumCurrentMenu.deriveSelectMember:
                    var selectedPartyMember = _party[_currentListItemSelected];
                    if (_inventory.TryAddGameItemToPartyMember(selectedPartyMember, _currentBackBagItem)) {
                        _inventory.RemoveFromBackBag(_currentBackBagItem);
                        _dialogManager.EvokeSingleSentenceDialogue($"{_currentBackBagItem.ItemName.AddColor(Color.green)} added " +
                                                                   $"to {selectedPartyMember.Name.AddColor(Constants.Orange)}");

                        _currentBackBagItem = null;
                        _enumCurrentMenuType = EnumCurrentMenu.derive;
                        CloseMemberMenu();
                        CloseObjectMenu();
                        OpenBackBagMenu();
                        return;
                    } else {
                        _dialogManager.EvokeSingleSentenceDialogue($"{selectedPartyMember.Name.AddColor(Constants.Orange)} has" +
                                                                   $"no space in the inventory.");
                    }
                    break;
                case EnumCurrentMenu.magic:
                    _itemsToTrade.SetActive(true);
                    _inInventoryMenu = true;
                    _memberInventoryUI.SelectMagic(DirectionType.up);
                    break;
            }

            if (_firstSelectedPartyMember == null) {
                _firstSelectedPartyMember = _party[_currentListItemSelected];
            }
            else {
                _secondSelectedPartyMember = _party[_currentListItemSelected];
            }

        }
        else if (Input.GetButtonUp("Back")) {
            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.use:
                case EnumCurrentMenu.drop:
                case EnumCurrentMenu.give:
                case EnumCurrentMenu.equip:
                    if (_firstSelectedItem != null) {
                        _firstSelectedItem = null;
                        _firstSelectedPartyMember = null;

                        if (_currentlyShowingEquipmentList) {
                            _currentlyShowingEquipmentList = false;
                            _characterSelector.LoadCharacterList(_party, null, _currentListItemSelected);
                        }

                        _itemsToTradeOne.sprite = _blankSprite;
                    }
                    else {
                        CloseObjectMenu();
                        _enumCurrentMenuType = EnumCurrentMenu.none;
                        _itemsToTrade.SetActive(false);
                        OpenObjectButtonMenu();
                    }
                    break;
                case EnumCurrentMenu.deposit:
                case EnumCurrentMenu.deriveSelectMember:
                    CloseObjectMenu();
                    _enumCurrentMenuType = EnumCurrentMenu.backBag;
                    _itemsToTrade.SetActive(false);
                    OpenBackBagButtonMenu();
                    break;
                case EnumCurrentMenu.join:
                case EnumCurrentMenu.purge:
                case EnumCurrentMenu.talk:
                    _enumCurrentMenuType = EnumCurrentMenu.caravan;
                    _itemsToTrade.SetActive(false);
                    CloseMemberMenu();
                    OpenCaravanButtonMenu();
                    break;
                case EnumCurrentMenu.member:
                    if (_firstSelectedPartyMember != null) {
                        _firstSelectedPartyMember = null;
                    }
                    else {
                        _enumCurrentMenuType = EnumCurrentMenu.none;
                        _itemsToTrade.SetActive(false);
                        CloseMemberMenu();
                        OpenMainButtonMenu();
                    }

                    break;
                case EnumCurrentMenu.magic:
                    if (_firstSelectedPartyMember != null) {
                        _firstSelectedPartyMember = null;
                    }
                    else {
                        _enumCurrentMenuType = EnumCurrentMenu.none;
                        _itemsToTrade.SetActive(false);

                        CloseMagicMenu();
                        OpenMainButtonMenu();
                    }
                    break;
            }
        }
    }

    #endregion

    private void RemoveCurrentItem(bool addToDealsIfUnique = true) {
        _firstSelectedPartyMember.RemoveItem(_firstSelectedItem, addToDealsIfUnique);
        LoadInventory(_party[_currentListItemSelected]);
        _characterSelector.LoadCharacterList(_party,null,_currentListItemSelected);
    }

    private void HandleInventoryMenu() {
        if (Input.GetButtonUp("Back")) {
            if (_secondSelectedItem != null) {
                _secondSelectedItem = null;
                _itemsToTradeTwo.sprite = _blankSprite;
            } else if (_firstSelectedItem == null) {
                _firstSelectedPartyMember = null;
            }

            _inInventoryMenu = false;
            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.equip:
                    LoadInventory(_party[_currentListItemSelected]);
                    break;
                case EnumCurrentMenu.member:
                    CloseCharacterDetail();
                    OpenMemberMenu();
                    return;
            }
            _memberInventoryUI.UnselectObject();
        }

        if (_enumCurrentMenuType == EnumCurrentMenu.member) {
            return;
        } else if (_enumCurrentMenuType == EnumCurrentMenu.magic) {
            _memberInventoryUI.SelectMagic(_inputDirection);
            if (Input.GetButtonUp("Interact") && !Player.IsInDialogue) {
                var selectedMagic = _memberInventoryUI.GetSelectedMagic();
                if (selectedMagic.IsEmpty()) {
                    _dialogManager.EvokeSingleSentenceDialogue("Select a magic spell...");
                    return;
                }
                _dialogManager.EvokeSingleSentenceDialogue(
                    $"{selectedMagic.SpellName.AddColor(Constants.Violet)} doesn't seem to do anything.");
                return;
            }
        } else {
            _memberInventoryUI.SelectObject(_inputDirection);
            if (Input.GetButtonUp("Interact") && !Player.IsInDialogue) {
                var selectedItem = _memberInventoryUI.GetSelectedGameItem();
                if (_firstSelectedItem == null && selectedItem.IsEmpty()) {
                    _dialogManager.EvokeSingleSentenceDialogue("Select an Item first ...");
                    return;
                }
                switch (_enumCurrentMenuType) {
                    case EnumCurrentMenu.use:
                        HandleUseMenu(selectedItem);
                        break;
                    case EnumCurrentMenu.give:
                        HandleGiveMenu(selectedItem);
                        break;
                    case EnumCurrentMenu.drop:
                        HandelDropMenu(selectedItem);
                        break;
                    case EnumCurrentMenu.equip:
                        HandleEquipMenu(selectedItem);
                        break;
                    case EnumCurrentMenu.deposit:
                        _inventory.AddToBackBag(selectedItem);
                        _firstSelectedPartyMember?.RemoveItem(selectedItem, false);
                        _dialogManager.EvokeSingleSentenceDialogue($"{selectedItem.ItemName.AddColor(Color.green)} added to " +
                                                                   $"back bag.");
                        LoadInventory(_firstSelectedPartyMember);
                        _memberInventoryUI.SelectObject(DirectionType.up);
                        OpenCharacterSelectMenu();
                        break;
                }
            }
        }
    }

    private void HandleUseMenu(GameItem selectedItem) {
        switch (selectedItem.EnumItemType) {
            case EnumItemType.none:
                _dialogManager.EvokeSingleSentenceDialogue("Select an Item first ...");
                break;
            case EnumItemType.equipment:
                _dialogManager.EvokeSingleSentenceDialogue(
                    "This is an equipment.\n" +
                    "Equipment cannot be 'used', only equipped.");
                break;
            case EnumItemType.consumable:
                if (_firstSelectedItem == null) {
                    _itemsToTradeOne.sprite = selectedItem.ItemSprite;
                    _firstSelectedItem = selectedItem;
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"Use {selectedItem.ItemName.AddColor(Color.green)} with whom?");
                    _inInventoryMenu = false;
                    _memberInventoryUI.UnselectObject();
                }
                break;
            case EnumItemType.forgeable:
                _dialogManager.EvokeSingleSentenceDialogue(
                    "This item can be used for forging\n" +
                    "You need a forge to combine this item with another item.");
                break;
            case EnumItemType.promotion:
                _dialogManager.EvokeSingleSentenceDialogue(
                    "This is a promotion Item.\n" +
                    "Once a character can be promoted you might be able to use this.");
                break;
        }
    }

    private bool TryUseItemOnCharacter(PartyMember selectedPartyMember) {
        var itemToUse = (Consumable) _firstSelectedItem;
        var memberAsUnit = new Unit();
        memberAsUnit.SetHackCharacter(selectedPartyMember);
        return itemToUse.TryUseItem(memberAsUnit, new List<Unit>() { memberAsUnit }, false);
    }

    private void HandleGiveMenu(GameItem selectedItem) {
        if (_firstSelectedItem == null) {
            _itemsToTradeOne.sprite = selectedItem.ItemSprite;
            _firstSelectedItem = selectedItem;
            _dialogManager.EvokeSingleSentenceDialogue(
                _itemDialogue.Replace(
                    "#ITEMNAME#", selectedItem.ItemName.AddColor(Color.green)));
            if (selectedItem.EnumItemType == EnumItemType.equipment) {
                _currentlyShowingEquipmentList = true;
                _characterSelector.LoadCharacterList(_party, (Equipment) selectedItem, _currentListItemSelected);
            }
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
        } else {
            _secondSelectedItem = selectedItem;

            var sentence = "";
            if (selectedItem.IsEmpty()) {
                sentence = $"Gave {_firstSelectedPartyMember.Name.AddColor(Constants.Orange)}'s {_firstSelectedItem.ItemName.AddColor(Color.green)} " +
                           $"to {_secondSelectedPartyMember.Name.AddColor(Constants.Orange)}";
            } else {
                sentence = $"Swapped {_firstSelectedPartyMember.Name.AddColor(Constants.Orange)}'s {_firstSelectedItem.ItemName.AddColor(Color.green)} " +
                           $"with {_secondSelectedPartyMember.Name.AddColor(Constants.Orange)}'s {_secondSelectedItem.ItemName.AddColor(Color.green)}";
            }
            _dialogManager.EvokeSingleSentenceDialogue(sentence);

            _itemsToTradeTwo.sprite = selectedItem.ItemSprite;
            _inventory.SwapItems(_firstSelectedPartyMember, _secondSelectedPartyMember,
                _firstSelectedItem, _secondSelectedItem);

            if (_currentlyShowingEquipmentList) {
                _currentlyShowingEquipmentList = false;
                _characterSelector.LoadCharacterList(_party, null, _currentListItemSelected);
            }

            LoadInventory(_secondSelectedPartyMember);
            ClearAllSelection();
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
        }
    }

    private void HandelDropMenu(GameItem itemToDrop) {
        if (itemToDrop.EnumItemType == EnumItemType.none) {
            _dialogManager.EvokeSingleSentenceDialogue("Select an item to drop ...");
            return;
        }

        _firstSelectedItem = itemToDrop;
        var dropItemCallback = new QuestionCallback {
            Name = "DropText",
            Sentences = new List<string>() {
                $"Do you really want to drop {itemToDrop.ItemName.AddColor(Color.green)}"
            },
            DefaultSelectionForQuestion = YesNo.No,
            OnAnswerAction = DropItemAnswer,
        };
        _dialogManager.StartDialogue(dropItemCallback);
    }

    private void DropItemAnswer(bool answer) {
        if (answer) {
            RemoveCurrentItem();
            LoadInventory(_firstSelectedPartyMember);
            _memberInventoryUI.SelectObject(DirectionType.up);
        }

        _firstSelectedItem = null;
    }
    private void HandelBackBagDropMenu() {
        if (_currentBackBagItem.EnumItemType == EnumItemType.none) {
            _dialogManager.EvokeSingleSentenceDialogue("Select an item to drop ...");
            return;
        }

        var dropItemCallback = new QuestionCallback {
            Name = "DropText",
            Sentences = new List<string>() {
                $"Do you really want to drop {_currentBackBagItem.ItemName.AddColor(Color.green)}"
            },
            DefaultSelectionForQuestion = YesNo.No,
            OnAnswerAction = DropBackBagItemAnswer,
        };
        _dialogManager.StartDialogue(dropItemCallback);
    }

    private void DropBackBagItemAnswer(bool answer) {
        if (answer) {
            _inventory.RemoveFromBackBag(_currentBackBagItem);
            OpenBackBagMenu();
        }
        _currentBackBagItem = null;
    }

    private void HandleEquipMenu(GameItem itemToEquip) {
        if (itemToEquip is Equipment equipment) {
            if (equipment.EquipmentForClass.Any(x => x == _firstSelectedPartyMember.ClassType)) {
                var oldEquipment = (Equipment) _firstSelectedPartyMember.GetInventory().FirstOrDefault(
                    x => x.EnumItemType == EnumItemType.equipment 
                         && ((Equipment)x).EquipmentType == equipment.EquipmentType
                         && ((Equipment)x).IsEquipped);
                
                if (equipment == oldEquipment) {
                    _firstSelectedPartyMember.CharStats.UnEquip(equipment);
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{itemToEquip.ItemName.AddColor(Color.green)} successfully unequipped.");
                } else {
                    _firstSelectedPartyMember.CharStats.Equip(equipment);
                    _firstSelectedPartyMember.CharStats.UnEquip(oldEquipment);
                    _dialogManager.EvokeSingleSentenceDialogue(
                        $"{itemToEquip.ItemName.AddColor(Color.green)} successfully equipped.");
                }
                _memberInventoryUI.LoadMemberEquipmentInventory(_firstSelectedPartyMember);
                _characterSelector.LoadCharacterList(_party, null, _currentListItemSelected);
            } else {
                _dialogManager.EvokeSingleSentenceDialogue(
                    $"{_firstSelectedPartyMember.Name} cannot equip " +
                    $"{itemToEquip.ItemName.AddColor(Color.green)}");
            }
        } else {
            _dialogManager.EvokeSingleSentenceDialogue(
                $"{itemToEquip.ItemName.AddColor(Color.green)} is not an Equipment");
        }
    }

    private void ClearAllSelection() {
        _firstSelectedPartyMember = null;
        _secondSelectedPartyMember = null;
        Debug.Log(_firstSelectedItem);
        _firstSelectedItem = null;
        _secondSelectedItem = null;
        _itemsToTradeOne.sprite = _blankSprite;
        _itemsToTradeTwo.sprite = _blankSprite;
    }

    private void HandleMainMenu() {
        _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton) {
                case "Member":
                    _enumCurrentMenuType = EnumCurrentMenu.member;
                    _fourWayButtonMenu.CloseButtons();
                    OpenMemberMenu();
                    break;
                case "Magic":
                    _enumCurrentMenuType = EnumCurrentMenu.magic;
                    _fourWayButtonMenu.CloseButtons();
                    OpenMagicMenu();
                    break;
                case "Caravan":
                    _fourWayButtonMenu.CloseButtons();
                    OpenCaravanButtonMenu();
                    break;
                case "Item":
                    _fourWayButtonMenu.CloseButtons();
                    OpenObjectButtonMenu();
                    break;
            }
        }

        if (Input.GetButtonUp("Back")) {
            if (_enumCurrentMenuType == EnumCurrentMenu.none) {
                CloseMainMenuForGood();
            }
        }
    }

    private void HandleCaravanButtonMenu() {
        _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton) {
                case "Join":
                    _enumCurrentMenuType = EnumCurrentMenu.join;
                    break;
                case "BackBag":
                    _enumCurrentMenuType = EnumCurrentMenu.backBag;
                    OpenBackBagButtonMenu();
                    return;
                case "Purge":
                    _enumCurrentMenuType = EnumCurrentMenu.purge;
                    break;
                case "Talk":
                    _enumCurrentMenuType = EnumCurrentMenu.talk;
                    break;
            }
            OpenMemberMenu();
            _fourWayButtonMenu.CloseButtons();
        }

        if (Input.GetButtonUp("Back")) {
            _fourWayButtonMenu.CloseButtons();
            OpenMainButtonMenu();
        }
    }

    private void HandleBackBagButtonMenu() {
        _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton) {
                case "Look":
                    _enumCurrentMenuType = EnumCurrentMenu.look;
                    _fourWayButtonMenu.CloseButtons();
                    OpenBackBagMenu();
                    break;
                case "Deposit":
                    _enumCurrentMenuType = EnumCurrentMenu.deposit;
                    _fourWayButtonMenu.CloseButtons();
                    OpenObjectMenu();
                    break;
                case "Derive":
                    _enumCurrentMenuType = EnumCurrentMenu.derive;
                    _fourWayButtonMenu.CloseButtons();
                    OpenBackBagMenu();
                    break;
                case "Drop":
                    _enumCurrentMenuType = EnumCurrentMenu.dropBackBag;
                    _fourWayButtonMenu.CloseButtons();
                    OpenBackBagMenu();
                    break;
            }
        }

        if (Input.GetButtonUp("Back")) {
            _fourWayButtonMenu.CloseButtons();
            OpenCaravanButtonMenu();
        }
    }

    private void HandleObjectButtonMenu() {
        _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton) {
                case "Use":
                    _enumCurrentMenuType = EnumCurrentMenu.use;
                    break;
                case "Give":
                    _enumCurrentMenuType = EnumCurrentMenu.give;
                    break;
                case "Drop":
                    _enumCurrentMenuType = EnumCurrentMenu.drop;
                    break;
                case "Equip":
                    _enumCurrentMenuType = EnumCurrentMenu.equip;
                    break;
            }
            OpenObjectMenu();
            _fourWayButtonMenu.CloseButtons();
        }

        if (Input.GetButtonUp("Back")) {
            _fourWayButtonMenu.CloseButtons();
            OpenMainButtonMenu();
        }
    }

    private void OpenMainButtonMenu() {
        Player.PlayerIsInMenu = EnumMenuType.mainMenu;
        _enumCurrentMenuType = EnumCurrentMenu.none;
        _showUi = true;
        ObjectMenu.SetActive(true);
        _fourWayButtonMenu.InitializeButtons(_animatorMemberButton, _animatorMagicButton,
            _animatorBackBagButton, _animatorItemButton,
            "Member", "Magic", "Caravan", "Item");
    }

    private void OpenObjectButtonMenu() {
        _enumCurrentMenuType = EnumCurrentMenu.objectMenu;
        _fourWayButtonMenu.InitializeButtons(_animatorUseButton, _animatorGiveButton,
            _animatorDropButton, _animatorEquipButton,
            "Use", "Give", "Drop", "Equip");
    }

    private void OpenCaravanButtonMenu() {
        _enumCurrentMenuType = EnumCurrentMenu.caravan;
        _fourWayButtonMenu.InitializeButtons(_animatorJoinButton, _animatorBackBagButton,
            _animatorPurgeButton, _animatorTalkButton,
            "Join", "BackBag", "Purge", "Talk");
    }

    private void OpenBackBagButtonMenu() {
        _enumCurrentMenuType = EnumCurrentMenu.backBag;
        _fourWayButtonMenu.InitializeButtons(_animatorLookButton, _animatorDepositButton,
            _animatorDropButton, _animatorDeriveButton,
            "Look", "Deposit", "Drop", "Derive");
    }


    private void OpenObjectMenu() {
        _party = _inventory.GetParty();

        OpenCharacterSelectMenu();
        LoadInventory(_party[_currentListItemSelected]);
    }

    private void CloseObjectMenu() {
        _portrait.HidePortrait();
        _memberInventoryUI.CloseInventory();
        _characterSelector.ClearCharacterList();
    }

    private void OpenBackBagMenu() {
        _party = _inventory.GetParty();
        _backBagItems = _inventory.GetBackPack();
        if (_backBagItems.Count <= 0) {
            _enumCurrentMenuType = EnumCurrentMenu.backBag;
            CloseBackBagMenu();
            _fourWayButtonMenu.OpenButtons();
            _dialogManager.EvokeSingleSentenceDialogue("There are no items in your back bag.", true);
            return;
        }
        ObjectsForSale.SetActive(true);
        Gold.SetActive(true);
        _animatorObjectsForSale.SetBool("isOpen", true);
        _animatorGold.SetBool("isOpen", true);
        Gold.transform.Find("GoldText").GetComponent<Text>().text = _inventory.GetGold().ToString();

        LoadItemsInBackBag();
    }

    private void CloseBackBagMenu() {
        _animatorObjectsForSale.SetBool("isOpen", false);
        _animatorGold.SetBool("isOpen", false);
        ObjectsForSale.SetActive(false);
        Gold.SetActive(false);
    }

    private void OpenMagicMenu() {
        _party = _inventory.GetParty();

        OpenCharacterSelectMenu();
        LoadMagic(_party[_currentListItemSelected]);

    }

    private void CloseMagicMenu() {
        _portrait.HidePortrait();
        _memberInventoryUI.CloseInventory();
        _characterSelector.ClearCharacterList();
    }

    private void OpenMemberMenu() {
        _party = _inventory.GetParty();

        OpenCharacterSelectMenu();
        LoadMemberOverview(_party[_currentListItemSelected]);
    }

    private void CloseMemberMenu() {
        _portrait.HidePortrait();
        _memberOverviewUI.CloseMemberOverviewUi();
        _characterSelector.ClearCharacterList();
    }

    private void OpenCharacterDetail(Character character) {
        _disableSounds = true;
        _characterDetailUI.LoadCharacterDetails(character);
    }

    private void CloseCharacterDetail() {
        _disableSounds = false;
        _characterDetailUI.CloseCharacterDetailsUi();
    }

    private void OpenCharacterSelectMenu() {
        _characterSelector.LoadCharacterList(_party, null, _currentListItemSelected);
    }

    private void CloseMainMenuForGood() {
        _fourWayButtonMenu.CloseButtons();
        _showUi = false;
        StartCoroutine(WaitForQuaterSecCloseMainMenu());
        Player.PlayerIsInMenu = EnumMenuType.none;
    }

    #region Pause
    private void Resume() {
        PauseUI.SetActive(false);
        Time.timeScale = 1f;
        Player.PlayerIsInMenu = _previousMenuType;
    }

    private void Pause() {
        PauseUI.SetActive(true);
        Time.timeScale = 0f;
        _previousMenuType = Player.PlayerIsInMenu;
        Player.PlayerIsInMenu = EnumMenuType.pause;
    }
    #endregion

    private void LoadInventory(PartyMember partyMember) {
        _memberInventoryUI.LoadMemberInventory(partyMember);
        _portrait.ShowPortrait(partyMember.PortraitSprite);
    }

    private void LoadMagic(PartyMember partyMember) {
        _memberInventoryUI.LoadMemberMagic(partyMember);
        _portrait.ShowPortrait(partyMember.PortraitSprite);
    }

    private void LoadMemberOverview(PartyMember partyMember) {
        _memberOverviewUI.LoadMemberInventory(partyMember);
        _portrait.ShowPortrait(partyMember.PortraitSprite);
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
            _inputDirection  = DirectionType.none;
        }

        if (currentDirection == _lastInputDirection) {
            _inputDirection = DirectionType.none;
        }
        else {
            _lastInputDirection = _inputDirection = currentDirection;
            if (_inputDirection != DirectionType.none && !_disableSounds) {
                _audioManager.PlaySFX(Constants.SfxMenuDing);
            }
        }

        if (Input.GetButtonUp("Back") || Input.GetButtonUp("Interact") && !_disableSounds) {
            _audioManager.PlaySFX(Constants.SfxMenuSwish);
        }

    }

    private void LoadItemsInBackBag() {
        var upDown = ObjectsForSale.transform.Find("UpDown").gameObject;
        var itemCount = _backBagItems.Count();
        _pageSizeBackBag = Math.Ceiling((float)itemCount / 8);
        if (_currentBackBagItemPage > _pageSizeBackBag - 1) {
            _currentBackBagItemPage = (int)_pageSizeBackBag - 1;
        }

        if (_currentBackBagItemPage == (int)_pageSizeBackBag - 1) {
            var lastItemOnPage = ((itemCount-1) % 8);
            if (_currentBackBagItemSelected > lastItemOnPage) {
                _currentBackBagItemSelected = lastItemOnPage;
            }
        }

        if (_currentBackBagItemPage == 0 && _pageSizeBackBag > 1) {
            upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Visible;
        } else {
            upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Invisible;
        }

        if (_pageSizeBackBag > 1 && _currentBackBagItemPage != 0) {
            upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Visible;
        } else {
            upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Invisible;
        }

        var spawnPoint = ObjectsForSale.transform.Find("ItemSpawnPoint");
        foreach (Transform child in spawnPoint.transform) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 8; i++) {
            var nextItemIndex = i + _currentBackBagItemPage * 8;
            if (nextItemIndex >= itemCount) {
                return;
            }
            var pos = new Vector3(0, 0, spawnPoint.position.z);
            var spawnedItem = Instantiate(_backBagItem, pos, spawnPoint.rotation);
            spawnedItem.transform.SetParent(spawnPoint, false);

            var itemDetails = _backBagItems.ToArray()[nextItemIndex];
            var spawnedItemImage = spawnedItem.transform.GetComponent<Image>();
            var spawnedItemText = spawnedItem.transform.GetChild(0).GetComponent<Text>();

            spawnedItemImage.sprite = itemDetails.ItemSprite;
            spawnedItemText.text = itemDetails.Price.ToString();

            var selectionColor = i == _currentBackBagItemSelected ? Constants.Orange : Constants.Visible;

            spawnedItemImage.color = selectionColor;
            spawnedItemText.color = selectionColor;
        }
    }

    private void HandleBackBagItems() {
        if (_inputDirection == DirectionType.left && _currentBackBagItemSelected > 0) {
            _currentBackBagItemSelected--;
            LoadItemsInBackBag();
        } else if (_inputDirection == DirectionType.right && _currentBackBagItemSelected < 7) {
            _currentBackBagItemSelected++;
            LoadItemsInBackBag();
        } else if (_inputDirection == DirectionType.up && _currentBackBagItemPage > 0) {
            _currentBackBagItemPage--;
            LoadItemsInBackBag();
        } else if (_inputDirection == DirectionType.down && _currentBackBagItemPage < _pageSizeBackBag - 1) {
            _currentBackBagItemPage++;
            LoadItemsInBackBag();
        }

        if (Input.GetButtonUp("Interact")) {
            _currentBackBagItem = _backBagItems.ToArray()[_currentBackBagItemSelected + _currentBackBagItemPage * 8];
            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.look:
                    _dialogManager.EvokeSentenceDialogue(_currentBackBagItem.GetDescription());
                    break;
                case EnumCurrentMenu.derive:
                    _dialogManager.EvokeSingleSentenceDialogue($"Give {_currentBackBagItem.ItemName.AddColor(Color.green)} to whom?");
                    _enumCurrentMenuType = EnumCurrentMenu.deriveSelectMember;
                    CloseBackBagMenu();
                    OpenObjectMenu();
                    break;
                case EnumCurrentMenu.dropBackBag:
                    HandelBackBagDropMenu();
                    //TODO
                    break;
                default:
                    Debug.LogError($"{Enum.GetName(typeof(EnumCurrentMenu), _enumCurrentMenuType)} is not valid during HandleBackBagItems.");
                    break;
            }
            return;
        }

        if (Input.GetButtonUp("Back")) {
            _enumCurrentMenuType = EnumCurrentMenu.backBag;
            CloseBackBagMenu();
            _fourWayButtonMenu.OpenButtons();
        }
    }

    #region Coroutines

    IEnumerator WaitCloseSetActiveFalseAndStartNextCoroutine(List<GameObject> gameObjects, IEnumerator coroutine) {
        yield return new WaitForSeconds(0.1f);
        foreach (var thisGameObject in gameObjects) {
            thisGameObject.SetActive(false);
        }

        StartCoroutine(coroutine);
    }

    IEnumerator WaitForQuaterSecCloseMainMenu() {
        yield return new WaitForSeconds(0.1f);
        if (!_showUi) {
            ObjectMenu.SetActive(false);
        }
    }

    #endregion
    
}