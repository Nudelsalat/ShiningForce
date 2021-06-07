using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts.GameData;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadGame {
    
    public static void Save(string fileName) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/" + fileName + ".dat", FileMode.Create);
        Debug.Log(Application.persistentDataPath);
        var player = GameObject.Find("Player");
        GameData data = new GameData(player.GetComponent<Player>(), SceneManager.GetActiveScene().name);

        bf.Serialize(file, data);
        file.Close();
    }

    public static GameData Load(string fileName) {
        string path = Application.persistentDataPath + "/" + fileName + ".dat";

        if (File.Exists(path)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/" + fileName + ".dat", FileMode.Open);

            GameData data = bf.Deserialize(file) as GameData;
            file.Close();

            return data;
        }
        else {
            Debug.LogError("SaveFile not found in " + path + " with filename: " + fileName);
            return null;
        }
    }
}
