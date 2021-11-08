using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData.Magic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.GameData.Characters {
    [Serializable]
    public class SerializableCharacter {
        public string ResourcePath;
        public string Name;
        public bool IsLeader;
        public bool IsActivePartyMember;
        public bool IsPromoted;
        public EnumClassType ClassType;
        public CharacterStatistics CharStats;
        public EnumStatusEffect StatusEffects;
        public SerializableGameItem[] Inventory = new SerializableGameItem[4];
        public SerializableMagic[] Magic = new SerializableMagic[4];

        public SerializableCharacter(Character character) {
            ResourcePath = string.Concat("SharedObjects/Characters/Monsters/", 
                character.name.Replace("(Clone)", ""));
            if (character is PartyMember partyMember) {
                IsLeader = partyMember.partyLeader;
                IsActivePartyMember = partyMember.activeParty;
                ResourcePath = string.Concat("SharedObjects/Characters/Heroes/",
                    character.name.Replace("(Clone)", ""));
            }

            Name = character.Name;
            IsPromoted = character.IsPromoted;
            ClassType = character.ClassType;
            CharStats = character.CharStats;
            StatusEffects = character.StatusEffects;

            var charInventory = character.GetInventory();
            for (int i = 0; i < charInventory.Length; i++) {
                Inventory[i] = new SerializableGameItem(charInventory[i]);
            }

            var charMagic = character.GetMagic();
            for (int i = 0; i < charMagic.Length; i++) {
                Magic[i] = new SerializableMagic(charMagic[i]);
            }
        }

        public Character GetCharacter() {
            var character = Object.Instantiate(Resources.Load<Character>(this.ResourcePath));

            if (character is PartyMember partyMember) {
                partyMember.partyLeader = this.IsLeader;
                partyMember.activeParty = this.IsActivePartyMember;
            }

            character.Name = string.IsNullOrEmpty(Name) ? character.Name : Name;
            character.IsPromoted = this.IsPromoted;
            character.CharStats = this.CharStats;
            character.ClassType = this.ClassType;
            character.StatusEffects = this.StatusEffects;

            var inventory = character.GetInventory();
            for (int i = 0; i < this.Inventory.Length; i++) {
                inventory[i] = Inventory[i].GetGameItem();
            }

            var magic = character.GetMagic();
            for (int i = 0; i < this.Inventory.Length; i++) {
                magic[i] = Magic[i].GetMagic();
            }

            return character;
        }
    }
}
