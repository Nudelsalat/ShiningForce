using System.Collections.Generic;
using Assets.Scripts.GameData.Characters;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    class MemberDialogueUpdateTrigger : MonoBehaviour, IEventTrigger {
        public List<Tuple<EnumCharacterType, List<string>>> DialogueList = new List<Tuple<EnumCharacterType, List<string>>>();
        public MonoBehaviour FollowUpEvent;

        void Start() {
            var memberDialogueStorage = MemberDialogueStorage.Instance;
            if (!memberDialogueStorage.TryGetDialogueForMember(EnumCharacterType.Bowie, out var dialogue)) {
                memberDialogueStorage.UpdateDialogueForMultiple(DialogueList);
            }
        }

        public void EventTrigger() {
            var memberDialogueStorage = MemberDialogueStorage.Instance;
            memberDialogueStorage.UpdateDialogueForMultiple(DialogueList);
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
