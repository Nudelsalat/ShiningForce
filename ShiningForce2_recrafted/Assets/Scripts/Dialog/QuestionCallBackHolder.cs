using Assets.Scripts.Dialog;
using UnityEngine;

public class QuestionCallBackHolder : AbstractDialogHolder {

    public QuestionCallback CallbackQuestion;

    // Start is called before the first frame update
    public override void TriggerDialogue() {
        DialogManager.Instance.StartDialogue(CallbackQuestion);
    }
}
