using UnityEngine;

public abstract class AbstractDialogHolder : MonoBehaviour {
    public bool DespawnAfterUser = false;

    private bool _isInSpace;

    public void EventTrigger() {
        TriggerDialogue();
    }

    // Start is called before the first frame update
    public abstract void TriggerDialogue();

    public virtual void Update() {
        if (_isInSpace && Input.GetButtonUp("Interact") && !Player.IsInDialogue 
            && !Player.InputDisabledInDialogue && !Player.InputDisabled) {
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
