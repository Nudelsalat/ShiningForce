using UnityEngine;

public class DialogHolder : AbstractDialogHolder {

    public Dialogue Dialogue;

    // Start is called before the first frame update
    public override void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(Dialogue);
    }
}
