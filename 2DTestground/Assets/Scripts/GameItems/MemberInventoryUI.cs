using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;

public class MemberInventoryUI : MonoBehaviour {
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;

    private Text _titleText;
    private Sprite _blankSprite;
    private GameObject[] _itemList;
    private GameItem[] _gameItemList;
    private GameObject _currentSelectedItem;
    private GameItem _currentSelectedGameItem;

    private Color _redisch = new Color(1f, 0.6f, 0.6f);

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
        _gameItemList = gameItems;
        _titleText.text = "- ITEMS -";
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

    public void LoadMemberMagic(GameItem[] gameItems) {
        _titleText.text = "- MAGIC -";
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
        selectedGameObject.transform.GetComponent<Image>().color = _redisch;
        selectedGameObject.transform.Find("ItemName").GetComponent<Text>().color = _redisch;
        _currentSelectedGameItem = selectedItem;
    }
}

