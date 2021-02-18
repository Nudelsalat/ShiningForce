using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts.GlobalObjectScripts;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public sealed class Inventory {
    
    private List<GameItem> Backpack = new List<GameItem>();
    private List<PartyMember> Party = new List<PartyMember>();

    private int _gold;

    private static readonly Lazy<Inventory> lazy = new Lazy<Inventory>(() => new Inventory());
    public static Inventory Instance {
        get {
            return lazy.Value;
        }
    }
    private Inventory() {
        var chesterAnimator = Resources.Load<AnimatorController>(Constants.AnimationsFieldChester);
        var bowieAnimator = Resources.Load<AnimatorController>(Constants.AnimationsFieldBowie);
        var merchantAnimator = Resources.Load<AnimatorController>(Constants.AnimationsFieldMerchant);
        var bowie = new PartyMember() {
            Id = 0,
            Name = "Bowie",
            CharacterInventory = new GameItem[] {
                Object.Instantiate(Resources.Load<GameItem>(Constants.EquipmentWunderWaffe)),
                Object.Instantiate(Resources.Load<Equipment>(Constants.EquipmentWoodenSword)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.EquipmentPowerRings)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.EquipmentQuickRing)),
            },
            Magic = new Magic[] {
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEgress)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty)),
            },
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/bowie"),
            partyLeader = true,
            activeParty = true,
            AnimatorSprite = bowieAnimator,
            ClassType = EnumClassType.SDMN,
            CharacterType = EnumCharacterType.bowie,
            CharStats = new CharacterStatistics() {
                Level = 1,
                Exp = 0,
                MaxHp = 12,
                CurrentHp = 12,
                MaxMp = 8,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(4),
                Agility = new Stat(4),
                Movement = new Stat(6)
            }
        };
        var wunderWaffe = (Equipment) bowie.CharacterInventory[1];
        bowie.CharStats.Equip(wunderWaffe);

        var sarah = new PartyMember() {
            Id = 2,
            Name = "Sarah",
            CharacterInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/sarah"),
            partyLeader = false,
            activeParty = true,
            AnimatorSprite = chesterAnimator,
            ClassType = EnumClassType.PRST,
            CharacterType = EnumCharacterType.sarah,
            CharStats = new CharacterStatistics() {
                Level = 1,
                Exp = 0,
                MaxHp = 12,
                CurrentHp = 12,
                MaxMp = 8,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(4),
                Agility = new Stat(4),
                Movement = new Stat(6)
            }
        };
        var jaha = new PartyMember() {
            Id = 3,
            Name = "JAHA",
            CharacterInventory = new GameItem[] {
                Object.Instantiate(Resources.Load<GameItem>(Constants.ItemMedicalHerb)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.ItemHealingSeed)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.ItemAntidote)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.ItemFairyTear)),
            },
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/jaha"),
            partyLeader = false,
            activeParty = true,
            AnimatorSprite = merchantAnimator,
            ClassType = EnumClassType.KNTE,
            CharacterType = EnumCharacterType.jaha,
            StatusEffects = EnumStatusEffect.poisoned,
            CharStats = new CharacterStatistics() {
                Level = 1,
                Exp = 0,
                MaxHp = 122,
                CurrentHp = 95,
                MaxMp = 18,
                CurrentMp = 18,
                Attack = new Stat(16),
                Defense = new Stat(14),
                Agility = new Stat(14),
                Movement = new Stat(6)
            }
        };
        var seppPartyMember = new PartyMember() {
            Id = 4,
            Name = "sepp",
            CharacterInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/gerhalt"),
            partyLeader = false,
            activeParty = false,
            AnimatorSprite = chesterAnimator,
            ClassType = EnumClassType.WARR,
            CharacterType = EnumCharacterType.jaha,
            CharStats = new CharacterStatistics() {
                Level = 2,
                Exp = 0,
                MaxHp = 22,
                CurrentHp = 22,
                MaxMp = 18,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(4),
                Agility = new Stat(4),
                Movement = new Stat(6)
            }
        };
        var karl = new PartyMember() {
            Id = 5,
            Name = "KAZIN",
            CharacterInventory = new GameItem[4],
            Magic = new Magic[] {
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicBlaze)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEgress)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty)),
                Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty)),
            },
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/kazin"),
            partyLeader = false,
            activeParty = false,
            ClassType = EnumClassType.SDMN,
            AnimatorSprite = bowieAnimator,
            CharacterType = EnumCharacterType.bowie,
            CharStats = new CharacterStatistics() {
                Level = 2,
                Exp = 0,
                MaxHp = 242,
                CurrentHp = 22,
                MaxMp = 18,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(14),
                Agility = new Stat(4),
                Movement = new Stat(16)
            }
        };
        karl.Magic[1].CurrentLevel = 0;
        var luke = new PartyMember() {
            Id = 6,
            Name = "lUkE",
            CharacterInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/luke"),
            partyLeader = false,
            activeParty = true,
            AnimatorSprite = merchantAnimator,
            ClassType = EnumClassType.SDMN,
            CharacterType = EnumCharacterType.sarah,
            CharStats = new CharacterStatistics() {
                Level = 2,
                Exp = 0,
                MaxHp = 242,
                CurrentHp = 22,
                MaxMp = 18,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(14),
                Agility = new Stat(4),
                Movement = new Stat(16)
            }
        };
        var may = new PartyMember() {
            Id = 7,
            Name = "May",
            CharacterInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/may"),
            partyLeader = false,
            activeParty = true,
            AnimatorSprite = bowieAnimator,
            ClassType = EnumClassType.SDMN,
            CharacterType = EnumCharacterType.jaha,
            CharStats = new CharacterStatistics() {
                Level = 2,
                Exp = 0,
                MaxHp = 242,
                CurrentHp = 22,
                MaxMp = 18,
                CurrentMp = 8,
                Attack = new Stat(6),
                Defense = new Stat(14),
                Agility = new Stat(4),
                Movement = new Stat(16)
            }
        };
        AddPartyMember(bowie);
        AddPartyMember(sarah);
        AddPartyMember(jaha);
        AddPartyMember(seppPartyMember);
        AddPartyMember(karl);
        AddPartyMember(luke);
        AddPartyMember(may);
    }

    public void AddGold(int addGold) {
        _gold += addGold;
    }
    public void RemoveGold(int lostGold) {
        _gold -= lostGold;
    }
    public int GetGold() {
        return _gold;
    }

    public string AddGameItem(GameItem gameItem) {
        if (gameItem == null) {
            return "But nothing is inside.";
        }
        foreach (var partyMember in Party) {
            if (TryAddGameItemToPartyMember(partyMember, gameItem)) {
                return $"{gameItem.itemName.AddColor(Color.green)} added to {partyMember.Name.AddColor(Constants.Redish)}!";
            }
        }

        Backpack.Add(gameItem);

        return $"{gameItem.itemName} added to Backpack!";
    }

    public bool AddPartyMember(PartyMember partyMember) {
        var alreadyExists = Party.Any(member => member.Id == partyMember.Id);
        if (alreadyExists) {
            Debug.LogError("PartyMember with ID " + partyMember.Id + " already exists!\n Oider, you fucked up...");
            return false;
        }
        // active party full?
        partyMember.activeParty = Party.Select(x => x.activeParty).Count() < 6;
        InitializeInventoryAndMagic(partyMember);

        Party.Add(partyMember);
        
        return true;
    }
    public bool TryAddGameItemToPartyMember(PartyMember partyMember, GameItem item) {
        for(int i = 0; i < 4; i++) {
            if (!partyMember.CharacterInventory[i].IsSet()) {
                item.positionInInventory = (DirectionType) i;
                partyMember.CharacterInventory[i] = item;
                return true;
            }
        }
        return false;
    }

    public PartyMember GetPartyMemberById(int id) {
        return Party.First(x => x.Id == id);
    }

    public List<PartyMember> GetParty() {
        return Party;
    }

    public string GetPartyLeaderName() {
        return Party.First(x => x.partyLeader == true).Name;
    }
    public string GetPartyMemberNameByEnum(EnumCharacterType enumCharacterType) {
        var partyMember = Party.FirstOrDefault(x => x.CharacterType == enumCharacterType);
        if (partyMember == null) {
            return "PartyMemberDoesNotExist";
        }
        else {
            return partyMember.Name;
        }
    }

    public void SwapItems(PartyMember firstMember, PartyMember secondMember, GameItem firstItem, GameItem secondItem) {
        if (firstItem is Equipment firstEquipment) {
            firstMember.CharStats.UnEquip(firstEquipment);
        }
        if (secondItem is Equipment secondEquipment) {
            secondMember.CharStats.UnEquip(secondEquipment);
        }

        var tempDirection = firstItem.positionInInventory;
        firstItem.positionInInventory = secondItem.positionInInventory;
        secondItem.positionInInventory = tempDirection;

        secondMember.CharacterInventory[(int) firstItem.positionInInventory] = firstItem;
        firstMember.CharacterInventory[(int) secondItem.positionInInventory] = secondItem;
        
    }

    private void InitializeInventoryAndMagic(PartyMember member) {
        for (int i = 0; i < member.CharacterInventory.Length; i++) {
            if (member.CharacterInventory[i] == null) {
                var gameItem = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
                member.CharacterInventory[i] = gameItem;
            }
        }

        for (int i = 0; i < member.Magic.Length; i++) {
            if (member.Magic[i] == null) {
                var gameItem = Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty));
                member.Magic[i] = gameItem;
            }
        }
    }
}

