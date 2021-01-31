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

    private static Text _titleText;
    private static Sprite _blankSprite;
    private static GameObject[] _itemList;

    void Awake() {
        _itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };
        _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
        _titleText = transform.Find("Title").GetComponent<Text>();
    }

    public static void LoadMemberInventory(GameItem[] gameItems) {
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
        //TODO
    }
}

