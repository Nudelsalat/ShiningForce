using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public sealed class Inventory {
    
    private List<GameItem> Backpack = new List<GameItem>();
    private List<PartyMember> Party = new List<PartyMember>();

    private int _gold;
    private List<GameItem> _dealsList = new List<GameItem>();

    private static readonly Lazy<Inventory> lazy = new Lazy<Inventory>(() => new Inventory());
    public static Inventory Instance {
        get {
            return lazy.Value;
        }
    }
    private Inventory() {
        AddGold(7500);
        _dealsList.Add(Object.Instantiate(Resources.Load<GameItem>(Constants.EquipmentWunderWaffe)));

        var bowie = Object.Instantiate(Resources.Load<PartyMember>(Constants.CharacterBowie));
        var sarah = Object.Instantiate(Resources.Load<PartyMember>(Constants.CharacterSarah));
        sarah.StatusEffects = sarah.StatusEffects.Add(EnumStatusEffect.dead);
        sarah.StatusEffects = sarah.StatusEffects.Add(EnumStatusEffect.poisoned);

        AddPartyMember(bowie);
        AddPartyMember(sarah);
        /*
        AddPartyMember(sarah);
        AddPartyMember(jaha);
        AddPartyMember(seppPartyMember);
        AddPartyMember(karl);
        AddPartyMember(luke);
        AddPartyMember(may);
        */
    }

    public void Initialize(List<PartyMember> party, List<GameItem> backPack, List<GameItem> dealsList, int gold) {
        Party = party;
        Backpack = backPack;
        _dealsList = dealsList;
        _gold = gold;
    }

    public void TestSetDealslist(List<GameItem> gameItems) {
        _dealsList = gameItems;
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

    public List<GameItem> GetDeals() {
        return _dealsList;
    }

    public void RemoveFromDeals(GameItem item) {
        _dealsList.Remove(item);
    }

    public void AddToDeals(GameItem item) {
        _dealsList.Add(item);
    }

    public string AddGameItem(GameItem gameItem) {
        if (gameItem == null) {
            return "But nothing is inside.";
        }
        foreach (var partyMember in Party) {
            if (TryAddGameItemToPartyMember(partyMember, gameItem)) {
                return $"{gameItem.ItemName.AddColor(Color.green)} added to {partyMember.Name.AddColor(Constants.Orange)}!";
            }
        }

        Backpack.Add(gameItem);

        return $"{gameItem.ItemName.AddColor(Color.green)} added to Backpack!";
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
            if (!partyMember.GetInventory()[i].IsSet()) {
                item.PositionInInventory = (DirectionType) i;
                partyMember.GetInventory()[i] = item;
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
        return Party.First(x => x.partyLeader == true).Name.AddColor(Constants.Orange);
    }
    public string GetPartyMemberNameByEnum(EnumCharacterType enumCharacterType) {
        var partyMember = Party.FirstOrDefault(x => x.CharacterType == enumCharacterType);
        if (partyMember == null) {
            return "PartyMemberDoesNotExist";
        }
        else {
            return partyMember.Name.AddColor(Constants.Orange);
        }
    }

    public void SwapItems(PartyMember firstMember, PartyMember secondMember, GameItem firstItem, GameItem secondItem) {
        if (firstItem is Equipment firstEquipment) {
            firstMember.CharStats.UnEquip(firstEquipment);
        }
        if (secondItem is Equipment secondEquipment) {
            secondMember.CharStats.UnEquip(secondEquipment);
        }

        var tempDirection = firstItem.PositionInInventory;
        firstItem.PositionInInventory = secondItem.PositionInInventory;
        secondItem.PositionInInventory = tempDirection;

        secondMember.GetInventory()[(int) firstItem.PositionInInventory] = firstItem;
        firstMember.GetInventory()[(int) secondItem.PositionInInventory] = secondItem;
    }

    public List<GameItem> GetBackPack() {
        return Backpack;
    }

    private void InitializeInventoryAndMagic(PartyMember member) {
        var memberInventory = member.GetInventory();
        var memberMagic = member.GetMagic();
        for (int i = 0; i < member.GetInventory().Length; i++) {
            if (memberInventory[i] == null) {
                var gameItem = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
                memberInventory[i] = gameItem;
            }
        }

        for (int i = 0; i < memberMagic.Length; i++) {
            if (memberMagic[i] == null) {
                var magic = Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty));
                memberMagic[i] = magic;
            }
        }
    }
}

