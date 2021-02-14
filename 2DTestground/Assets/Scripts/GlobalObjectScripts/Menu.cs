using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.Scripts.GameData;
using Assets.Scripts.GlobalObjectScripts;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject pauseUI;
    public GameObject objectMenu;
    public GameObject mainMenu;
    public GameObject characterSelector;
    public GameObject portrait;
    
    private Animator _animatorCharacterSelector;
    private Animator _animatorPortrait;

    private Animator _animatorInventory;
    private Animator _animatorMainMenuButtons;
    private Animator _animatorObjectMenuButtons;

    private Animator _animatorMemberButton;
    private Animator _animatorMagicButton;
    private Animator _animatorSearchButton;
    private Animator _animatorItemButton;

    private Animator _animatorUseButton;
    private Animator _animatorGiveButton;
    private Animator _animatorDropButton;
    private Animator _animatorEquipButton;

    private Text _textMainButtonMenu;
    private Text _textObjectButtonMenu;

    private MemberInventoryUI _memberInventoryUI;
    private Inventory _inventory;

    private Animator _currentlyAnimatedButton;

    private DialogManager _dialogManager;
    private Dialogue _dialogueSearchButton;
    private Dialogue _tempDialogue = new Dialogue {
        Name = "Itemtext",
        Sentences = new List<string>() {
            "SENTENCE NOT REPLACED!"
        },
    };
    private string _itemDialogue = "Give #ITEMNAME# to whom?";

    private ListCreator _listCreator;

    private List<PartyMember> _party;
    private int _currentListItemSelected = 0;
    private PartyMember _firstSelectedPartyMember;
    private PartyMember _secondSelectedPartyMember;

    private static Sprite _blankSprite;

    private GameObject _itemsToTrade;
    private Image _itemsToTradeOne;
    private Image _itemsToTradeTwo;

    private EnumCurrentMenu _enumCurrentMenuType = EnumCurrentMenu.none;

    private bool _isInMainButtonMenu = false;
    private bool _isInObjectButtonMenu = false;
    private bool _inInventoryMenu = false;
    private bool _isPause = false;
    private bool _currentlyShowingEquipmentList = false;
    private DirectionType _inputDirection;
    private GameItem _firstSelectedItem;
    private GameItem _secondSelectedItem;


    void Awake() {
        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");

        _dialogManager = FindObjectOfType<DialogManager>();

        _listCreator = characterSelector.GetComponent<ListCreator>();

        _itemsToTrade = objectMenu.transform.Find("ItemsToTrade").gameObject;
        _itemsToTradeOne = _itemsToTrade.transform.Find("ItemToTradeOne").GetComponent<Image>();
        _itemsToTradeTwo = _itemsToTrade.transform.Find("ItemToTradeTwo").GetComponent<Image>();
        _itemsToTrade.SetActive(false);

        _animatorPortrait = portrait.GetComponent<Animator>();
        _animatorCharacterSelector = characterSelector.GetComponent<Animator>();

        _animatorInventory = objectMenu.transform.Find("Inventory").GetComponent<Animator>();
        _animatorObjectMenuButtons = objectMenu.transform.Find("ObjectButtons").GetComponent<Animator>();
        _animatorMainMenuButtons = mainMenu.transform.Find("Buttons").GetComponent<Animator>();

        _animatorMemberButton = mainMenu.transform.Find("Buttons/Member").GetComponent<Animator>();
        _animatorMagicButton = mainMenu.transform.Find("Buttons/Magic").GetComponent<Animator>();
        _animatorSearchButton = mainMenu.transform.Find("Buttons/Search").GetComponent<Animator>();
        _animatorItemButton = mainMenu.transform.Find("Buttons/Item").GetComponent<Animator>();

        _textMainButtonMenu = mainMenu.transform.Find("Buttons/Label/LabelText").GetComponent<Text>();

        _animatorUseButton = objectMenu.transform.Find("ObjectButtons/Use").GetComponent<Animator>();
        _animatorGiveButton = objectMenu.transform.Find("ObjectButtons/Give").GetComponent<Animator>();
        _animatorDropButton = objectMenu.transform.Find("ObjectButtons/Drop").GetComponent<Animator>();
        _animatorEquipButton = objectMenu.transform.Find("ObjectButtons/Equip").GetComponent<Animator>();

        _textObjectButtonMenu = objectMenu.transform.Find("ObjectButtons/Label/LabelText").GetComponent<Text>();

        _dialogueSearchButton = new Dialogue {
            Name = "SearchText",
            Sentences = new List<string>() {
                "This Button Function will be replaced with the 'Interact' Key. It's easier that way...",
                "And also:\n#LEADERNAME# investigated the area.\nNothing was there."
            },
        };
    }

    void Start() {
        _memberInventoryUI = MemberInventoryUI.Instance;
        _inventory = Inventory.Instance;
    }

    #region OverAllInput
    // Update is called once per frame
    void Update() {
        GetInputDirection();

        if (Input.GetButtonUp("Select")) {
            if (!_isPause) {
                Pause();
            }
        }
        if (_isPause) {
            if (Input.GetButtonUp("Back")) {
                Resume();
            }
            return;
        }

        if (Player.IsInDialogue || Player.InputDisabledInDialogue || Player.InputDisabledInEvent) {
            if (Input.GetButtonUp("Interact") && !Player.InputDisabledInDialogue && !Player.InputDisabledInEvent) {
                _dialogManager.DisplayNextSentence();
            }
            return;
        }
        
        if (_isInMainButtonMenu) {
            if (_enumCurrentMenuType != EnumCurrentMenu.none) {
                if (_inInventoryMenu) {
                    HandleInventoryMenu();
                    return;
                }
                HandleCharacterSelectionMenu();
                return;
            }

            if (_isInObjectButtonMenu) {
                HandleObjectButtonMenu();
                return;
            }
            
            HandleMainMenu();
            return;
        }



        if (Input.GetButtonUp("Menu")) {
            if (!_isInMainButtonMenu) {
                OpenMainButtonMenu();
            }
        }
    }
    #endregion

    #region CharacterSelection

    private void HandleCharacterSelectionMenu() {
        var previousSelected = _currentListItemSelected;
        if (Input.GetKeyUp(KeyCode.DownArrow) && _currentListItemSelected != _party.Count-1) {
            _currentListItemSelected++;
            var selectedMember = _party[_currentListItemSelected];
            if (selectedMember != null) {
                LoadInventory(selectedMember);
                _listCreator.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
            }
        } else if (Input.GetKeyUp(KeyCode.UpArrow) && _currentListItemSelected != 0) {
            _currentListItemSelected--;
            var selectedMember = _party[_currentListItemSelected];
            if (selectedMember != null) {
                LoadInventory(selectedMember);
                _listCreator.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
            }
        } else if (Input.GetButtonUp("Interact")) {
            switch (_enumCurrentMenuType) {
                case EnumCurrentMenu.use:
                    if (_firstSelectedItem == null) {
                        _itemsToTrade.SetActive(true);
                        _inInventoryMenu = true;
                        _memberInventoryUI.SelectObject(DirectionType.up);
                    } else {
                        if (TryUseItemOnCharacter(_party[_currentListItemSelected])) {
                            RemoveCurrentItem();
                            ClearAllSelection();
                        }
                    }
                    break;
                case EnumCurrentMenu.drop:
                case EnumCurrentMenu.give:
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
                case EnumCurrentMenu.magic:
                    //TODO
                    break;
            }

            if (_firstSelectedPartyMember == null) {
                _firstSelectedPartyMember = _party[_currentListItemSelected];
            } else {
                _secondSelectedPartyMember = _party[_currentListItemSelected];
            }
        
        } else if (Input.GetButtonUp("Back")) {
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
                            _listCreator.LoadCharacterList(_party, null, _currentListItemSelected);
                        }

                        _itemsToTradeOne.sprite = _blankSprite;
                        //TODD: soundeffekt
                    } else {
                        CloseObjectMenu();
                        _enumCurrentMenuType = EnumCurrentMenu.none;
                        _itemsToTrade.SetActive(false);
                        OpenObjectButtonMenu();
                    }
                    break;

                case EnumCurrentMenu.member:
                case EnumCurrentMenu.magic:
                    if (_firstSelectedPartyMember != null) {
                        _firstSelectedPartyMember = null;
                        //TODD: soundeffekt
                    }
                    else {
                        _enumCurrentMenuType = EnumCurrentMenu.none;
                        OpenMainButtonMenu();
                    }

                    break;
            }
        }
    }

    private void RemoveCurrentItem() {
        _firstSelectedPartyMember.RemoveItem(_firstSelectedItem);
        LoadInventory(_party[_currentListItemSelected]);
    }

    private void HandleInventoryMenu() {
        _memberInventoryUI.SelectObject(_inputDirection);
        if (Input.GetButtonUp("Interact") && !Player.IsInDialogue) {
            var selectedItem = _memberInventoryUI.GetSelectedGameItem();
            if (_firstSelectedItem == null && !selectedItem.IsSet()) {
                EvokeSingleSentenceDialogue("Select an Item first ...");
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
            }
        }

        if (Input.GetButtonUp("Back")) {
            if (_secondSelectedItem != null) {
                _secondSelectedItem = null;
                _itemsToTradeTwo.sprite = _blankSprite;
            } else if (_firstSelectedItem == null) {
                _firstSelectedPartyMember = null;
            }

            _inInventoryMenu = false;
            if (_enumCurrentMenuType == EnumCurrentMenu.equip) {
                LoadInventory(_party[_currentListItemSelected]);
            }

            _memberInventoryUI.UnselectObject();
        }
        
    }

    private void HandleUseMenu(GameItem selectedItem) {
        switch (selectedItem.EnumItemType) {
            case EnumItemType.none:
                EvokeSingleSentenceDialogue("Select an Item first ...");
                break;
            case EnumItemType.equipment:
                EvokeSingleSentenceDialogue("This is an equipment.\nEquipment cannot be 'used', only equipped.");
                break;
            case EnumItemType.consumable:
                if (_firstSelectedItem == null) {
                    _itemsToTradeOne.sprite = selectedItem.ItemSprite;
                    _firstSelectedItem = selectedItem;
                    EvokeSingleSentenceDialogue($"Use {selectedItem.itemName} with whom?");
                    _inInventoryMenu = false;
                    _memberInventoryUI.UnselectObject();
                }
                break;
            case EnumItemType.forgeable:
                EvokeSingleSentenceDialogue("This item can be used for forging.\nYou need a forge to combine this item with another item.");
                break;
            case EnumItemType.promotion:
                EvokeSingleSentenceDialogue("This is a promotion Item.\nOnce a character can be promoted you might be able to use this.");
                break;
        }
    }

    private bool TryUseItemOnCharacter(PartyMember selectedPartyMember) {
        var itemToUse = (Consumable) _firstSelectedItem;
        return itemToUse.TryUseItem(selectedPartyMember);
    }

    private void HandleGiveMenu(GameItem selectedItem) {
        if (_firstSelectedItem == null) {
            _itemsToTradeOne.sprite = selectedItem.ItemSprite;
            _firstSelectedItem = selectedItem;
            EvokeSingleSentenceDialogue(_itemDialogue.Replace("#ITEMNAME#", selectedItem.itemName));
            if (selectedItem.EnumItemType == EnumItemType.equipment) {
                _currentlyShowingEquipmentList = true;
                _listCreator.LoadCharacterList(_party, (Equipment) selectedItem, _currentListItemSelected);
            }
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
        } else {
            _secondSelectedItem = selectedItem;

            var sentence = "";
            if (!selectedItem.IsSet()) {
                sentence = $"Gave {_firstSelectedPartyMember.Name}'s {_firstSelectedItem.itemName} " +
                           $"to {_secondSelectedPartyMember.Name}";
            } else {
                sentence = $"Swapped {_firstSelectedPartyMember.Name}'s {_firstSelectedItem.itemName} " +
                           $"with {_secondSelectedPartyMember.Name}'s {_secondSelectedItem.itemName}";
            }
            EvokeSingleSentenceDialogue(sentence);

            _itemsToTradeTwo.sprite = selectedItem.ItemSprite;
            _inventory.SwapItems(_firstSelectedPartyMember, _secondSelectedPartyMember,
                _firstSelectedItem, _secondSelectedItem);

            if (_currentlyShowingEquipmentList) {
                _currentlyShowingEquipmentList = false;
                _listCreator.LoadCharacterList(_party, null, _currentListItemSelected);
            }

            LoadInventory(_secondSelectedPartyMember);
            ClearAllSelection();
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
        }
    }

    private void HandelDropMenu(GameItem itemToDrop) {
        if (itemToDrop.EnumItemType == EnumItemType.none) {
            EvokeSingleSentenceDialogue("Select an item to drop ...");
            return;
        }

        _firstSelectedItem = itemToDrop;
        var dropItemCallback = new QuestionCallback {
            Name = "DropText",
            Sentences = new List<string>() {
                $"Do you want really to drop {itemToDrop.itemName}"
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
            _memberInventoryUI.SelectObject(0);
        }

        _firstSelectedItem = null;
    }

    private void HandleEquipMenu(GameItem itemToEquip) {
        if (itemToEquip is Equipment equipment) {
            if (equipment.EquipmentForClass.Any(x => x == _firstSelectedPartyMember.ClassType)) {
                var oldEquipment = (Equipment) _firstSelectedPartyMember.PartyMemberInventory.FirstOrDefault(
                    x => x.EnumItemType == EnumItemType.equipment 
                         && ((Equipment)x).EquipmentType == equipment.EquipmentType
                         && ((Equipment)x).IsEquipped);
                
                if (equipment == oldEquipment) {
                    _firstSelectedPartyMember.CharStats.UnEquip(equipment);
                    EvokeSingleSentenceDialogue($"{itemToEquip.itemName} successfully unequipped.");
                } else {
                    _firstSelectedPartyMember.CharStats.Equip(equipment);
                    _firstSelectedPartyMember.CharStats.UnEquip(oldEquipment);
                    EvokeSingleSentenceDialogue($"{itemToEquip.itemName} successfully equipped.");
                }
                _memberInventoryUI.LoadMemberEquipmentInventory(_firstSelectedPartyMember);
                _listCreator.LoadCharacterList(_party, null, _currentListItemSelected);
            }
            EvokeSingleSentenceDialogue($"{_firstSelectedPartyMember.Name} cannot equip {itemToEquip.itemName}");
        } else {
            EvokeSingleSentenceDialogue($"{itemToEquip.itemName} is not an Equipment");
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

    #endregion

    private void HandleMainMenu() {
        switch (_inputDirection) {
            case DirectionType.up:
                SetButtonActiveAndDeactivateLastButton(_animatorMemberButton, _textMainButtonMenu);
                break;
            case DirectionType.left:
                SetButtonActiveAndDeactivateLastButton(_animatorMagicButton, _textMainButtonMenu);
                break;
            case DirectionType.down:
                SetButtonActiveAndDeactivateLastButton(_animatorSearchButton, _textMainButtonMenu);
                break;
            case DirectionType.right:
                SetButtonActiveAndDeactivateLastButton(_animatorItemButton, _textMainButtonMenu);
                break;
        }

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton.name) {
                case "Member":
                    break;
                case "Magic":
                    break;
                case "Search":
                    CloseMainMenuForGood();
                    FindObjectOfType<DialogManager>().StartDialogue(_dialogueSearchButton);
                    break;
                case "Item":
                    CloseMainButtonMenu();
                    OpenObjectButtonMenu();
                    break;
            }
        }

        if (Input.GetButtonUp("Back")) {
            if (_isInMainButtonMenu) {
                CloseMainMenuForGood();
            }
        }
    }

    private void HandleObjectButtonMenu() {
        switch (_inputDirection) {
            case DirectionType.up:
                SetButtonActiveAndDeactivateLastButton(_animatorUseButton, _textObjectButtonMenu);
                break;
            case DirectionType.left:
                SetButtonActiveAndDeactivateLastButton(_animatorGiveButton, _textObjectButtonMenu);
                break;
            case DirectionType.down:
                SetButtonActiveAndDeactivateLastButton(_animatorDropButton, _textObjectButtonMenu);
                break;
            case DirectionType.right:
                SetButtonActiveAndDeactivateLastButton(_animatorEquipButton, _textObjectButtonMenu);
                break;
        }


        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton.name) {
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
            CloseObjectButtonMenu();
        }

        if (Input.GetButtonUp("Back")) {
            CloseObjectButtonMenu();
            _isInObjectButtonMenu = false;
            OpenMainButtonMenu();
        }
    }



    private void OpenMainButtonMenu() {
        _isInMainButtonMenu = true;
        Player.InputDisabled = true;
        mainMenu.SetActive(true);
        objectMenu.SetActive(true);
        portrait.SetActive(true);
        characterSelector.SetActive(true);
        _animatorMainMenuButtons.SetBool("mainMenuIsOpen", true);
        SetButtonActiveAndDeactivateLastButton(_animatorMemberButton, _textMainButtonMenu);
    }

    private void CloseMainButtonMenu() {
        _animatorMainMenuButtons.SetBool("mainMenuIsOpen", false);
    }

    private void OpenObjectButtonMenu() {
        _isInObjectButtonMenu = true;
        _animatorObjectMenuButtons.SetBool("isOpen", true);
        SetButtonActiveAndDeactivateLastButton(_animatorUseButton, _textObjectButtonMenu);
    }

    private void CloseObjectButtonMenu() {
        _animatorObjectMenuButtons.SetBool("isOpen", false);
    }

    private void OpenObjectMenu() {
        _party = _inventory.GetParty();

        OpenCharacterSelectMenu();

        _animatorPortrait.SetBool("portraitIsOpen", true);
        _animatorInventory.SetBool("inventoryIsOpen", true);
    }

    private void OpenCharacterSelectMenu() {
        characterSelector.SetActive(true);
        _listCreator.LoadCharacterList(_party, null, _currentListItemSelected);
        LoadInventory(_party[_currentListItemSelected]);
        _animatorCharacterSelector.SetBool("characterSelectorIsOpen", true);
    }

    private void CloseObjectMenu() {
        _animatorPortrait.SetBool("portraitIsOpen", false);
        _animatorInventory.SetBool("inventoryIsOpen", false);
        _animatorCharacterSelector.SetBool("characterSelectorIsOpen", false);
        _listCreator.ClearCharacterList();
    }

    private void CloseMainMenuForGood() {
        _animatorMainMenuButtons.SetBool("mainMenuIsOpen", false);
        StartCoroutine(WaitForQuaterSecCloseMainMenu());
        _isInMainButtonMenu = false;
    }

    #region Pause
    private void Resume() {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        _isPause = false;
    }

    private void Pause() {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        _isPause = true;
    }
    #endregion



    private void LoadInventory(PartyMember partyMember) {
        var gameItem = partyMember.PartyMemberInventory;
        _memberInventoryUI.LoadMemberInventory(gameItem);
        LoadPortraitOfMember(partyMember);
    }

    private void LoadPortraitOfMember(PartyMember partyMember) {
        var image = portrait.transform.Find("PortraitPicture").GetComponent<Image>();
        var sprite = partyMember.PortraitSprite;
        image.sprite = sprite;
    }

    private void SetButtonActiveAndDeactivateLastButton(Animator animator, Text label) {
        if (_currentlyAnimatedButton != null) {
            _currentlyAnimatedButton.SetBool("selected", false);
        }

        label.text = animator.name;
        animator.SetBool("selected", true);
        _currentlyAnimatedButton = animator;
    }

    private void GetInputDirection() {
        if (Input.GetAxisRaw("Vertical") > 0.05f) {
            _inputDirection = DirectionType.up;
        } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
            _inputDirection = DirectionType.left;
        } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
            _inputDirection = DirectionType.down;
        } else if (Input.GetAxisRaw("Horizontal") > 0.05f) {
            _inputDirection = DirectionType.right;
        } else {
            _inputDirection = DirectionType.none;
        }
    }

    private void EvokeSingleSentenceDialogue(string sentence) {
        _tempDialogue.Sentences.Clear();
        _tempDialogue.Sentences.Add(sentence);
        _dialogManager.StartDialogue(_tempDialogue);
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
        mainMenu.SetActive(false);
        objectMenu.SetActive(false);
        portrait.SetActive(false);
        characterSelector.SetActive(false);
        Player.InputDisabled = false;
    }

    #endregion
    
}