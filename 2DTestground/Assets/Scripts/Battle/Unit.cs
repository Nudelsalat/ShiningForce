using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class Unit : MonoBehaviour {
        public Character Character;

        private Character _character;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private BattleController _battleController;
        private bool _isSelected;
#if UNITY_EDITOR
        void OnValidate() {
            var sprite = GetComponent<SpriteRenderer>();
            var clip = Character.AnimatorSprite.animationClips[0];
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).FirstOrDefault();
            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding).FirstOrDefault();
            sprite.sprite = (Sprite)keyframes.value;
        }
#endif

        void Awake() {
            _character = Instantiate(Character);
            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = _character.AnimatorSprite;
            _animator.SetInteger("moveDirection", 2);
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        void Start() {
            _battleController = BattleController.Instance;
        }

        public Animator GetAnimator() {
            return _animator;
        }

        public void SetUnitFlicker() {
            StartCoroutine(FlickerAnimator());
        }
        public void ClearUnitFlicker() {
            StopAllCoroutines();
            _spriteRenderer.color = Constants.Visible;
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (_battleController.GetCurrentState() == EnumBattleState.unitSelected) {
                return;
            }
            if (collider.gameObject.tag.Equals("Player")) {
                Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                QuickInfoUi.Instance.ShowQuickInfo(_character);
            }
        }
        void OnTriggerExit2D(Collider2D collider) {
            if (_battleController.GetCurrentState() == EnumBattleState.unitSelected) {
                return;
            }
            if (collider.gameObject.tag.Equals("Player")) {
                Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                QuickInfoUi.Instance.CloseQuickInfo();
            }
        }

        IEnumerator FlickerAnimator() {
            while (true) {
                _spriteRenderer.color = Constants.Visible;
                yield return new WaitForSeconds(0.4f);
                _spriteRenderer.color = Constants.Invisible;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
