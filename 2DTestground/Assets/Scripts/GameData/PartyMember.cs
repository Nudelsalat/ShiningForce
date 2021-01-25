using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class PartyMember {
    public int id;
    public GameItem[] partyMemberInventory = new GameItem[4];
    public string name;
    public Sprite portraitSprite;
}

