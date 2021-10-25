using System;
using UnityEngine;

[Serializable]
public class QuestionFollowUpEvent : Question {
    public MonoBehaviour FollowUpEventForYes;
    public MonoBehaviour FollowUpEventForNo;
}

