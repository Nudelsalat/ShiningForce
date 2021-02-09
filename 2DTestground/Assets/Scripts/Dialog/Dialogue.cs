using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialogue {
    public string Name;
    public Sprite Portrait;
    [TextArea(3,10)]
    public List<string> Sentences;
    public MonoBehaviour FollowUpEvent;
}

