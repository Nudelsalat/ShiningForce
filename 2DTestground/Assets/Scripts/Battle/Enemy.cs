using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class Enemy : MonoBehaviour {
        public Character Character;

        private Character _character;
        private Animator _animator;

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

        void OnTriggerEnter2D(Collider2D collider) {
            if (collider.gameObject.tag.Equals("Player")) {
                Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                QuickInfoUi.Instance.ShowQuickInfo(_character);
            }
        }
        void OnTriggerExit2D(Collider2D collider) {
            if (collider.gameObject.tag.Equals("Player")) {
                Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                QuickInfoUi.Instance.CloseQuickInfo();
            }
        }
    }
}
