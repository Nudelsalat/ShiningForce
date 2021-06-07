using Assets.Scripts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CameraScripts {
    public class MiniMapDot : MonoBehaviour
    {
        private Unit _target;
        private SpriteRenderer _spriteRenderer;
        private Color _color = Color.green;
        private Color _color2 = Color.green;
        private float _time = 0.0f;

        private readonly float _duration = 0.25f;

        void Awake() {
            _spriteRenderer = transform.GetComponent<SpriteRenderer>();
        }
        void FixedUpdate()
        {
            if (_target == null) {
                Destroy(gameObject);
                return;
            }
            while (_time > _duration) {
                _spriteRenderer.color = _spriteRenderer.color == _color ? _color2 : _color;
                transform.position = _target.transform.position;
                _time = 0.0f;
            }
            _time += Time.deltaTime;
        }

        public void SetTargetAndColor(Unit targetUnit, Color color) {
            _target = targetUnit;
            _color = color;
            _color2 = color;
            _color2.a = 0.5f;
        }
    }
}
