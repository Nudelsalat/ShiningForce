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
    private GameObject _currentSelectedItem;

    private Color _redisch = new Color(1f, 0.6f, 0.6f);

    public static MemberInventoryUI Instance;
    
    void Awake() {
        if (Instance != null) {
            Debug.LogWarning("More than once Instance of MemberInventoryUI found.");
        } else {
            Instance = this;
        }

        _itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };
        _currentSelectedItem = TopItem;

        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
        _titleText = transform.Find("Title").GetComponent<Text>();

    }

    public void LoadMemberInventory(GameItem[] gameItems) {
        _titleText.text = "- ITEMS -";
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i]?.ItemSprite;
            if (itemSprite != null) {
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite;
            }
            else {
                _itemList[i].gameObject.GetComponent<Image>().sprite = _blankSprite;
            }

            _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i]?.itemName;
            _itemList[i].transform.GetComponent<GameItem>().SetGameItem(gameItems[i], (DirectionType)i); 
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
                SetCurrentSelectedItem(TopItem);
                break;
            case DirectionType.left:
                SetCurrentSelectedItem(LeftItem);
                break;
            case DirectionType.down:
                SetCurrentSelectedItem(BottomItem);
                break;
            case DirectionType.right:
                SetCurrentSelectedItem(RightItem);
                break;
        }
    }

    public void UnselectObject() {
        _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
        _currentSelectedItem.transform.GetChild(0).GetComponent<Text>().color = Color.white;
    }

    public GameItem GetSelectedGameItem() {
        return _currentSelectedItem.GetComponent<GameItem>();
    }

    private void SetCurrentSelectedItem(GameObject selectedGameObject) {
        if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
            _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            _currentSelectedItem.transform.GetChild(0).GetComponent<Text>().color = Color.white;
        }
        selectedGameObject.transform.GetComponent<Image>().color = _redisch;
        selectedGameObject.transform.GetChild(0).GetComponent<Text>().color = _redisch;
        _currentSelectedItem = selectedGameObject;
    }
}

