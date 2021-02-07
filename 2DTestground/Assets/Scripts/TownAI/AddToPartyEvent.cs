using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class AddToPartyEvent : MonoBehaviour {
    public PartyMember partyMemberToAdd;
    void EventTrigger() {
        var inventory = partyMemberToAdd.PartyMemberInventory;
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i] != null) {
                partyMemberToAdd.PartyMemberInventory[i] = Instantiate(inventory[i]);
            }
        }
        var partyMemberAdded = Inventory.Instance.AddPartyMember(partyMemberToAdd);
        StartCoroutine(AddPartyMember());
    }

    IEnumerator AddPartyMember() {
        Player.InputDisabledInEvent = true;
        FindObjectOfType<DialogManager>().StartDialogue(new Dialogue() {
            Name = "Event",
            Sentences = new List<string>() { partyMemberToAdd.Name + " joined the Shining Force!" }
        });
        AudioClip audio = Resources.Load<AudioClip>("ShiningForce/sounds/victory");
        AudioSource.PlayClipAtPoint(audio, transform.position);
        yield return new WaitForSecondsRealtime(audio.length-3);
        Player.InputDisabledInEvent = false;
    }
}
