using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class PartyMember {
    public int id;
    public string name;
    public bool partyLeader;
    public bool activeParty = true;
    public Sprite portraitSprite;
    public GameItem[] partyMemberInventory = new GameItem[4];

    public ClassType classType = ClassType.SDMN;
    public CharacterType characterType;
    public CharacterStatistics charStats;
    public StatusEffect statusEffects = StatusEffect.none;


    public void RemoveItem(GameItem item) {
        partyMemberInventory[(int)item.positionInInventory] = Object.Instantiate(Resources.Load<GameItem>(Constants.PathEmptyItem));
    }
}



public enum CharacterType {
    bowie,
    chester,
    sarah,
    jaha
}

[Flags] public enum StatusEffect {
    none = 0,
    poisoned = 1,
    muddled = 2,
    stunned = 4,
    dead = 8,
}

public static class EnumerationExtensions {

    public static bool Has<T>(this System.Enum type, T value) {
        try {
            return (((int)(object)type & (int)(object)value) == (int)(object)value);
        } catch {
            return false;
        }
    }

    public static bool Is<T>(this System.Enum type, T value) {
        try {
            return (int)(object)type == (int)(object)value;
        } catch {
            return false;
        }
    }


    public static T Add<T>(this System.Enum type, T value) {
        try {
            return (T)(object)(((int)(object)type | (int)(object)value));
        } catch (Exception ex) {
            throw new ArgumentException(
                string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                ), ex);
        }
    }


    public static T Remove<T>(this System.Enum type, T value) {
        try {
            return (T)(object)(((int)(object)type & ~(int)(object)value));
        } catch (Exception ex) {
            throw new ArgumentException(
                string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                ), ex);
        }
    }

}

