using Assets.Scripts.Dialog;
using UnityEngine;

public class QuestionFollowUpEventHolder : AbstractDialogHolder {
    [SerializeField]
    public QuestionFollowUpEvent FollowUpEventQuestion;

    void Start() {
    }

    // Start is called before the first frame update
    public override void TriggerDialogue() {
        DialogManager.Instance.StartDialogue(FollowUpEventQuestion);
    }
}
