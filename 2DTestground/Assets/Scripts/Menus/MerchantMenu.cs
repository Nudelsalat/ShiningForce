using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class MerchantMenu : MonoBehaviour {

        public GameObject Buttons;
        public GameObject ObjectsForSale;
        public GameObject Gold;
        public GameObject CharacterSelector;
        public GameObject Portrait;
        public GameObject ObjectMenu;

        private Animator _animatorButtons;
        private Animator _animatorObjectsForSale;
        private Animator _animatorGold;

        private Animator _animatorBuyButton;
        private Animator _animatorSellButton;
        private Animator _animatorDealsButton;
        private Animator _animatorRepairButton;

        private Animator _animatorCharacterSelector;
        private Animator _animatorPortrait;
        private Animator _animatorInventory;
        
        private static Sprite _blankSprite;

        private GameObject _buyItem;
        private Animator _currentlyAnimatedButton;
        private ListCreator _listCreator;
        private Inventory _inventory;
        private MemberInventoryUI _memberInventoryUI;
        private DialogManager _dialogManager;
        private List<GameItem> _itemsToSell;
        private Text _buttonLabel;
        private bool _menuActive = false;
        private DirectionType _inputDirection;
        private EnumCurrentMerchantMenu _enumCurrentMenuType;
        private int _currentListItemSelected = 0;


        public static MerchantMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            }
            else {
                Instance = this;
            }

            _buyItem = Resources.Load<GameObject>("SharedObjects/BuyItem");
            _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");

            _dialogManager = FindObjectOfType<DialogManager>();
            _listCreator = CharacterSelector.GetComponent<ListCreator>();

            _animatorPortrait = Portrait.GetComponent<Animator>();
            _animatorCharacterSelector = CharacterSelector.GetComponent<Animator>();
            _animatorInventory = ObjectMenu.transform.Find("Inventory").GetComponent<Animator>();

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
            _memberInventoryUI = MemberInventoryUI.Instance;
            _inventory = Inventory.Instance;
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

            HandleMerchantMenu();
        }
        #endregion

        public void OpenMerchantWindow(List<GameItem> itemsToSell) {
            Buttons.SetActive(true);
            ObjectsForSale.SetActive(true);
            Gold.SetActive(true);
            SetButtonActiveAndDeactivateLastButton(_animatorBuyButton);

            _animatorButtons.SetBool("mainMenuIsOpen", true);
            _menuActive = true;
            _itemsToSell = itemsToSell;
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
                        OpenBuyMenu();
                        CloseButtonMenu();
                        break;
                    case "Sell":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.sell;
                        OpenSellMenu();
                        CloseButtonMenu();
                        break;
                    case "Deals":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.deals;
                        //TODO
                        CloseButtonMenu();
                        break;
                    case "Repair":
                        _enumCurrentMenuType = EnumCurrentMerchantMenu.repair;
                        //TODO
                        CloseButtonMenu();
                        break;
                }
            }

            if (Input.GetButtonUp("Back")) {
                CloseMenuForGood();
            }
        }

        public void OpenBuyMenu() {
            _animatorObjectsForSale.SetBool("isOpen", true);
            _animatorGold.SetBool("isOpen", true);
            LoadItemsToBuy(0,0);
        }

        private void OpenSellMenu() {
            OpenCharacterSelectMenu();
            LoadInventory(_inventory.GetParty()[_currentListItemSelected]);

            _animatorPortrait.SetBool("portraitIsOpen", true);
            _animatorInventory.SetBool("inventoryIsOpen", true);
        }

        private void CloseButtonMenu() {
            _animatorButtons.SetBool("mainMenuIsOpen", false);
        }

        private void CloseMenuForGood() {
            _animatorButtons.SetBool("mainMenuIsOpen", false);
            StartCoroutine(WaitForQuaterSecCloseMainMenu());
            _menuActive = false;
        }
        private void OpenCharacterSelectMenu() {
            CharacterSelector.SetActive(true);
            _listCreator.LoadCharacterList(_inventory.GetParty(), null, _currentListItemSelected);
            _animatorCharacterSelector.SetBool("characterSelectorIsOpen", true);
        }

        private void SetButtonActiveAndDeactivateLastButton(Animator animator) {
            if (_currentlyAnimatedButton != null) {
                _currentlyAnimatedButton.SetBool("selected", false);
            }

            _buttonLabel.text = animator.name;
            animator.SetBool("selected", true);
            _currentlyAnimatedButton = animator;
        }

        private void LoadItemsToBuy(int selected, int page) {
            var upDown = ObjectsForSale.transform.Find("UpDown").gameObject;
            var itemCount = _itemsToSell.Count();
            var pageSize = Math.Ceiling((float)itemCount / 8);

            if (page == 0 && pageSize > 1) {
                upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Visible;
            } else {
                upDown.transform.Find("Down").GetComponent<Image>().color = Constants.Invisible;
            }

            if (pageSize > 1 && page != 0) {
                upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Visible;
            }else {
                upDown.transform.Find("Up").GetComponent<Image>().color = Constants.Invisible;
            }

            var spawnPoint = ObjectsForSale.transform.Find("ItemSpawnPoint");
            foreach (Transform child in spawnPoint.transform) {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < 8; i++) {
                var nextItemIndex = i + page * 8;
                if (nextItemIndex >= itemCount) {
                    return;
                }
                var pos = new Vector3(0, 0, spawnPoint.position.z);
                var spawnedItem = Instantiate(_buyItem, pos, spawnPoint.rotation);
                spawnedItem.transform.SetParent(spawnPoint, false);
                var itemDetails = _itemsToSell.ToArray()[nextItemIndex];
                spawnedItem.transform.GetComponent<Image>().sprite = itemDetails.ItemSprite;
                spawnedItem.transform.GetChild(0).GetComponent<Text>().text = itemDetails.Price.ToString();
            }
        }

        private void LoadInventory(PartyMember partyMember) {
            _memberInventoryUI.LoadMemberInventory(partyMember);
            LoadPortraitOfMember(partyMember);
        }

        private void LoadPortraitOfMember(Character partyMember) {
            var image = Portrait.transform.Find("PortraitPicture").GetComponent<Image>();
            var sprite = partyMember.PortraitSprite;
            image.sprite = sprite != null ? sprite : _blankSprite;
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

        IEnumerator WaitForQuaterSecCloseMainMenu() {
            yield return new WaitForSeconds(0.1f);
            Buttons.SetActive(false);
            ObjectsForSale.SetActive(false);
            Gold.SetActive(false);
            Player.InputDisabled = false;
        }
    }

    public enum EnumCurrentMerchantMenu {
        none,
        buy,
        sell,
        deals,
        repair
    }
}
