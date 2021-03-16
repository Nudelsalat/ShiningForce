using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "PartyMember", menuName = "Character/new PartyMember")]
public class PartyMember : Character {
    public bool partyLeader;
    public bool activeParty = true;
}