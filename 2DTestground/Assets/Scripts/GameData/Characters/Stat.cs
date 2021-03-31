using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class Stat {
    [SerializeField]
    private int _baseValue;
    private List<int> _modifiers = new List<int>();

    public Stat(int baseValue) {
        _baseValue = baseValue;
    }

    public int GetBaseValue() {
        return _baseValue;
    }

    public int GetModifiedValue() {
        var modifiedValue = _baseValue;
        _modifiers.ForEach(x => modifiedValue += x);
        return modifiedValue;
    }

    public void AddModifiert(int modifier) {
        _modifiers.Add(modifier);
    }

    public void RemoveModifier(int modifier) {
        _modifiers.Remove(modifier);
    }

    public void ClearModifier() {
        _modifiers.Clear();
    }

}

