using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

    private Animator _currentlyAnimatedButton;

    private Dialogue _dialogue;

    private Inventory _gameInventory;
    private ListCreator _listCreator;
    private int _currentItemSelected = 0;
    private List<PartyMember> _party;
    private CurrentMenu _currentMenuType = CurrentMenu.none;

    private bool _isInMainButtonMenu = false;
    private bool _isInObjectButtonMenu = false;
    private bool _inInventoryMenu = false;
    private bool _isPause = false;


    void Awake() {
        _listCreator = characterSelector.GetComponent<ListCreator>();

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

        _dialogue = new Dialogue {
            Name = "SearchText",
            Sentences = new List<string>() {
                "This Button Function will be replaced with the 'Interact' Key. It's easier that way...",
                "And also:\n#LEADERNAME# investigated the area.\nNothing was there."
            },
        };
    }

    void Start() {
        _gameInventory = Inventory.Instance;
    }

    #region OverAllInput
    // Update is called once per frame
    void Update()
    {
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

        if (Player.IsInDialogue) {
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
                if (Input.GetButtonUp("Back")) {
                    CloseObjectButtonMenu();
                    _isInObjectButtonMenu = false;
                    OpenMainButtonMenu();
                }
                HandleObjectButtonMenu();
                return;
            }

            if (Input.GetButtonUp("Back")) {
                if (_isInMainButtonMenu) {
                    CloseMainMenuForGood();
                } 
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
        } else if (Input.GetButtonUp("Back")) {
            switch (_currentMenuType ) {
                case CurrentMenu.use:
                case CurrentMenu.drop:
                case CurrentMenu.give:
                case CurrentMenu.equip:
                    CloseObjectMenu();
                    _currentMenuType = CurrentMenu.none;
                    OpenObjectButtonMenu();
                    break;
                case CurrentMenu.member:
                case CurrentMenu.magic:
                    _currentMenuType = CurrentMenu.none;
                    OpenMainButtonMenu();
                    break;
            }
        } else if (Input.GetButtonUp("Interact")) {
            _inInventoryMenu = true;
        }
    }

    private void HandleInventoryMenu() {
        
    }

    #endregion

    private void HandleMainMenu() {
        if (Input.GetAxisRaw("Horizontal") > 0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorItemButton, _textMainButtonMenu);
        } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorMagicButton, _textMainButtonMenu);
        } else if (Input.GetAxisRaw("Vertical") > 0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorMemberButton, _textMainButtonMenu);
        } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorSearchButton, _textMainButtonMenu);
        }

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton.name) {
                case "Member":
                    break;
                case "Magic":
                    break;
                case "Search":
                    CloseMainMenuForGood();
                    FindObjectOfType<DialogManager>().StartDialogue(_dialogue);
                    break;
                case "Item":
                    CloseMainButtonMenu();
                    OpenObjectButtonMenu();
                    break;
            }
        }
    }

    private void HandleObjectButtonMenu() {
        if (Input.GetAxisRaw("Horizontal") > 0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorEquipButton, _textObjectButtonMenu);
        } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorGiveButton, _textObjectButtonMenu);
        } else if (Input.GetAxisRaw("Vertical") > 0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorUseButton, _textObjectButtonMenu);
        } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
            SetButtonActiveAndDeactivateLastButton(_animatorDropButton, _textObjectButtonMenu);
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
        _party = _gameInventory.GetParty();

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
        _listCreator.WhipeCharacterList();
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
        MemberInventoryUI.LoadMemberInventory(gameItem);
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
