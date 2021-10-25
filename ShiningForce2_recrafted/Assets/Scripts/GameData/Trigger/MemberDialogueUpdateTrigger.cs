using System.Collections.Generic;
using Assets.Scripts.GameData.Characters;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    class MemberDialogueUpdateTrigger : MonoBehaviour, IEventTrigger {
        public List<Tuple<EnumCharacterType, List<string>>> DialogueList = new List<Tuple<EnumCharacterType, List<string>>>();

        void Start() {
            var memberDialogueStorage = MemberDialogueStorage.Instance;
            if (!memberDialogueStorage.TryGetDialogueForMember(EnumCharacterType.bowie, out var dialogue)) {
                memberDialogueStorage.UpdateDialogueForMultiple(DialogueList);
            }
        }

        public void EventTrigger() {
            var memberDialogueStorage = MemberDialogueStorage.Instance;
            memberDialogueStorage.UpdateDialogueForMultiple(DialogueList);
        }
    }
}
