using UnityEngine;

public class QuestionFollowUpEventHolder : AbstractDialogHolder {
    [SerializeField]
    public QuestionFollowUpEvent FollowUpEventQuestion;

    // Start is called before the first frame update
    public override void TriggerDialogue() {
        FindObjectOfType<DialogManager>().StartDialogue(FollowUpEventQuestion);
    }
}
