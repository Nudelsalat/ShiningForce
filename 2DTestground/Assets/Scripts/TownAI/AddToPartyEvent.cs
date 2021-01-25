using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class AddToPartyEvent : MonoBehaviour {
    public PartyMember partyMemberToAdd;
    void EventTrigger() {
        var partyMemberAdded = Inventory.Instance.AddPartyMember(partyMemberToAdd);
        StartCoroutine(AddPartyMember());
    }

    IEnumerator AddPartyMember() {
        Player.InputDisabled = true;
        FindObjectOfType<DialogManager>().StartDialogue(new Dialogue() {
            Name = "Event",
            Sentences = new List<string>() { partyMemberToAdd.name + " joined the Shining Force!" }
        });
        AudioClip audio = Resources.Load<AudioClip>("ShiningForce/sounds/victory");
        AudioSource.PlayClipAtPoint(audio, transform.position);
        yield return new WaitForSecondsRealtime(audio.length-3);
        Player.InputDisabled = false;
    }
}
