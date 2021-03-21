using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class Unit : MonoBehaviour {
        public Character Character;

        private Character _character;
        private Animator _animator;
        private BattleController _battleController;
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
        }

        void Start() {
            _battleController = BattleController.Instance;
        }

        public Animator GetAnimator() {
            return _animator;
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
    }
}
