using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemHolder : MonoBehaviour {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public GameItem gameItem;
    public bool DespawnAfterUser = false;

    private bool _isInSpace = false;
    private Inventory inventory;

    void Awake() {
        inventory = Inventory.Instance;
        var newList = Dialogue.Sentences.Select(x => x.Replace("#ITEMNAME#", gameItem.itemName)).ToList();
        Dialogue.Sentences = newList;
    }

    // Start is called before the first frame update
    public void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(Dialogue);
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }

        Debug.Log("gameItem.itemName");
        // add into inventory here.
    }

    void Update() {
        if (_isInSpace && Input.GetButtonDown("Interact") && !Player.IsInDialogue) {
            var addedToWhom = inventory.AddGameItem(gameItem);
            Dialogue.Sentences.Add(addedToWhom);
            TriggerDialogue();
            if (DespawnAfterUser) {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("InteractionPointer")) {
            _isInSpace = true;
        }
    }
    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("InteractionPointer")) {
            _isInSpace = false;
        }
    }
}
