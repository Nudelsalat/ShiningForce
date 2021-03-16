using System;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.GameData {
    [Serializable]
    public class SerializableGameItem {
        public string ResourcePath;
        public bool IsEquipped = false;
        public DirectionType PositionInInventory = DirectionType.none;

        public SerializableGameItem(GameItem gameItem) {
            PositionInInventory = gameItem.PositionInInventory;
            ResourcePath = gameItem.GetResourcePath();
            if (gameItem is Equipment equipment) {
                IsEquipped = equipment.IsEquipped;
            }
        }

        public GameItem GetGameItem() {
            var gameItem = Object.Instantiate(Resources.Load<GameItem>(this.ResourcePath));
            gameItem.PositionInInventory = this.PositionInInventory;
            if (gameItem is Equipment equipment) {
                equipment.IsEquipped = this.IsEquipped;
            }

            return gameItem;
        }
    }
}
