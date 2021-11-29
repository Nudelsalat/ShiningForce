using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace Assets.Scripts.Menus.MainMenu {
    class MenuYesNo : MonoBehaviour {

        public Button No;
        public Text Text;
        private string _command;
        
        private void Start() {
            No.Select();
        }

        private void OnEnable() {
            StartCoroutine(WaitForButton());
        }

        public void SetYesCommand(string command) {
            _command = command;
        }

        public void SetLabelText(string text) {
            Text.text = text;
        }

        public void ExecuteCommandEvent() {
            this.Invoke(_command, 0f);
        }

        private void QuitGame() {
            Debug.Log("Quit game event triggered!");
            Application.Quit();
        }

        private void LoadGame() {

        }
        IEnumerator WaitForButton() {
            yield return null;
            Start();
        }
    }
}
