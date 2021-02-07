using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class PartyMember : Character {
    public bool partyLeader;
    public bool activeParty = true;
}