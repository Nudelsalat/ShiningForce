using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    private Inventory  _inventory;

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
    private int _currentItemSelected = 0;
    private PartyMember _firstSelectedPartyMember;
    private PartyMember _secondSelectedPartyMember;

    private static Sprite _blankSprite;

    private GameObject _itemsToTrade;
    private Image _itemsToTradeOne;
    private Image _itemsToTradeTwo;

    private CurrentMenu _currentMenuType = CurrentMenu.none;

    private bool _isInMainButtonMenu = false;
    private bool _isInObjectButtonMenu = false;
    private bool _inInventoryMenu = false;
    private bool _isPause = false;
    private DirectionType _inputDirection;
    private GameItem _firstSelectedItem;
    private GameItem _secondSelectedItem;


    void Awake() {
        _memberInventoryUI = MemberInventoryUI.Instance;
        _inventory = Inventory.Instance;

        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");

        _dialogManager = FindObjectOfType<DialogManager>();

        _listCreator = characterSelector.GetComponent<ListCreator>();

        _itemsToTrade = objectMenu.transform.Find("ItemsToTrade").gameObject;
        _itemsToTradeOne = _itemsToTrade.transform.GetChild(0).GetComponent<Image>();
        _itemsToTradeTwo = _itemsToTrade.transform.GetChild(1).GetComponent<Image>();
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
            if (_currentMenuType != CurrentMenu.none) {
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
        var previousSelected = _currentItemSelected;
        if (Input.GetKeyUp(KeyCode.DownArrow) && _currentItemSelected != _party.Count-1) {
            _currentItemSelected++;
            var selectedMember = _party[_currentItemSelected];
            if (selectedMember != null) {
                LoadInventory(selectedMember);
                _listCreator.SetScrollbar(previousSelected, _currentItemSelected, _party.Count);
            }
        } else if (Input.GetKeyUp(KeyCode.UpArrow) && _currentItemSelected != 0) {
            _currentItemSelected--;
            var selectedMember = _party[_currentItemSelected];
            if (selectedMember != null) {
                LoadInventory(selectedMember);
                _listCreator.SetScrollbar(previousSelected, _currentItemSelected, _party.Count);
            }
        } else if (Input.GetButtonUp("Interact")) {
            switch (_currentMenuType) {
                case CurrentMenu.use:
                    if (_firstSelectedItem == null) {
                        _itemsToTrade.SetActive(true);
                        _inInventoryMenu = true;
                        _memberInventoryUI.SelectObject(DirectionType.up);
                    } else {
                        if (TryUseItemOnCharacter(_party[_currentItemSelected])) {
                            //TODO remove item
                            _firstSelectedPartyMember.RemoveItem(_firstSelectedItem);
                            LoadInventory(_party[_currentItemSelected]);
                            ClearAllSelection();
                        }
                    }
                    break;
                case CurrentMenu.drop:
                case CurrentMenu.give:
                case CurrentMenu.equip:
                    _itemsToTrade.SetActive(true);
                    _inInventoryMenu = true;
                    _memberInventoryUI.SelectObject(DirectionType.up);
                    break;
                case CurrentMenu.member:
                case CurrentMenu.magic:
                    //TODO
                    break;
            }

            if (_firstSelectedPartyMember == null) {
                _firstSelectedPartyMember = _party[_currentItemSelected];
            } else {
                _secondSelectedPartyMember = _party[_currentItemSelected];
            }
        
        } else if (Input.GetButtonUp("Back")) {
            switch (_currentMenuType) {
                case CurrentMenu.use:
                case CurrentMenu.drop:
                case CurrentMenu.give:
                case CurrentMenu.equip:
                    if (_firstSelectedItem != null) {
                        _firstSelectedItem = null;
                        _firstSelectedPartyMember = null;

                        _itemsToTradeOne.sprite = _blankSprite;
                        //TODD: soundeffekt
                    }
                    else {
                        CloseObjectMenu();
                        _currentMenuType = CurrentMenu.none;
                        _itemsToTrade.SetActive(false);
                        OpenObjectButtonMenu();
                    }

                    break;
                case CurrentMenu.member:
                case CurrentMenu.magic:
                    if (_firstSelectedPartyMember != null) {
                        _firstSelectedPartyMember = null;
                        //TODD: soundeffekt
                    }
                    else {
                        _currentMenuType = CurrentMenu.none;
                        OpenMainButtonMenu();
                    }

                    break;
            }
        }
    }

    private void HandleInventoryMenu() {
        _memberInventoryUI.SelectObject(_inputDirection);
        if (Input.GetButtonUp("Interact") && !Player.IsInDialogue) {
            var selectedItem = _memberInventoryUI.GetSelectedGameItem();
            if (_firstSelectedItem == null && !selectedItem.IsSet()) {
                EvokeSingleSentenceDialogue("Select an Item first ...");
                return;
            }
            switch (_currentMenuType) {
                case CurrentMenu.use:
                    HandleUseMenu(selectedItem);
                    break;
                case CurrentMenu.give:
                    HandleGiveMenu(selectedItem);
                    break;
                case CurrentMenu.drop:
                    //HandelDropMenu(selectedItem);
                    break;
                case CurrentMenu.equip:
                    //HandelEquipMenu(selectedItem);
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
            _memberInventoryUI.UnselectObject();
        }
        
    }

    private void HandleUseMenu(GameItem selectedItem) {
        switch (selectedItem.itemType) {
            case ItemType.none:
                EvokeSingleSentenceDialogue("Select an Item first ...");
                break;
            case ItemType.equipment:
                EvokeSingleSentenceDialogue("This is an equipment.\nEquipment cannot be 'used', only equipped.");
                break;
            case ItemType.consumable:
                if (_firstSelectedItem == null) {
                    _itemsToTradeOne.sprite = selectedItem.ItemSprite;
                    _firstSelectedItem = selectedItem;
                    EvokeSingleSentenceDialogue($"Use {selectedItem.itemName} with whom?");
                    _inInventoryMenu = false;
                    _memberInventoryUI.UnselectObject();
                }
                break;
            case ItemType.forgeable:
                EvokeSingleSentenceDialogue("This item can be used for forging.\nYou need a forge to combine this item with another item.");
                break;
            case ItemType.promotion:
                EvokeSingleSentenceDialogue("This is a promotion Item.\nOnce a character can be promoted you might be able to use this.");
                break;
        }
    }

    private bool TryUseItemOnCharacter(PartyMember selectedPartyMember) {
        if (_firstSelectedItem.healValue > 0) {
            //TODO introduce to battle menu
            var toHeal = selectedPartyMember.charStats.maxHp - selectedPartyMember.charStats.currentHp;
            if (toHeal == 0) {
                EvokeSingleSentenceDialogue($"{selectedPartyMember.name} does not need any healing!");
                return false;
            }
            toHeal = toHeal >= _firstSelectedItem.healValue ? _firstSelectedItem.healValue : toHeal;
            selectedPartyMember.charStats.currentHp += toHeal;
            EvokeSingleSentenceDialogue($"{selectedPartyMember.name} was healed for {toHeal} points!");
            return true;
        } else if (_firstSelectedItem.itemName.Equals("ANTIDOTE")) {
            if (selectedPartyMember.statusEffects.HasFlag(StatusEffect.poisoned)) {
                selectedPartyMember.statusEffects.Remove(StatusEffect.poisoned);
                EvokeSingleSentenceDialogue($"{selectedPartyMember.name} is no longer poisoned!");
                return true;
            } else {
                EvokeSingleSentenceDialogue($"{selectedPartyMember.name} is not poisoned!");
                return false;
            }
        } else if (_firstSelectedItem.itemName.Equals("PHOENIX FEATHER")) {
            if (selectedPartyMember.statusEffects.HasFlag(StatusEffect.dead)) {
                selectedPartyMember.statusEffects.Remove(StatusEffect.dead);
                EvokeSingleSentenceDialogue($"{selectedPartyMember.name} is no longer dead!");
                return true;
            } else {
                EvokeSingleSentenceDialogue($"{selectedPartyMember.name} is not dead!");
                return false;
            }
        }
        return false;
    }
    private void HandleGiveMenu(GameItem selectedItem) {
        if (_firstSelectedItem == null) {
            _itemsToTradeOne.sprite = selectedItem.ItemSprite;
            _firstSelectedItem = selectedItem;
            EvokeSingleSentenceDialogue(_itemDialogue.Replace("#ITEMNAME#", selectedItem.itemName));
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
        } else {
            _secondSelectedItem = selectedItem;

            var sentence = "";
            if (!selectedItem.IsSet()) {
                sentence = $"Gave {_firstSelectedPartyMember.name}'s {_firstSelectedItem.itemName} " +
                           $"to {_secondSelectedPartyMember.name}";
            } else {
                sentence = $"Swapped {_firstSelectedPartyMember.name}'s {_firstSelectedItem.itemName} " +
                           $"with {_secondSelectedPartyMember.name}'s {_secondSelectedItem.itemName}";
            }
            EvokeSingleSentenceDialogue(sentence);

            _itemsToTradeTwo.sprite = selectedItem.ItemSprite;
            _inventory.SwapItems(_firstSelectedPartyMember, _secondSelectedPartyMember,
                _firstSelectedItem, _secondSelectedItem);

            LoadInventory(_secondSelectedPartyMember);
            ClearAllSelection();
            _inInventoryMenu = false;
            _memberInventoryUI.UnselectObject();
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
                    OpenObjectMenu();
                    _currentMenuType = CurrentMenu.use;
                    CloseObjectButtonMenu();
                    break;
                case "Give":
                    OpenObjectMenu();
                    _currentMenuType = CurrentMenu.give;
                    CloseObjectButtonMenu();
                    break;
                case "Drop":
                    OpenObjectMenu();
                    _currentMenuType = CurrentMenu.drop;
                    CloseObjectButtonMenu();
                    break;
                case "Equip":
                    OpenObjectMenu();
                    _currentMenuType = CurrentMenu.equip;
                    CloseObjectButtonMenu();
                    break;
            }
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
        _listCreator.LoadCharacterList(_party);
        _listCreator.DrawBoundary(_currentItemSelected, _currentItemSelected);
        LoadInventory(_party[_currentItemSelected]);
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

    /*
    IEnumerator WaitCloseSetActiveFalse(List<GameObject> gameObjects) {
        yield return new WaitForSeconds(0.25f);
        foreach (var thisGameObject in gameObjects) {
            thisGameObject.SetActive(false);
        }
    }
    */

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
        var gameItem = partyMember.partyMemberInventory;
        _memberInventoryUI.LoadMemberInventory(gameItem);
        LoadPortraitOfMember(partyMember);
    }

    private void LoadPortraitOfMember(PartyMember partyMember) {
        var image = portrait.transform.GetChild(0).GetComponent<Image>();
        var sprite = partyMember.portraitSprite;
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
        yield return new WaitForSeconds(0.25f);
        foreach (var thisGameObject in gameObjects) {
            thisGameObject.SetActive(false);
        }

        StartCoroutine(coroutine);
    }

    IEnumerator WaitForQuaterSecCloseMainMenu() {
        yield return new WaitForSeconds(0.25f);
        mainMenu.SetActive(false);
        objectMenu.SetActive(false);
        portrait.SetActive(false);
        characterSelector.SetActive(false);
        Player.InputDisabled = false;
    }

    #endregion

    private enum CurrentMenu {
        none,
        magic,
        member,
        use,
        give,
        drop,
        equip
    }
}
