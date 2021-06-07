﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.MainMenu {
    class MainMenuButtonSelector : MonoBehaviour {

        public Button NewGame;
        public Button LoadGame;

        void Start() {
            NewGame.Select();
            string path = Application.persistentDataPath + "/";
            var listOfFiles = Directory.GetFiles(path);
            if (listOfFiles.Any(x => x.ToLower().EndsWith(".dat"))) {
                LoadGame.Select();
            }
        }
    }
}
