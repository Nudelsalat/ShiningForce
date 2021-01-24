using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogHolder : MonoBehaviour {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public bool DespawnAfterUser = false;

    private bool _isInSpace = false;
    private bool _isInDialogue = false;

    // Start is called before the first frame update
    public void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(Dialogue);
    }

    void Update() {
        if (_isInSpace && Input.GetButtonDown("Interact") && !Player.IsInDialogue) {
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
