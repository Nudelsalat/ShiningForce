
using System.Collections;
using System.Linq;
using System.Reflection;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.Menus;
using Assets.Scripts.Menus.Battle;
using UnityEngine;
using UnityEngine.UI;

public class MemberInventoryUI : MonoBehaviour{
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;
    public GameObject StatusEffects;
    public GameObject CurrentSelectedItem;
    public GameObject Gold;

    private Animator _inventoryAnimator;
    private Animator _animatorGold;
    private Inventory _inventory;
    private Text _titleText;
    private Sprite _blankSprite;
    private GameObject[] _itemList;
    private GameObject _currentSelectedItem;
    private GameItem[] _gameItemList;
    private Magic[] _magicList;
    private GameItem _currentSelectedGameItem;
    private Magic _currentSelectedMagic;
    private Character _partyMember;
    private bool _isEquipment;
    private bool _showInventory;
    private QuickStats _quickStats;

    public static MemberInventoryUI Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        _itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };

        _inventory = Inventory.Instance;
        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
        _titleText = transform.Find("MemberInventoryUI/Title").GetComponent<Text>();
        _inventoryAnimator = transform.GetComponent<Animator>();
        _animatorGold = Gold.transform.GetComponent<Animator>();
    }

    void Start() {
        _quickStats = QuickStats.Instance;
        _currentSelectedItem = TopItem;
        transform.gameObject.SetActive(false);
    }

    public void LoadMemberInventory(Character character) {
        Gold.SetActive(true);
        OpenInventory();
        _quickStats.CloseQuickInfo();

        StatusEffectDisplayer.Instance.SetAllStatusEffectsOfCharacter(
            StatusEffects, character.StatusEffects);
        var gameItems = character.GetInventory();

        CurrentSelectedItem.transform.Find("Equipped").GetComponent<Image>().color = Constants.Invisible;
        CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>().text = "";
        _isEquipment = false;
        _partyMember = null;
        _gameItemList = gameItems;
        _titleText.text = "-ITEMS-";
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i].ItemSprite;
            _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;

            _gameItemList[i].PositionInInventory = (DirectionType) i;

            _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i].ItemName;

            if (gameItems[i] is Equipment equipment && equipment.IsEquipped) {
                _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Visible;
            } else {
                _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            }
        }
    }

    public void LoadMemberEquipmentInventory(Character member) {
        OpenInventory();
        _quickStats.CloseQuickInfo();

        StatusEffectDisplayer.Instance.SetAllStatusEffectsOfCharacter(
            StatusEffects, member.StatusEffects);

        _currentSelectedItem.transform.Find("ItemName").gameObject.SetActive(true);
        _partyMember = member;
        _isEquipment = true;
        _gameItemList = member?.GetInventory();
        _titleText.text = "-EQUIPMENT-";

        for (int i = 0; i < _gameItemList.Length; i++) {
            var itemSprite = _gameItemList[i].ItemSprite;
            _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
            _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
        }

        SelectObject(_currentSelectedGameItem != null ? _currentSelectedGameItem.PositionInInventory : 0);
    }

    public void LoadMemberMagic(Character character) {
        OpenInventory();

        StatusEffectDisplayer.Instance.SetAllStatusEffectsOfCharacter(
            StatusEffects, character.StatusEffects);
        var gameItems = character.GetMagic();

        CurrentSelectedItem.transform.Find("Equipped").GetComponent<Image>().color = Constants.Invisible;
        CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>().text = "";
        _isEquipment = false;
        _partyMember = null;
        _magicList = gameItems;
        _titleText.text = "-MAGIC-";
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i].SpellSprite;
            _itemList[i].gameObject.GetComponent<Image>().sprite = 
                (itemSprite != null && gameItems[i].CurrentLevel != 0) ? itemSprite : _blankSprite;

            _magicList[i].PositionInInventory = (DirectionType)i;

            _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i].SpellName;


            _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            
        }
    }

    public void SelectObject(DirectionType direction) {
        switch (direction) {
            case DirectionType.up:
                SetCurrentSelectedItem(TopItem, _gameItemList[0]);
                break;
            case DirectionType.left:
                SetCurrentSelectedItem(LeftItem, _gameItemList[1]);
                break;
            case DirectionType.down:
                SetCurrentSelectedItem(BottomItem, _gameItemList[2]);
                break;
            case DirectionType.right:
                SetCurrentSelectedItem(RightItem, _gameItemList[3]);
                break;
        }
    }


    public void SelectMagic(DirectionType direction) {
        switch (direction) {
            case DirectionType.up:
                SetCurrentSelectedMagic(TopItem, _magicList[0]);
                break;
            case DirectionType.left:
                SetCurrentSelectedMagic(LeftItem, _magicList[1]);
                break;
            case DirectionType.down:
                SetCurrentSelectedMagic(BottomItem, _magicList[2]);
                break;
            case DirectionType.right:
                SetCurrentSelectedMagic(RightItem, _magicList[3]);
                break;
        }
    }

    public void UnselectObject() {
        _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
        _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
    }

    public GameItem GetSelectedGameItem() {
        return _currentSelectedGameItem;
    }
    public Magic GetSelectedMagic() {
        return _currentSelectedMagic;
    }

    public void CloseInventory() {
        _quickStats.CloseQuickInfo();
        _inventoryAnimator.SetBool("inventoryIsOpen", false);
        _showInventory = false;
        CloseGold();
        if (this.isActiveAndEnabled) {
            StartCoroutine(WaitForTenthASecond());
        }
    }

    private void OpenInventory() {
        transform.gameObject.SetActive(true);
        _showInventory = true;
        _inventoryAnimator.SetBool("inventoryIsOpen", true);
        OpenGold();
    }

    private void SetCurrentSelectedItem(GameObject selectedGameObject, GameItem selectedItem) {
        if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
            _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
        }
        _currentSelectedItem = selectedGameObject;
        selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
        selectedGameObject.transform.Find("ItemName").GetComponent<Text>().color = _isEquipment ? Constants.Visible : Constants.Orange;
        _currentSelectedGameItem = selectedItem;

        if (_isEquipment) {
            LoadEquipmentStats();
        } 
    }
    
    private void SetCurrentSelectedMagic(GameObject selectedGameObject, Magic selectedMagic) {
        if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
            _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
        }
        _currentSelectedItem = selectedGameObject;
        selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
        selectedGameObject.transform.Find("ItemName").GetComponent<Text>().color = _isEquipment ? Constants.Visible : Constants.Orange;
        _currentSelectedMagic = selectedMagic;
    }

    private void LoadEquipmentStats() {
        if (_currentSelectedGameItem is Equipment equipment) {

            var text = CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>();
            text.text = _currentSelectedGameItem.ItemName;
            text.color = Constants.Visible;


            CurrentSelectedItem.transform.Find("Equipped").gameObject.GetComponent<Image>().color =
                equipment.IsEquipped ? Constants.Visible : Constants.Invisible;
            if (equipment.EquipmentForClass.Any(x => x == _partyMember.ClassType)) {
                _quickStats.ShowQuickInfo(_partyMember, equipment);
            }
        } else {
            CurrentSelectedItem.transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            var text = CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>();
            text.text = "Select Equipment";
            text.color = Color.red;
            _quickStats.CloseQuickInfo();
        }
    }

    private void OpenGold() {
        _animatorGold.SetBool("isOpen", true);
        Gold.transform.Find("GoldText").GetComponent<Text>().text = _inventory.GetGold().ToString();
    }

    private void CloseGold() {
        _animatorGold.SetBool("isOpen", false);
    }

    IEnumerator WaitForTenthASecond() {
        yield return new WaitForSeconds(0.1f);
        if (!_showInventory) {
            transform.gameObject.SetActive(false);
            if (!_animatorGold.GetBool("isOpen")) {
                Gold.SetActive(false);
            }
        }
    }
}

