using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberInventoryUI : MonoBehaviour {
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;

    private static Sprite BlankSprite;
    private static GameObject[] itemList;

    void Awake() {
        itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };
        BlankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
    }

    public static void LoadMemberInventory(GameItem[] gameItems) {
        for (int i = 0; i < gameItems.Length; i++) {
            var itemSprite = gameItems[i]?.ItemSprite;
            if (itemSprite != null) {
                itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite;
            }
            else {
                itemList[i].gameObject.GetComponent<Image>().sprite = BlankSprite;
            }

            itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i]?.itemName;
        }
    }
}

