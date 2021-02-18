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
        if (gameItem != null) {
            var newList = Dialogue.Sentences.Select(x => x.Replace("#ITEMNAME#",
                gameItem.itemName.AddColor(Color.green))).ToList();
            Dialogue.Sentences = newList;
        }
    }

    void Start() {
        inventory = Inventory.Instance;
    }

    void Update() {
        if (_isInSpace && Input.GetButtonUp("Interact") && !Player.InputDisabledInDialogue && !Player.InputDisabled) {
            if (gameItem != null) {
                var addedToWhom = inventory.AddGameItem(Instantiate(gameItem));
                Dialogue.Sentences.Add(addedToWhom);
                TriggerDialogue();
                Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
                RemoveItem();
            } else {
                TriggerDialogue();
            }

            if (DespawnAfterUser) {
                Destroy(gameObject);
            }
        }
    }

    public void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(Dialogue);
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
    }

    public void RemoveItem() {
        gameItem = null;
        audioClip = null;
        Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
        Dialogue.Sentences.Add("Nothing here...");
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
