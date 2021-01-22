using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : MonoBehaviour {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public GameItem gameItem;
    public bool DespawnAfterUser = false;

    private bool _isInSpace = false;

    // Start is called before the first frame update
    public void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(Dialogue);
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }

        // add into inventory here.
    }

    void Update() {
        if (_isInSpace && Input.GetButtonDown("Interact") && !PlayerMovement.IsInDialogue) {
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
