using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;

public class MemberInventoryUI : MonoBehaviour {
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;
    public GameObject CurrentSelectedItem;

    private Text _titleText;
    private Sprite _blankSprite;
    private GameObject[] _itemList;
    private GameObject _currentSelectedItem;
    private GameItem[] _gameItemList;
    private GameItem _currentSelectedGameItem;
    private PartyMember _partyMember;
    private bool _isEquipment;
    
    public static MemberInventoryUI Instance;
    
    void Awake() {
        if (Instance != null) {
            Debug.LogWarning("More than once Instance of MemberInventoryUI found.");
        } else {
            Instance = this;
        }

        _currentSelectedItem = TopItem;

        _itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };

        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
        _titleText = transform.Find("Title").GetComponent<Text>();

    }

    public void LoadMemberInventory(GameItem[] gameItems) {
        CurrentSelectedItem.transform.Find("Equipped").GetComponent<Image>().color = Constants.Invisible;
        CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>().text = "";
        _isEquipment = false;
        _partyMember = null;
        _gameItemList = gameItems;
        _titleText.text = "-ITEMS-";
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i].ItemSprite;
            _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;

            _gameItemList[i].positionInInventory = (DirectionType) i;

            _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i].itemName;

            if (gameItems[i] is Equipment equipment && equipment.IsEquipped) {
                _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Visible;
            } else {
                _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            }
        }
    }
    public void LoadMemberEquipmentInventory(PartyMember member) {
        _currentSelectedItem.transform.Find("ItemName").gameObject.SetActive(true);
        _partyMember = member;
        _isEquipment = true;
        _gameItemList = member?.PartyMemberInventory;
        _titleText.text = "-EQUIPMENT-";

        for (int i = 0; i < _gameItemList.Length; i++) {
            var itemSprite = _gameItemList[i].ItemSprite;
            _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
            _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
        }

        SelectObject(_currentSelectedGameItem != null ? _currentSelectedGameItem.positionInInventory : 0);
    }

    public void LoadMemberMagic(GameItem[] gameItems) {
        _titleText.text = "-MAGIC-";
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i]?.ItemSprite;
            if (itemSprite != null) {
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite;
            } else {
                _itemList[i].gameObject.GetComponent<Image>().sprite = _blankSprite;
            }

            _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i]?.itemName;
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

    public void UnselectObject() {
        _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
        _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
    }

    public GameItem GetSelectedGameItem() {
        return _currentSelectedGameItem;
    }

    private void SetCurrentSelectedItem(GameObject selectedGameObject, GameItem selectedItem) {
        if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
            _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
        }
        _currentSelectedItem = selectedGameObject;
        selectedGameObject.transform.GetComponent<Image>().color = Constants.Redish;
        selectedGameObject.transform.Find("ItemName").GetComponent<Text>().color = _isEquipment ? Constants.Visible : Constants.Redish;
        _currentSelectedGameItem = selectedItem;

        if (_isEquipment) {
            LoadEquipmentStats();
        }
    }

    private void LoadEquipmentStats() {
        Equipment newEquipment = null;
        Equipment oldEquipment = null;
        if (_currentSelectedGameItem is Equipment equipment) {
            newEquipment = equipment;
            oldEquipment = _partyMember.GetCurrentEquipment(equipment.EquipmentType);

            var text = CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>();
            text.text = _currentSelectedGameItem.itemName;
            text.color = Constants.Visible;


            CurrentSelectedItem.transform.Find("Equipped").gameObject.GetComponent<Image>().color =
                equipment.IsEquipped ? Constants.Visible : Constants.Invisible;
            
        } else {
            CurrentSelectedItem.transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            var text = CurrentSelectedItem.transform.Find("ItemName").GetComponent<Text>();
            text.text = "Select Equipment";
            text.color = Color.red;
        }

        _itemList[0].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
            "ATTACK" + _partyMember.CharStats.CalculateNewAttack(newEquipment, oldEquipment).ToString().PadLeft(6);

        _itemList[1].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
            "DEFENSE" + _partyMember.CharStats.CalculateNewDefense(newEquipment, oldEquipment).ToString().PadLeft(5);

        _itemList[2].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
            "AGILITY" + _partyMember.CharStats.CalculateNewAgility(newEquipment, oldEquipment).ToString().PadLeft(5);

        _itemList[3].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
            "MOVEMENT" + _partyMember.CharStats.CalculateNewMovement(newEquipment, oldEquipment).ToString().PadLeft(4);

    }
}

