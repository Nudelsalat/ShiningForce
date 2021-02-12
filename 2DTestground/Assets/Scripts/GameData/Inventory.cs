using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts.GlobalObjectScripts;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public sealed class Inventory {
    
    private List<GameItem> Backpack = new List<GameItem>();
    private List<PartyMember> Party = new List<PartyMember>();

    private static readonly Lazy<Inventory> lazy = new Lazy<Inventory>(() => new Inventory());
    public static Inventory Instance {
        get {
            return lazy.Value;
        }
    }
    private Inventory() {
        var bowie = new PartyMember() {
            Id = 0,
            Name = "Bowie",
            PartyMemberInventory = new GameItem[] {
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathWunderWaffe)),
                Object.Instantiate(Resources.Load<Equipment>(Constants.PathWoodenSword)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathEmptyItem)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathEmptyItem)),
            },
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/bowie"),
            partyLeader = true,
            activeParty = true,
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
        var wunderWaffe = (Equipment) bowie.PartyMemberInventory[1];
        bowie.CharStats.Equip(wunderWaffe);

        var sarah = new PartyMember() {
            Id = 2,
            Name = "Sarah",
            PartyMemberInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/sarah"),
            partyLeader = false,
            activeParty = true,
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
            PartyMemberInventory = new GameItem[] {
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathMedicalHerb)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathHealingSeed)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathAntidote)),
                Object.Instantiate(Resources.Load<GameItem>(Constants.PathFairyTear)),
            },
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/jaha"),
            partyLeader = false,
            activeParty = true,
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
            PartyMemberInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/gerhalt"),
            partyLeader = false,
            activeParty = false,
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
            PartyMemberInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/kazin"),
            partyLeader = false,
            activeParty = false,
            ClassType = EnumClassType.SDMN,
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
        var luke = new PartyMember() {
            Id = 6,
            Name = "lUkE",
            PartyMemberInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/luke"),
            partyLeader = false,
            activeParty = true,
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
            PartyMemberInventory = new GameItem[4],
            PortraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/may"),
            partyLeader = false,
            activeParty = true,
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
    

    public string AddGameItem(GameItem gameItem) {
        if (gameItem == null) {
            return "But nothing is inside.";
        }
        foreach (var partyMember in Party) {
            if (TryAddGameItemToPartyMember(partyMember, gameItem)) {
                return $"{gameItem.itemName} added to {partyMember.Name}!";
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
        InitializeInventory(partyMember);

        Party.Add(partyMember);
        
        return true;
    }
    public bool TryAddGameItemToPartyMember(PartyMember partyMember, GameItem item) {
        for(int i = 0; i < 4; i++) {
            if (!partyMember.PartyMemberInventory[i].IsSet()) {
                item.positionInInventory = (DirectionType) i;
                partyMember.PartyMemberInventory[i] = item;
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

        secondMember.PartyMemberInventory[(int) firstItem.positionInInventory] = firstItem;
        firstMember.PartyMemberInventory[(int) secondItem.positionInInventory] = secondItem;
        
    }

    private void InitializeInventory(PartyMember member) {
        for (int i = 0; i < member.PartyMemberInventory.Length; i++) {
            if (member.PartyMemberInventory[i] == null) {
                var gameItem = Object.Instantiate(Resources.Load<GameItem>(Constants.PathEmptyItem));
                member.PartyMemberInventory[i] = gameItem;
            }
        }
    }

}

