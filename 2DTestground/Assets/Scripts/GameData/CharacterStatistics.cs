using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterStatistics {
    public int level = 1;
    public int exp = 0;
    public int maxHp = 0;
    public int currentHp = 0;
    public int maxMp = 0;
    public int currentMp = 0;
    public int attack = 0;
    public int defense = 0;
    public int agility = 0;
    public int movement = 0;

    public CharacterStatistics() {
        level = 1;
        exp = 0;
        maxHp = currentHp = 10;
        maxMp = currentMp = 0;
        attack = 5;
        defense = 5;
        agility = 5;
        movement = 5;
    }
}
