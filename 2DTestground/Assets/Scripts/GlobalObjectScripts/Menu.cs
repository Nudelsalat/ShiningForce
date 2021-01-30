using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public static bool isInMenu = false;
    public static bool isInObjectMenu = false;
    public static bool isPause = false;

    public GameObject pauseUI;
    public GameObject objectMenu;
    public GameObject mainMenu;
    public GameObject characterSelector;
    public GameObject portrait;
    
    public Animator animatorInventory;
    public Animator animatorMainMenuButtons;
    public Animator animatorCharacterSelector;
    public Animator animatorPortrait;

    private Animator _animatorMemberButton;
    private Animator _animatorMagicButton;
    private Animator _animatorSearchButton;
    private Animator _animatorItemButton;
    private Animator _currentlyAnimatedButton;

    private Inventory _gameInventory;
    private ListCreator _listCreator;
    private int _currentItemSelected;
    private List<PartyMember> _party;

    void Awake() {
        _listCreator = characterSelector.GetComponent<ListCreator>();
        _gameInventory = Inventory.Instance;
        _animatorMemberButton = mainMenu.transform.Find("Buttons/Member").GetComponent<Animator>();
        _animatorMagicButton = mainMenu.transform.Find("Buttons/Magic").GetComponent<Animator>();
        _animatorSearchButton = mainMenu.transform.Find("Buttons/Search").GetComponent<Animator>();
        _animatorItemButton = mainMenu.transform.Find("Buttons/Item").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Select")) {
            if (!isPause) {
                Pause();
            }
        }
        if (isPause) {
            if (Input.GetButtonUp("Back")) {
                Resume();
            }
            return;
        }

        if (Player.IsInDialogue) {
            return;
        }
        
        if (isInMenu) {
            if (isInObjectMenu) {
                if (Input.GetButtonUp("Back")) {
                    if (isInObjectMenu) {
                        CloseObjectMenu();
                        isInObjectMenu = false;
                    }
                }

                HandleObjectMenu();
                return;
            }

            if (Input.GetButtonUp("Back")) {
                if (isInMenu) {
                    CloseMainMenuForGood();
                    isInMenu = false;
                } 
            }

            HandleMainMenu();
            return;
        }



        if (Input.GetButtonUp("Menu")) {
            if (!isInMenu) {
                OpenMainMenu();
            }
            isInMenu = true;
        }
    }



    void HandleObjectMenu() {
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
        }
    }

    void HandleMainMenu() {
        if (Input.GetAxisRaw("Horizontal") > 0.05f) {
            _currentlyAnimatedButton.SetBool("selected", false);
            _animatorItemButton.SetBool("selected", true);
            _currentlyAnimatedButton = _animatorItemButton;
        } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
            _currentlyAnimatedButton.SetBool("selected", false);
            _animatorMagicButton.SetBool("selected", true);
            _currentlyAnimatedButton = _animatorMagicButton;
        } else if (Input.GetAxisRaw("Vertical") > 0.05f) {
            _currentlyAnimatedButton.SetBool("selected", false);
            _animatorMemberButton.SetBool("selected", true);
            _currentlyAnimatedButton = _animatorMemberButton;
        } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
            _currentlyAnimatedButton.SetBool("selected", false);
            _animatorSearchButton.SetBool("selected", true);
            _currentlyAnimatedButton = _animatorSearchButton;
        }

        if (Input.GetButtonUp("Interact")) {
            switch (_currentlyAnimatedButton.name) {
                case "Member":
                    OpenObjectMenu();
                    CloseMainMenu();
                    break;
                case "Magic":
                    break;
                case "Search":
                    break;
                case "Item":
                    break;
            }
        }
    }

    void Resume() {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPause = false;
    }

    void Pause() {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPause = true;
    }

    void OpenMainMenu() {
        Player.InputDisabled = true;
        mainMenu.SetActive(true);
        animatorMainMenuButtons.SetBool("mainMenuIsOpen", true);
        _animatorMemberButton.SetBool("selected", true);
        _currentlyAnimatedButton = _animatorMemberButton;
    }

    void OpenObjectMenu() {
        _party = _gameInventory.GetParty();
        isInObjectMenu = true;
        portrait.SetActive(true);
        objectMenu.SetActive(true);

        OpenCharacterSelectMenu();

        animatorPortrait.SetBool("portraitIsOpen", true);
        animatorInventory.SetBool("inventoryIsOpen", true);
    }

    void OpenCharacterSelectMenu() {
        characterSelector.SetActive(true);
        _currentItemSelected = 0;
        _listCreator.LoadCharacterList(_party);
        _listCreator.DrawBoundary(_currentItemSelected, _currentItemSelected);
        LoadInventory(_party.FirstOrDefault());
        animatorCharacterSelector.SetBool("characterSelectorIsOpen", true);
    }

    void CloseObjectMenu() {
        animatorPortrait.SetBool("portraitIsOpen", false);
        animatorInventory.SetBool("inventoryIsOpen", false);
        animatorCharacterSelector.SetBool("characterSelectorIsOpen", false);
        _listCreator.WhipeCharacterList();
        StartCoroutine(WaitCloseSetActiveFalse(new List<GameObject>() {
            objectMenu,
            portrait,
            characterSelector
        }));
        OpenMainMenu();
    }

    void CloseMainMenu() {
        animatorMainMenuButtons.SetBool("mainMenuIsOpen", false);
        StartCoroutine(WaitCloseSetActiveFalse(new List<GameObject>() {
            mainMenu
        }));
    }

    void CloseMainMenuForGood() {
        animatorMainMenuButtons.SetBool("mainMenuIsOpen", false);
        StartCoroutine(WaitForQuaterSecCloseMainMenu());
    }

    IEnumerator WaitCloseSetActiveFalse(List<GameObject> gameObjects) {
        yield return new WaitForSeconds(0.25f);
        foreach (var thisGameObject in gameObjects) {
            thisGameObject.SetActive(false);
        }}
    
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
        Player.InputDisabled = false;
    }

    void LoadInventory(PartyMember partyMember) {
        var gameItem = partyMember.partyMemberInventory;
        MemberInventoryUI.LoadMemberInventory(gameItem);
        LoadPortraitOfMember(partyMember);
    }

    void LoadPortraitOfMember(PartyMember partyMember) {
        var image = portrait.transform.GetChild(0).GetComponent<Image>();
        var sprite = partyMember.portraitSprite;
        image.sprite = sprite;
    }
}
