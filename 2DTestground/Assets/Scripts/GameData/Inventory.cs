using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;


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
            id = 0,
            name = "Bowie",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/bowie"),
            partyLeader = true,
            activeParty = true,
            classType = ClassType.SDMN,
            characterType = CharacterType.bowie,
            charStats = new CharacterStatistics() {
                level = 1,
                exp = 0,
                maxHp = 12,
                currentHp = 12,
                maxMp = 8,
                currentMp = 8,
                attack = 6,
                defense = 4,
                agility = 4,
                movement = 6
            }
        };
        var sarah = new PartyMember() {
            id = 2,
            name = "Sarah",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/sarah"),
            partyLeader = false,
            activeParty = true,
            classType = ClassType.PRST,
            characterType = CharacterType.sarah,
            charStats = new CharacterStatistics() {
                level = 1,
                exp = 0,
                maxHp = 12,
                currentHp = 12,
                maxMp = 8,
                currentMp = 8,
                attack = 6,
                defense = 4,
                agility = 4,
                movement = 6
            }
        };
        var jaha = new PartyMember() {
            id = 3,
            name = "JAHA",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/jaha"),
            partyLeader = false,
            activeParty = true,
            classType = ClassType.KNTE,
            characterType = CharacterType.jaha,
            charStats = new CharacterStatistics() {
                level = 1,
                exp = 0,
                maxHp = 122,
                currentHp = 122,
                maxMp = 18,
                currentMp = 18,
                attack = 16,
                defense = 14,
                agility = 14,
                movement = 6
            }
        };
        var seppPartyMember = new PartyMember() {
            id = 4,
            name = "sepp",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/gerhalt"),
            partyLeader = false,
            activeParty = false,
            classType = ClassType.WARR,
            characterType = CharacterType.jaha,
            charStats = new CharacterStatistics() {
                level = 2,
                exp = 0,
                maxHp = 22,
                currentHp = 22,
                maxMp = 18,
                currentMp = 8,
                attack = 6,
                defense = 4,
                agility = 4,
                movement = 6
            }
        };
        var karl = new PartyMember() {
            id = 5,
            name = "KAZIN",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/kazin"),
            partyLeader = false,
            activeParty = false,
            classType = ClassType.SDMN,
            characterType = CharacterType.bowie,
            charStats = new CharacterStatistics() {
                level = 2,
                exp = 0,
                maxHp = 242,
                currentHp = 22,
                maxMp = 18,
                currentMp = 8,
                attack = 6,
                defense = 14,
                agility = 4,
                movement = 16
            }
        };
        var luke = new PartyMember() {
            id = 6,
            name = "lUkE",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/luke"),
            partyLeader = false,
            activeParty = true,
            classType = ClassType.SDMN,
            characterType = CharacterType.sarah,
            charStats = new CharacterStatistics() {
                level = 2,
                exp = 0,
                maxHp = 242,
                currentHp = 22,
                maxMp = 18,
                currentMp = 8,
                attack = 6,
                defense = 14,
                agility = 4,
                movement = 16
            }
        };
        var may = new PartyMember() {
            id = 7,
            name = "May",
            partyMemberInventory = new GameItem[4],
            portraitSprite = Resources.Load<Sprite>("ShiningForce/Images/face/may"),
            partyLeader = false,
            activeParty = true,
            classType = ClassType.SDMN,
            characterType = CharacterType.jaha,
            charStats = new CharacterStatistics() {
                level = 2,
                exp = 0,
                maxHp = 242,
                currentHp = 22,
                maxMp = 18,
                currentMp = 8,
                attack = 6,
                defense = 14,
                agility = 4,
                movement = 16
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
                return $"{gameItem.itemName} added to {partyMember.name}!";
            }
        }

        Backpack.Add(gameItem);

        return $"{gameItem.itemName} added to Backpack!";
    }

    public bool AddPartyMember(PartyMember partyMember) {
        var alreadyExists = Party.Any(member => member.id == partyMember.id);
        if (alreadyExists) {
            Debug.LogError("PartyMember with ID " + partyMember.id + " already exists!\n Oider, you fucked up...");
            return false;
        }
        // active party full?
        partyMember.activeParty = Party.Select(x => x.activeParty).Count() < 6;

        Party.Add(partyMember);
        
        return true;
    }
    public bool TryAddGameItemToPartyMember(PartyMember partyMember, GameItem item) {
        for(int i = 0; i < 4; i++) {
            if (partyMember.partyMemberInventory[i] == null) {
                partyMember.partyMemberInventory[i] = item;
                return true;
            }
        }
        return false;
    }

    public PartyMember GetPartyMemberById(int id) {
        return Party.First(x => x.id == id);
    }

    public List<PartyMember> GetParty() {
        return Party;
    }

    public string GetPartyLeaderName() {
        return Party.First(x => x.partyLeader == true).name;
    }
    public string GetPartyMemberNameByEnum(CharacterType characterType) {
        var partyMember = Party.FirstOrDefault(x => x.characterType == characterType);
        if (partyMember == null) {
            return "PartyMemberDoesNotExist";
        }
        else {
            return partyMember.name;
        }

    }
}

