using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveLoadGame {
    
    public static void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/gameData.dat", FileMode.Create);
        Debug.Log(Application.persistentDataPath);
        var player = GameObject.Find("Player");
        GameData data = new GameData(player.GetComponent<Player>());

        bf.Serialize(file, data);
        file.Close();
    }

    public static GameData Load() {
        string path = Application.persistentDataPath + "/gameData.dat";

        if (File.Exists(path)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameData.dat", FileMode.Open);

            GameData data = bf.Deserialize(file) as GameData;
            file.Close();

            return data;
        }
        else {
            Debug.LogError("SaveFile not found in " + path);
            return null;
        }
    }
}
