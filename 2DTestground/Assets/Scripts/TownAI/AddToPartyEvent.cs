using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToPartyEvent : MonoBehaviour {
    public PartyMember partyMemberToAdd;
    void EventTrigger() {
        var partyMember = Instantiate(partyMemberToAdd);
        Inventory.Instance.AddPartyMember(partyMember);
        StartCoroutine(AddPartyMember());
    }

    IEnumerator AddPartyMember() {
        Player.InputDisabledInEvent = true;
        DialogManager.Instance.StartDialogue(new Dialogue() {
            Name = "Event",
            Sentences = new List<string>() {
                partyMemberToAdd.Name.AddColor(Constants.Orange) + " joined the Shining Force!"
            },
            VoicePitch = EnumVoicePitch.none
        });
        AudioManager.Instance.PauseAll();
        var duration = AudioManager.Instance.Play(Constants.SoundNewPartyMember, false);
        yield return new WaitForSecondsRealtime(duration - 3);
        Player.InputDisabledInEvent = false;
        AudioManager.Instance.UnPauseAll();
    }
}
