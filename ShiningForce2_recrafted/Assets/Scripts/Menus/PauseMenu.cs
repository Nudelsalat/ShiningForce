using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.Menus.MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class PauseMenu : MonoBehaviour {

        public Button PrimaryButton;
        public GameObject MainMenu;
        public GameObject YesNo;
        public GameObject LoadGame;
        public GameObject Controls;


        public static PauseMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }
            this.gameObject.SetActive(false);
        }

        private void OnEnable() {
            StartCoroutine(WaitForButton());
        }

        void Start() {
            PrimaryButton.Select();
            MainMenu.SetActive(true);
            YesNo.SetActive(false);
            LoadGame.SetActive(false);
            Controls.SetActive(false);
        }

        IEnumerator WaitForButton() {
            yield return null;
            Start();
        }
    }
}