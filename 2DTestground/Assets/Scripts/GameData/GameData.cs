using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Characters;
using UnityEngine;

[Serializable]
public class GameData {
    public float[] Position;
    public string SceneName;
    public List<SerializableGameItem> Backpack;
    public List<SerializableCharacter> Party;
    public int Gold;
    public List<SerializableGameItem> DealsList;


    public GameData(Player player, string sceneName) {
        Position = new float[3];
        Position[0] = player.transform.position.x;
        Position[1] = player.transform.position.y;
        Position[2] = player.transform.position.z;
        SceneName = sceneName;
        var inventory = Inventory.Instance;
        var backpack = inventory.GetBackPack();
        var serializableBackpack = new List<SerializableGameItem>();
        foreach (var gameItem in backpack) {
            serializableBackpack.Add(new SerializableGameItem(gameItem));
        }
        Backpack = serializableBackpack.ToList();

        var deals = inventory.GetDeals();
        var serializableDeals = new List<SerializableGameItem>();
        foreach (var gameItem in deals) {
            serializableDeals.Add(new SerializableGameItem(gameItem));
        }
        DealsList = serializableDeals.ToList();

        var party = inventory.GetParty();
        var serializableParty = new List<SerializableCharacter>();
        foreach (var member in party) {
            serializableParty.Add(new SerializableCharacter(member));
        }
        Party = serializableParty;

        Gold = inventory.GetGold();
    }
}
