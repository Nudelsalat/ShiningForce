using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.HelperScripts;
using Assets.Scripts.Menus;
using UnityEngine;
using UnityEngine.U2D;
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
    private Animator _animatorOverViewUi;
    private bool _showUi;
    
    public static MemberOverviewUI Instance;

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
        _magicList = new GameObject[] {
            TopMagic, LeftMagic, BottomMagic, RightMagic
        };
        _blankSprite = Resources.Load<Sprite>(Constants.SpriteEmptyItem);

        _animatorOverViewUi = transform.GetComponent<Animator>();
    }


    public void LoadMemberInventory(Character character) {
        OpenMemberOverViewUi();

        MemberInfo.transform.Find("Name").GetComponent<Text>().text = character.Name;
        MemberInfo.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), character.ClassType);
        MemberInfo.transform.Find("Level").GetComponent<Text>().text = "LEVEL " + character.CharStats.Level;

        StatusEffectDisplayer.Instance.SetAllStatusEffectsOfCharacter(
            MemberInfo.transform.Find("StatusEffect").gameObject, character.StatusEffects);

        var charInventory = character.CharacterInventory;
        var charMagic = character.Magic;
        var firstTextItem = _itemList[0].transform.Find("ItemName").gameObject.GetComponent<Text>();
        var firstTextMagic = _magicList[0].transform.Find("SpellName").gameObject.GetComponent<Text>();

        if (charInventory == null || charInventory.All(x => x.EnumItemType == EnumItemType.none)) {
            foreach (var item in _itemList) {
                item.gameObject.GetComponent<Image>().sprite = _blankSprite;
                item.transform.Find("ItemName").gameObject.GetComponent<Text>().text = "";
                item.transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            }
            firstTextItem.text = "Nothing";
            firstTextItem.color = Constants.Orange;
        } else {
            firstTextItem.color = Color.white;
            for (int i = 0; i < charInventory.Length; i++) {
                var itemSprite = charInventory[i].ItemSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
                _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = charInventory[i].ItemName;

                if (charInventory[i] is Equipment equipment && equipment.IsEquipped) {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Visible;
                }
                else {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
                }
            }
        }

        if (charMagic == null || charMagic.All(x => x.IsEmpty())) {
            foreach (var magic in _magicList) {
                magic.gameObject.GetComponent<Image>().sprite = _blankSprite;
                magic.transform.Find("SpellName").gameObject.GetComponent<Text>().text = "";
                magic.transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = "";
            }
            firstTextMagic.text = "Nothing";
            firstTextMagic.color = Constants.Orange;
        } else {
            firstTextMagic.color = Color.white;
            for (int i = 0; i < charMagic.Length; i++) {
                if (!charMagic[i].IsEmpty()) {
                    var spellSprite = charMagic[i].SpellSprite;
                    _magicList[i].gameObject.GetComponent<Image>().sprite =
                        (spellSprite != null && charMagic[i].CurrentLevel != 0) ? spellSprite : _blankSprite;
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

    private void OpenMemberOverViewUi() {
        transform.gameObject.SetActive(true);
        _showUi = true;
        _animatorOverViewUi.SetBool("inventoryIsOpen", true);
    }

    public void CloseMemberOverviewUi() {
        _showUi = false;
        _animatorOverViewUi.SetBool("inventoryIsOpen", false);

    }
    IEnumerator WaitForTenthASecond() {
        yield return new WaitForSeconds(0.1f);
        if (!_showUi) {
            transform.gameObject.SetActive(false);
        }
    }
}

