using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Battle;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarTracker : MonoBehaviour
{
    private Unit _target;
    private Slider _slider;
    private bool _isActive;
        
    void Awake() {
        _slider = GetComponent<Slider>();
    }

    void FixedUpdate()
    {
        if (_isActive) {
            if (_target == null) {
                Destroy(gameObject);
                return;
            }
            var wantedPos = Camera.main.WorldToScreenPoint(_target.transform.position);
            wantedPos.y += 40;
            transform.position = wantedPos;
            var character = _target.GetCharacter();
            _slider.value = (float) character.CharStats.CurrentHp / (float) character.CharStats.MaxHp();
        }
    }

    public void SetTarget(Unit targetUnit) {
        _target = targetUnit;
        _isActive = true;
    }
}
