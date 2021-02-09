using System;
using UnityEngine;

[Serializable]
public class QuestionCallback : Question {
    public Action<bool> OnAnswerAction;
}

