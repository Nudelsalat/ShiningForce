﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class CharacterDetailUI : MonoBehaviour {
    public GameObject TopItem;
    public GameObject LeftItem;
    public GameObject RightItem;
    public GameObject BottomItem;

    public GameObject TopMagic;
    public GameObject LeftMagic;
    public GameObject RightMagic;
    public GameObject BottomMagic;

    public GameObject MemberInfo;
    public GameObject Stats;
    public GameObject KillsNGold;
    
    private Sprite _blankSprite;
    private GameObject[] _itemList;
    private GameObject[] _magicList;
    public Inventory _inventory;

    public static CharacterDetailUI Instance;
    
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

    void Start() {
        _inventory = Inventory.Instance;
    }

    public void LoadCharacterDetails(Character character) {
        MemberInfo.transform.Find("Name").GetComponent<Text>().text = character.Name;
        MemberInfo.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), character.ClassType);
        MemberInfo.transform.Find("Level").GetComponent<Text>().text = "LEVEL " + character.CharStats.Level;

        Stats.transform.Find("HP/Current").GetComponent<Text>().text = character.CharStats.CurrentHp.ToString();
        Stats.transform.Find("MP/Max").GetComponent<Text>().text = character.CharStats.MaxMp.ToString();
        Stats.transform.Find("MP/Current").GetComponent<Text>().text = character.CharStats.CurrentMp.ToString();
        Stats.transform.Find("HP/Max").GetComponent<Text>().text = character.CharStats.MaxHp.ToString();
        Stats.transform.Find("EXP/Value").GetComponent<Text>().text = character.CharStats.Exp.ToString();
        Stats.transform.Find("Level/Value").GetComponent<Text>().text = character.CharStats.Level.ToString();
        Stats.transform.Find("Attack/Value").GetComponent<Text>().text = character.CharStats.Attack.GetModifiedValue().ToString();
        Stats.transform.Find("Defense/Value").GetComponent<Text>().text = character.CharStats.Defense.GetModifiedValue().ToString();
        Stats.transform.Find("Agility/Value").GetComponent<Text>().text = character.CharStats.Agility.GetModifiedValue().ToString();
        Stats.transform.Find("Movement/Value").GetComponent<Text>().text = character.CharStats.Movement.GetModifiedValue().ToString();

        KillsNGold.transform.Find("Gold/GoldText").GetComponent<Text>().text = _inventory.GetGold().ToString();
        KillsNGold.transform.Find("KillsDefeats/Kills/KillCount").GetComponent<Text>().text = character.Kills.ToString();
        KillsNGold.transform.Find("KillsDefeats/Defeated/DefeatCount").GetComponent<Text>().text = character.Defeats.ToString();
        //TODO THIS CANOT WORK, because i need images!
        var SpriteAnimator = KillsNGold.transform.Find("KillsDefeats/Sprite").GetComponent<Animator>();
        SpriteAnimator.runtimeAnimatorController = character.AnimatorSprite;
        SpriteAnimator.SetInteger("moveDirection", 2);

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
                //TODO
                //magic.transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = "";
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
