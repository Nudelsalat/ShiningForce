using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Assets.Scripts.Menus {
    public class MerchantMenu : MonoBehaviour {

        public GameObject Buttons;
        public GameObject ObjectsForSale;
        public GameObject Gold;
        public GameObject CharacterSelector;

        private Animator _animatorButtons;
        private Animator _animatorObjectsForSale;
        private Animator _animatorGold;

        private Animator _animatorBuyButton;
        private Animator _animatorSellButton;
        private Animator _animatorDealsButton;
        private Animator _animatorRepairButton;

        private Dialogue _tempDialogue = new Dialogue {
            Name = "Itemtext",
            Sentences = new List<string>() {
                "SENTENCE NOT REPLACED!"
            },
        };

        private GameObject _buyItem;
        private Animator _currentlyAnimatedButton;
        private Portrait _portrait;
        private DialogManager _dialogManager;
        private GameItem _itemToSell;
        private List<GameItem> _itemsToBuy;
        private List<GameItem> _itemsInCurrentMenu;
        private List<PartyMember> _party;
        private Text _buttonLabel;
        private bool _menuActive = false;
        private bool _inInventoryMenu = false;
        private DirectionType _inputDirection;
        private DirectionType _lastInputDirection;
        private EnumCurrentMerchantMenu _enumCurrentMenuType;
        private EnumCurrentMerchantMenu _previousState;
        private int _currentListItemSelected = 0;
        private int _currentBuyItemSelected = 0;
        private int _currentBuyItemPage = 0;
        private double _pageSizeBuy = 0;

        private CharacterSelector _characterSelector;
        private Inventory _inventory;
        private MemberInventoryUI _memberInventoryUi;

        public static MerchantMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _buyItem = Resources.Load<GameObject>("SharedObjects/BuyItem");

            _characterSelector = CharacterSelector.GetComponent<CharacterSelector>();

            _buttonLabel = Buttons.transform.Find("Label/LabelText").GetComponent<Text>();

            _animatorBuyButton = Buttons.transform.Find("Buy").GetComponent<Animator>();
            _animatorSellButton = Buttons.transform.Find("Sell").GetComponent<Animator>();
            _animatorDealsButton = Buttons.transform.Find("Deals").GetComponent<Animator>();
            _animatorRepairButton = Buttons.transform.Find("Repair").GetComponent<Animator>();

            _animatorButtons = Buttons.transform.GetComponent<Animator>();
            _animatorObjectsForSale = ObjectsForSale.transform.GetComponent<Animator>();
            _animatorGold = Gold.transform.GetComponent<Animator>();
        }

        void Start() {
            _memberInventoryUi = MemberInventoryUI.Instance;
            _inventory = Inventory.Instance;
            _dialogManager = DialogManager.Instance;
            _portrait = Portrait.Instance;
        }

        #region OverAllInput


        // Update is called once per frame
        void Update() {
            if (!_menuActive) {
                return;
            }
            GetInputDirection();

            if (Player.IsInDialogue || Player.InputDisabledInDialogue || Player.InputDisabledInEvent) {
                if (Input.GetButtonUp("Interact") && !Player.InputDisabledInDialogue && !Player.InputDisabledInEvent) {
                    _dialogManager.DisplayNextSentence();
                }
                return;
            }

            switch (_enumCurrentMenuType) {
                case EnumCurrentMerchantMenu.buy:
                case EnumCurrentMerchantMenu.deals:
                    HandleBuy();
                    break;
                case EnumCurrentMerchantMenu.sell:
                    HandleSell();
                    break;
                case EnumCurrentMerchantMenu.repair:
                    HandleRepair();
                    break;
                case EnumCurrentMerchantMenu.buyToWhom:
                    HandleBuyToWhom();
                    break;
                case EnumCurrentMerchantMenu.none:
                    HandleMerchantMenu();
                    break;
            }
        }
        #endregion

        public void OpenMerchantWindow(List<GameItem> itemsToSell) {
            _party = _inventory.GetParty();
            Buttons.SetActive(true);
            ObjectsForSale.SetActive(true);
            Gold.SetActive(true);
            SetButtonActiveAndDeactivateLastButton(_animatorBuyButton);
            OpenMerchantMenu();
            
            _itemsToBuy = itemsToSell;
        }

        private void OpenMerchantMenu() {
            _animatorButtons.SetBool("mainMenuIsOpen", true);
            _menuActive = true;
        }

        private void HandleMerchantMenu() {
            switch (_inputDirection) {
                case DirectionType.up:
                    SetButtonActiveAndDeactivateLastButton(_animatorBuyButton);
                    break;
                case DirectionType.left:
                    SetButtonActiveAndDeactivateLastButton(_animatorSellButton);
                    break;
                case DirectionType.down:
                    SetButtonActiveAndDeactivateLastButton(_animatorDealsButton);
                    break;
                case DirectionType.right:
                    SetButtonActiveAndDeactivateLastButton(_animatorRepairButton);
                    break;
            }

            if (Input.GetButtonUp("Interact")) {
                switch (_currentlyAnimatedButton.name) {
                    case "Buy":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.buy;
                        OpenBuyMenu(_itemsToBuy);
                        CloseButtonMenu();
                        break;
                    case "Sell":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.sell;
                        OpenSellMenu();
                        CloseButtonMenu();
                        break;
                    case "Deals":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.deals;
                        if (_inventory.GetDeals().Count > 0) {
                            OpenBuyMenu(_inventory.GetDeals());
                            CloseButtonMenu();
                        } else {
                            EvokeSingleSentenceDialogue("Sorry, pal. Don't got any deals for ya!");
                        }
                        break;
                    case "Repair":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.repair;
                        //TODO
                        OpenRepairMenu();
                        CloseButtonMenu();
                        break;
                }
            }

            if (Input.GetButtonUp("Back")) {
                CloseMenuForGood();
            }
        }

        private void HandleBuy() {
            if (_inputDirection == DirectionType.left && _currentBuyItemSelected > 0) {
                _currentBuyItemSelected--;
                LoadItemsToBuy();
            } else if (_inputDirection == DirectionType.right && _currentBuyItemSelected < 7) {
                _currentBuyItemSelected++;
                LoadItemsToBuy();
            } else if(_inputDirection == DirectionType.up && _currentBuyItemPage > 0) {
                _currentBuyItemPage--;
                LoadItemsToBuy();
            } else if (_inputDirection == DirectionType.down && _currentBuyItemPage < _pageSizeBuy-1) {
                _currentBuyItemPage++;
                LoadItemsToBuy();
            }

            if (Input.GetButtonUp("Interact")) {
                var itemToBuy = _itemsInCurrentMenu.ToArray()[_currentBuyItemSelected + _currentBuyItemPage * 8];
                if (!HasEnoughGoldForItem(itemToBuy)) {
                    EvokeSingleSentenceDialogue($"Ups... You don't have enough gold.");
                    return;
                }
                EvokeSingleSentenceDialogue($"Give {itemToBuy.ItemName.AddColor(Color.green)} to whom?");
                var equipment = itemToBuy is Equipment ? (Equipment) itemToBuy : null;

                OpenCharacterSelectMenu(equipment);

                _previousState = _enumCurrentMenuType;
                _enumCurrentMenuType = EnumCurrentMerchantMenu.buyToWhom;
                CloseBuyMenu();
                return;
            }

            if (Input.GetButtonUp("Back")) {
                _enumCurrentMenuType = EnumCurrentMerchantMenu.none;
                CloseBuyMenu();
                OpenMerchantMenu();
            }
        }

        private void HandleBuyToWhom() {
            var selectedMember = DoCharacterSelection();

            if (Input.GetButtonUp("Back")) {
                _enumCurrentMenuType = _previousState;
                CloseCharacterSelection();
                OpenBuyMenu(_itemsInCurrentMenu);
            }

            if (Input.GetButtonUp("Interact")) {
                var inventory = selectedMember.CharacterInventory;
                for (int i = 0; i < 4; i++) {
                    if (inventory[i].IsSet()) {
                        continue;
                    }
                    
                    var itemToBuy = UnityEngine.Object.Instantiate(_itemsInCurrentMenu
                        [_currentBuyItemSelected + _currentBuyItemPage * 8]);
                    var sentence = "";
                    if (!TryPayForItem(itemToBuy)) {
                        sentence = $"Ups... You don't have enough gold!";
                    } else {
                        sentence = $"{selectedMember.Name.AddColor(Constants.Orange)} " +
                                       $"received {itemToBuy.ItemName.AddColor(Color.green)}.";
                        _inventory.RemoveFromDeals(_itemsInCurrentMenu
                            [_currentBuyItemSelected + _currentBuyItemPage * 8]);
                        inventory[i] = itemToBuy;
                    }
                    _enumCurrentMenuType = _previousState;
                    CloseCharacterSelection();
                    OpenBuyMenu(_itemsInCurrentMenu);
                    EvokeSingleSentenceDialogue(sentence);
                    return;
                }
                EvokeSingleSentenceDialogue($"{selectedMember.Name.AddColor(Constants.Orange)} " +
                                            $"already has enough items.");
            }
        }

        private void HandleSell() {
            if (_inInventoryMenu) {
                if (Input.GetButtonUp("Back")) {
                    _inInventoryMenu = false;
                    _memberInventoryUi.UnselectObject();
                }
                if (_inputDirection != DirectionType.none) {
                    _memberInventoryUi.SelectObject(_inputDirection);
                }
                if (Input.GetButtonUp("Interact")) {
                    var selectedItem = _memberInventoryUi.GetSelectedGameItem();
                    TrySellSelectedItem(selectedItem);
                }

                return;
            }

            DoCharacterSelection();

            if (Input.GetButtonUp("Back")) {
                CloseSellMenu();
            }

            if (Input.GetButtonUp("Interact")) {
                _inInventoryMenu = true;
                _memberInventoryUi.SelectObject(DirectionType.up);
            }
        }

        private void TrySellSelectedItem(GameItem selectedItem) {
            if (!selectedItem.IsSet()) {
                EvokeSingleSentenceDialogue("Please select an item to sell...");
            }

            _itemToSell = selectedItem;
            var halfValue = selectedItem.Price / 2;

            var dropItemCallback = new QuestionCallback {
                Name = "DropText",
                Sentences = new List<string>() {
                    $"For that {selectedItem.ItemName.AddColor(Color.green)} " +
                    $"I can give you {halfValue.ToString()}.\nDo we have a deal?"
                },
                DefaultSelectionForQuestion = YesNo.No,
                OnAnswerAction = DecisionSellItem,
            };
            _dialogManager.StartDialogue(dropItemCallback);
        }

        private void HandleRepair() {
            if (_inInventoryMenu) {
                if (Input.GetButtonUp("Back")) {
                    _inInventoryMenu = false;
                    _memberInventoryUi.UnselectObject();
                }
                if (_inputDirection != DirectionType.none) {
                    _memberInventoryUi.SelectObject(_inputDirection);
                }
                if (Input.GetButtonUp("Interact")) {
                    var selectedItem = _memberInventoryUi.GetSelectedGameItem();
                    TryRepairSelectedItem(selectedItem);
                }

                return;
            }

            DoCharacterSelection();

            if (Input.GetButtonUp("Back")) {
                CloseSellMenu();
            }

            if (Input.GetButtonUp("Interact")) {
                _inInventoryMenu = true;
                _memberInventoryUi.SelectObject(DirectionType.up);
            }
        }

        private void TryRepairSelectedItem(GameItem itemToRepair) {
            //TODO
            EvokeSingleSentenceDialogue("SORRY THIS IS NOT YET IMPLEMENTED...");
        }

        private void DecisionSellItem(bool sold) {
            var sentence = "";
            if (sold) {
                var currentMember = _party[_currentListItemSelected];
                _inventory.AddGold(_itemToSell.Price/2);
                currentMember.RemoveItem(_itemToSell);
                _inInventoryMenu = false;
                _memberInventoryUi.UnselectObject();
                OpenCharacterSelectMenu(null);
                OpenGold();
                
                sentence = "I like making business with you.\nAnything else?";
            } else {
                sentence = "Wanna hang on to it? Alright...\nAnything else?";
            }

            StartCoroutine(DisplaySentenceWithDelay(sentence));
        }

        private void OpenBuyMenu(List<GameItem> itemsToSellList) {
            if (itemsToSellList.Count <= 0) {
                _enumCurrentMenuType = EnumCurrentMerchantMenu.none;
                CloseBuyMenu();
                OpenMerchantMenu();
                EvokeSingleSentenceDialogue("Sorry, I'm out of stock...");
                return;
            }
            EvokeSingleSentenceDialogue("What do you want to buy?");

            _animatorObjectsForSale.SetBool("isOpen", true);
            _itemsInCurrentMenu = itemsToSellList;
            OpenGold();
            LoadItemsToBuy();
        }

        private void CloseBuyMenu() {
            _animatorObjectsForSale.SetBool("isOpen", false);
            CloseGold();
        }

        private void OpenSellMenu() {
            EvokeSingleSentenceDialogue("Let me see your goods...");
            OpenGold();
            OpenCharacterSelectMenu(null);
        }

        private void CloseSellMenu() {
            _enumCurrentMenuType = EnumCurrentMerchantMenu.none;
            CloseGold();
            CloseCharacterSelection();
            OpenMerchantMenu();
        }

        private void OpenRepairMenu() {
            EvokeSingleSentenceDialogue("What can I repair for ya?");
            OpenGold();
            OpenCharacterSelectMenu(null);
        }

        private void CloseButtonMenu() {
            _animatorButtons.SetBool("mainMenuIsOpen", false);
        }

        private void CloseMenuForGood() {
            _animatorButtons.SetBool("mainMenuIsOpen", false);
            StartCoroutine(WaitForQuaterSecCloseMainMenu());
            _menuActive = false;
        }
        private void OpenCharacterSelectMenu(Equipment equipment) {
            _characterSelector.LoadCharacterList(_party, equipment, _currentListItemSelected);
            LoadInventory(_party[_currentListItemSelected]);
        }

        private void SetButtonActiveAndDeactivateLastButton(Animator animator) {
            if (_currentlyAnimatedButton != null) {
                _currentlyAnimatedButton.SetBool("selected", false);
            }

            _buttonLabel.text = animator.name;
            animator.SetBool("selected", true);
            _currentlyAnimatedButton = animator;
        }

        private void LoadItemsToBuy() {
            var upDown = ObjectsForSale.transform.Find("UpDown").gameObject;
            var itemCount = _itemsInCurrentMenu.Count();
            _pageSizeBuy = Math.Ceiling((float)itemCount / 8);
            if (_currentBuyItemPage > _pageSizeBuy-1) {
                _currentBuyItemPage = (int)_pageSizeBuy-1;
            }

            if (_currentBuyItemPage == (int)_pageSizeBuy - 1) {
                var lastItemOnPage = (itemCount % 8) - 1;
                if (_currentBuyItemSelected > lastItemOnPage) {
                    _currentBuyItemSelected = lastItemOnPage;
                }
            }

            if (_currentBuyItemPage == 0 && _pageSizeBuy > 1) {
                upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Visible;
            } else {
                upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Invisible;
            }

            if (_pageSizeBuy > 1 && _currentBuyItemPage != 0) {
                upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Visible;
            }else {
                upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Invisible;
            }

            var spawnPoint = ObjectsForSale.transform.Find("ItemSpawnPoint");
            foreach (Transform child in spawnPoint.transform) {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < 8; i++) {
                var nextItemIndex = i + _currentBuyItemPage * 8;
                if (nextItemIndex >= itemCount) {
                    return;
                }
                var pos = new Vector3(0, 0, spawnPoint.position.z);
                var spawnedItem = Instantiate(_buyItem, pos, spawnPoint.rotation);
                spawnedItem.transform.SetParent(spawnPoint, false);

                var itemDetails = _itemsInCurrentMenu.ToArray()[nextItemIndex];
                var spawnedItemImage = spawnedItem.transform.GetComponent<Image>();
                var spawnedItemText = spawnedItem.transform.GetChild(0).GetComponent<Text>();

                spawnedItemImage.sprite = itemDetails.ItemSprite;
                spawnedItemText.text = itemDetails.Price.ToString();

                var selectionColor = i == _currentBuyItemSelected ? Constants.Orange : Constants.Visible;

                spawnedItemImage.color = selectionColor;
                spawnedItemText.color = selectionColor;
            }
        }

        private void LoadInventory(PartyMember partyMember) {
            _memberInventoryUi.LoadMemberInventory(partyMember);
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
                _inputDirection = DirectionType.none;
            }

            if (currentDirection == _lastInputDirection) {
                _inputDirection = DirectionType.none;
            } else {
                _lastInputDirection = _inputDirection = currentDirection;
            }

        }

        private PartyMember DoCharacterSelection() {
            var selectedMember = _party[_currentListItemSelected];

            var previousSelected = _currentListItemSelected;
            if (_inputDirection == DirectionType.down && _currentListItemSelected != _party.Count - 1) {
                _currentListItemSelected++;
                selectedMember = _party[_currentListItemSelected];

                LoadInventory(selectedMember);
                _characterSelector.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
            } else if (_inputDirection == DirectionType.up && _currentListItemSelected != 0) {
                _currentListItemSelected--;
                selectedMember = _party[_currentListItemSelected];

                LoadInventory(selectedMember);
                _characterSelector.SetScrollbar(previousSelected, _currentListItemSelected, _party.Count);
            }

            return selectedMember;
        }

        private void CloseCharacterSelection() {
            _characterSelector.ClearCharacterList();
            _memberInventoryUi.CloseInventory();
            _portrait.HidePortrait();
        }

        private bool HasEnoughGoldForItem(GameItem itemToBuy) {
            var currentGold = _inventory.GetGold();
            return currentGold >= itemToBuy.Price;
        }

        private bool TryPayForItem(GameItem itemToBuy) {
            var result = HasEnoughGoldForItem(itemToBuy);
            if (result) {
                _inventory.RemoveGold(itemToBuy.Price);
            }
            return result;
        }

        private void OpenGold() {
            _animatorGold.SetBool("isOpen", true);
            Gold.transform.Find("GoldText").GetComponent<Text>().text = _inventory.GetGold().ToString();
        }

        private void CloseGold() {
            _animatorGold.SetBool("isOpen", false);
        }

        private void EvokeSingleSentenceDialogue(string sentence) {
            _tempDialogue.Sentences.Clear();
            _tempDialogue.Sentences.Add(sentence);
            _dialogManager.StartDialogue(_tempDialogue);
        }

        IEnumerator WaitForQuaterSecCloseMainMenu() {
            yield return new WaitForSeconds(0.1f);
            Buttons.SetActive(false);
            ObjectsForSale.SetActive(false);
            Gold.SetActive(false);
            Player.InputDisabled = false;
        }

        IEnumerator DisplaySentenceWithDelay(string sentence) {
            yield return new WaitForSeconds(0.15f);
            EvokeSingleSentenceDialogue(sentence);
        }
    }

    public enum EnumCurrentMerchantMenu {
        none,
        buy,
        buyToWhom,
        sell,
        deals,
        repair
    }
}
