using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

class SaveLoadUI :MonoBehaviour {

    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            SavePlayerPosition();
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            LoadPlayerPosition();
        }
    }

    public void SavePlayerPosition() {
        //SaveLoadGame.Save();
        Debug.Log("Quicksave button disabled!");
    }

    public void LoadPlayerPosition() {
        GameData data = SaveLoadGame.Load();

        SceneManager.LoadScene(data.SceneName, LoadSceneMode.Single);

        PickedUpItemStorage.Instance.LoadData(data.PickedUpItemStorage);

        Vector3 position = new Vector3(data.Position[0], data.Position[1], data.Position[2]);
        var player = GameObject.Find("Player");
        var movePoint = GameObject.Find("movePoint");
        var mainCamera = GameObject.Find("Main Camera");
        player.transform.position = position;
        movePoint.transform.position = position;
        mainCamera.transform.position = new Vector3(position.x, position.y, mainCamera.transform.position.z);

        var partyList = new List<PartyMember>();
        var backPackList = new List<GameItem>();
        var dealsList = new List<GameItem>();

        var partySerialized = data.Party;
        var backPackSerialized = data.DealsList;
        var dealsSerialized = data.DealsList;

        foreach (var serializedPartyMember in partySerialized) {
            partyList.Add((PartyMember)serializedPartyMember.GetCharacter());

        }

        foreach (var serializableGameItem in backPackSerialized) {
            backPackList.Add(serializableGameItem.GetGameItem());
        }

        foreach (var serializableGameItem in dealsSerialized) {
            dealsList.Add(serializableGameItem.GetGameItem());
        }

        Inventory.Instance.Initialize(partyList, backPackList, dealsList, data.Gold);
    }
}

