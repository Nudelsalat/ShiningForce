using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.MainMenu {
    class NewGameButton : MonoBehaviour {
        public Text InputText;

        public void NewGame() {
            var fileName = InputText.text;
            if (string.IsNullOrEmpty(fileName)) {
                fileName = "Bowie";
            }

            var inventor = Inventory.Instance;
            inventor.SetSaveFileName(fileName);
            inventor.GetPartyMemberByEnum(EnumCharacterType.Bowie).Name = fileName;
            var warpGameObject = new GameObject();
            warpGameObject.AddComponent<WarpToScene>();
            var warp = warpGameObject.GetComponent<WarpToScene>();
            warp.sceneToWarpTo = Constants.FirstLevelName;
            warp.DoWarp();
        }
    }
}
