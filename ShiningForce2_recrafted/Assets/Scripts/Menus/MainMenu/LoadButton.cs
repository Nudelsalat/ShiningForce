using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData.Characters;
using Assets.Scripts.GameData.Chests;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.MainMenu {
    public class LoadButton : MonoBehaviour {

        public void LoadGame() {
            var fileName = transform.Find("Text").GetComponent<Text>().text;
            var data = SaveLoadGame.Load(fileName);
            DoLoadGame(data);
        }
        private void DoLoadGame(GameData.GameData data) {

            SceneManager.LoadScene(data.SceneName, LoadSceneMode.Single);

            TriggerStorage.Instance.LoadData(data.TriggerStorage);
            PickedUpItemStorage.Instance.LoadData(data.PickedUpItemStorage);
            MemberDialogueStorage.Instance.LoadData(data.MemberDialogueStorage);

            Vector3 position = new Vector3(data.Position[0], data.Position[1], data.Position[2]);
            var player = GameObject.Find("Player");
            var movePoint = GameObject.Find("movePoint");
            var mainCamera = GameObject.Find("Main Camera");
            if (player) {
                player.transform.position = position;
            }
            if (movePoint) {
                movePoint.transform.position = position;
            }
            if (mainCamera) {
                mainCamera.transform.position = new Vector3(position.x, position.y, mainCamera.transform.position.z);
            }

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

            Inventory.Instance.Initialize(partyList, backPackList, dealsList, data.Gold, data.SaveFileName);
            var pauseMenu = PauseMenu.Instance;
            if (pauseMenu) {
                pauseMenu.gameObject.SetActive(false);
            }
            Time.timeScale = 1f;
            Player.PlayerIsInMenu = EnumMenuType.none;
        }
    }
}
