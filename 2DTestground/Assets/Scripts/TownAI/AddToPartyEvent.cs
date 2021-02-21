using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class AddToPartyEvent : MonoBehaviour {
    public PartyMember partyMemberToAdd;
    void EventTrigger() {
        var inventory = partyMemberToAdd.CharacterInventory;
        var magic = partyMemberToAdd.Magic;
        for (var i = 0; i < inventory.Length; i++) {
            if (inventory[i] != null) {
                partyMemberToAdd.CharacterInventory[i] = Instantiate(inventory[i]);
            }
        }
        for (var i = 0; i < magic.Length; i++) {
            if (magic[i] != null) {
                partyMemberToAdd.Magic[i] = Instantiate(magic[i]);
            }
        }
        var partyMemberAdded = Inventory.Instance.AddPartyMember(partyMemberToAdd);
        StartCoroutine(AddPartyMember());
    }

    IEnumerator AddPartyMember() {
        Player.InputDisabledInEvent = true;
        FindObjectOfType<DialogManager>().StartDialogue(new Dialogue() {
            Name = "Event",
            Sentences = new List<string>() { partyMemberToAdd.Name.AddColor(Constants.Orange) + " joined the Shining Force!" }
        });
        AudioClip audio = Resources.Load<AudioClip>("ShiningForce/sounds/victory");
        AudioSource.PlayClipAtPoint(audio, transform.position);
        yield return new WaitForSecondsRealtime(audio.length-5);
        Player.InputDisabledInEvent = false;
    }
}
