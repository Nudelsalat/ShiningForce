using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue {

    public string Name;
    public Sprite Portrait;
    [TextArea(3,10)]
    public string[] Sentences;
}

