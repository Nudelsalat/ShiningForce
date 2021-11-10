using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    public class AddToPartyEvent : MonoBehaviour, IEventTrigger {
        public PartyMember PartyMemberToAdd;
        public MonoBehaviour FollowUpEvent;

        public void EventTrigger() {
            var partyMember = Instantiate(PartyMemberToAdd);
            Inventory.Instance.AddPartyMember(partyMember);
            StartCoroutine(AddPartyMember());
        }

        IEnumerator AddPartyMember() {
            Player.Instance.SetInputDisabledInEvent();
            DialogManager.Instance.StartDialogue(new Dialogue() {
                Name = "Event",
                Sentences = new List<string>() {
                    PartyMemberToAdd.Name.AddColor(Constants.Orange) + " joined the Shining Force!"
                },
                VoicePitch = EnumVoicePitch.none
            });
            AudioManager.Instance.PauseAll();
            var duration = AudioManager.Instance.Play(Constants.SoundNewPartyMember, false);
            yield return new WaitForSecondsRealtime(duration - 3);
            Player.Instance.UnsetInputDisabledInEvent();
            AudioManager.Instance.UnPauseAll();
            while (Player.IsInDialogue) {
                yield return null;
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
