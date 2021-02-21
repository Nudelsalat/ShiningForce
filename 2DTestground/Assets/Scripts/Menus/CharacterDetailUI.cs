using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
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
    private Inventory _inventory;

    public static CharacterDetailUI Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
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

    }

    void Start() {
        _inventory = Inventory.Instance;
    }

    public void LoadCharacterDetails(Character character) {
        MemberInfo.transform.Find("Name").GetComponent<Text>().text = character.Name;
        MemberInfo.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), character.ClassType);
        MemberInfo.transform.Find("Level").GetComponent<Text>().text = "LEVEL " + character.CharStats.Level;
        
        StopAllCoroutines();
        StartCoroutine(CycleThroughStatusEffects(character));

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

        var spriteAnimator = KillsNGold.transform.Find("KillsDefeats/Sprite").GetComponent<Animator>();
        spriteAnimator.runtimeAnimatorController = character.AnimatorSprite;
        spriteAnimator.SetInteger("moveDirection", 2);

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
            firstTextItem.color = Constants.Orange;
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

        if (charMagic == null || charMagic.All(x => x == null)) {
            foreach (var magic in _magicList) {
                magic.gameObject.GetComponent<Image>().sprite = _blankSprite;
                magic.transform.Find("SpellName").gameObject.GetComponent<Text>().text = "";
            }
            firstTextMagic.text = "Nothing";
            firstTextMagic.color = Constants.Orange;
        } else {
            firstTextMagic.color = Color.white;
            for (int i = 0; i < charMagic.Length; i++) {
                if (charMagic[i] != null) {
                    var currentMagic = charMagic[i];
                    var spellSprite = currentMagic.SpellSprite;
                    _magicList[i].gameObject.GetComponent<Image>().sprite =
                        (spellSprite != null && currentMagic.CurrentLevel != 0) ? spellSprite : _blankSprite;
                    _magicList[i].transform.Find("SpellName").gameObject.GetComponent<Text>().text =
                        currentMagic.SpellName;

                    for (int j = 1; j <= 4; j++) {
                        var spawnPoint = _magicList[i].transform.Find("SpellLevel/" + j.ToString());

                        if (currentMagic.CurrentLevel >= j) {
                            var item = Resources.Load(Constants.HealthBar) as GameObject;
                            var pos = new Vector3(0, 0, spawnPoint.transform.position.z);
                            var spawnedItem = Instantiate(item, pos, spawnPoint.transform.rotation);
                            spawnedItem.transform.SetParent(spawnPoint.transform, false);
                        }
                        else {
                            foreach (Transform child in spawnPoint.transform) {
                                Destroy(child.gameObject);
                            }
                        }
                    }

                } else {
                    _magicList[i].GetComponent<Image>().sprite = _blankSprite;
                    _magicList[i].transform.Find("SpellName").gameObject.GetComponent<Text>().text = "";
                    _magicList[i].transform.Find("SpellLevel").gameObject.GetComponent<Text>().text = "";
                }
            }
        }
    }
    private List<Sprite> SetAllStatusEffectsAsSprites(EnumStatusEffect statusEffectsAsEnum) {
        var spriteAtlas = Resources.Load<SpriteAtlas>(Constants.SpriteAtlasStatusEffects);
        var spriteList = new List<Sprite>();
        if (statusEffectsAsEnum == EnumStatusEffect.none) {
            spriteList.Add(spriteAtlas.GetSprite(Enum.GetName(typeof(EnumStatusEffect), EnumStatusEffect.none)));
            return spriteList;
        } else if (statusEffectsAsEnum.HasFlag(EnumStatusEffect.dead)) {
            spriteList.Add(spriteAtlas.GetSprite(Enum.GetName(typeof(EnumStatusEffect), EnumStatusEffect.dead)));
            return spriteList;
        }

        // Get all names of set Enum from statusEffectsAsEnum
        var stringList = Enum.GetValues(typeof(EnumStatusEffect)).Cast<EnumStatusEffect>()
            .Where(x => statusEffectsAsEnum.HasFlag(x)).Select(x => Enum.GetName(typeof(EnumStatusEffect), x))
            .ToList();

        foreach (var spriteName in stringList) {
            if (spriteName.Equals("none")) {
                continue;
            }
            spriteList.Add(spriteAtlas.GetSprite(spriteName));
        }

        return spriteList;
    }

    IEnumerator CycleThroughStatusEffects(Character character) {
        var spriteList = SetAllStatusEffectsAsSprites(character.StatusEffects);
        do {
            foreach (var sprite in spriteList) {
                MemberInfo.transform.Find("StatusEffect").GetComponent<Image>().sprite = sprite;
                yield return new WaitForSeconds(1f);
            }
        } while (true);
    }
}

