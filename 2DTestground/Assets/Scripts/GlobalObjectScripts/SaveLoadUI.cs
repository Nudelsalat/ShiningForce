using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
        SaveLoadGame.Save();
    }

    public void LoadPlayerPosition() {
        GameData data = SaveLoadGame.Load();

        Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
        var player = GameObject.Find("Player");
        var movePoint = GameObject.Find("MovePoint");
        var mainCamera = GameObject.Find("MainCamera");
        player.transform.position = movePoint.transform.position = position;
        mainCamera.transform.position = new Vector3(position.x, position.y, mainCamera.transform.position.z);
    }
}

