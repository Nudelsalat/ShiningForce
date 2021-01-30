using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
}

public enum CharacterType {
    bowie,
    chester,
    sarah,
    jaha
}

