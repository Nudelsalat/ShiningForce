using System;
using UnityEngine;
using UnityEngine.UI;

public class MemberOverviewUI : MonoBehaviour {
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;

    public GameObject TopMagic;
    public GameObject LeftMagic;
    public GameObject RightMagic;
    public GameObject BottomMagic;

    public GameObject MemberInfo;

    private Sprite _blankSprite;
    private GameObject[] _itemList;
    private GameObject[] _magicList;
    
    public static MemberOverviewUI Instance;
    
    void Awake() {
        if (Instance != null) {
            Debug.LogWarning("More than once Instance of MemberInventoryUI found.");
        } else {
            Instance = this;
        }
        _itemList = new GameObject[] {
            TopItem, LeftItem, BottomItem, RightItem
        };
        _magicList = new GameObject[] {
            TopMagic, LeftMagic, BottomMagic, RightMagic
        };
        _blankSprite = Resources.Load<Sprite>(Constants.PathEmptyItemSprite);

    }

    public void LoadMemberInventory(Character character) {
        MemberInfo.transform.Find("Name").GetComponent<Text>().text = character.Name;
        MemberInfo.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), character.ClassType);
        MemberInfo.transform.Find("Level").GetComponent<Text>().text = "LEVEL " + character.CharStats.Level;

        var charInventory = character.CharacterInventory;
        var charMagic = character.Magic;
        var firstTextItem = _itemList[0].transform.Find("ItemName").gameObject.GetComponent<Text>();
        var firstTextMagic = _magicList[0].transform.Find("SpellName").gameObject.GetComponent<Text>();

        if (charInventory == null) {
            foreach (var item in _itemList) {
                item.gameObject.GetComponent<Image>().sprite = _blankSprite;
                item.transform.Find("ItemName").gameObject.GetComponent<Text>().text = "";
                item.transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            }
            firstTextItem.text = "Nothing";
            firstTextItem.color = Constants.Redish;
        } else {
            firstTextItem.color = Color.white;
            for (int i = 0; i < charInventory.Length; i++) {
                var itemSprite = charInventory[i].ItemSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
                _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = charInventory[i].itemName;

                if (charInventory[i] is Equipment equipment && equipment.IsEquipped) {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Visible;
                }
                else {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
                }
            }
        }

        if (charMagic == null) {
            foreach (var magic in _magicList) {
                magic.gameObject.GetComponent<Image>().sprite = _blankSprite;
                magic.transform.Find("SpellName").gameObject.GetComponent<Text>().text = "";
                magic.transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = "";
            }
            firstTextMagic.text = "Nothing";
            firstTextMagic.color = Constants.Redish;
        } else {
            firstTextMagic.color = Color.white;
            for (int i = 0; i < charMagic.Length; i++) {
                if (charMagic[i] != null) {
                    var spellSprite = charMagic[i].SpellSprite;
                    _magicList[i].gameObject.GetComponent<Image>().sprite =
                        spellSprite != null ? spellSprite : _blankSprite;
                    _magicList[i].transform.Find("SpellName").gameObject.GetComponent<Text>().text =
                        charMagic[i].SpellName;
                    _magicList[i].transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = 
                        "Level " + charMagic[i].CurrentLevel;
                } else {
                    _magicList[i].GetComponent<Image>().sprite = _blankSprite;
                    _magicList[i].transform.Find("SpellName").gameObject.GetComponent<Text>().text = "";
                    _magicList[i].transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = "";
                }
            }
        }
    }
}

