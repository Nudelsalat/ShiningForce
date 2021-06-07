using System;
using System.Collections.Generic;
using Assets.Scripts.GameData.Chests;

namespace Assets.Scripts.GameData.Characters {
    [Serializable]
    public class MemberDialogueStorage {
        private Dictionary<EnumCharacterType, List<string>> _dict = new Dictionary<EnumCharacterType, List<string>>();

        private static readonly Lazy<MemberDialogueStorage>
            lazy =
                new Lazy<MemberDialogueStorage>
                    (() => new MemberDialogueStorage());

        public static MemberDialogueStorage Instance { get { return lazy.Value; } }

        private MemberDialogueStorage() {
        }

        public bool TryGetDialogueForMember(EnumCharacterType character, out List<string> dialogue) {
            return _dict.TryGetValue(character, out dialogue);
        }

        public void UpdateDialogueForMember(EnumCharacterType character, List<string> dialogue) {
            if (_dict.ContainsKey(character)) {
                _dict[character] = dialogue;
            } else {
                _dict.Add(character, dialogue);
            }
        }

        public void UpdateDialogueForMultiple(List<Tuple<EnumCharacterType, List<string>>> list) {
            foreach (var entry in list) {
                UpdateDialogueForMember(entry.Item1, entry.Item2);
            }
        }

        public void LoadData(MemberDialogueStorage dataToLoad) {
            _dict = dataToLoad._dict;
        }
    }
}
