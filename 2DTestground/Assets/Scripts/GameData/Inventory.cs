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
    private List<PartyMember> ActiveParty = new List<PartyMember>();

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
        };
        AddPartyMember(bowie);
    }
    

    public string AddGameItem(GameItem gameItem) {
        foreach (var partyMember in ActiveParty) {
            if (TryAddGameItemToPartyMember(partyMember, gameItem)) {
                return $"{gameItem.itemName} added to {partyMember.name}!";
            }
        }

        Backpack.Add(gameItem);

        return $"{gameItem.itemName} added to Backpack!";
    }

    public void AddPartyMember(PartyMember partyMember) {
        var alreadyExists = Party.Any(member => member.id == partyMember.id);
        if (alreadyExists) {
            Debug.LogError("PartyMember with ID " + partyMember.id + "already exists!\n Oider, you fucked up...");
            return;
        }

        Party.Add(partyMember);

        if (Party.Count() <= 12) {
            ActiveParty.Add(partyMember);
        }
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

}

