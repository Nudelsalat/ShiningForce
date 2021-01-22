using System;
using UnityEngine;

[Serializable]
public class GameData {
    public float[] position;


    public GameData(PlayerMovement player) {
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
    }

}
