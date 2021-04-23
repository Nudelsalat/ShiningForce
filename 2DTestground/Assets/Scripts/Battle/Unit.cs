using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class Unit : MonoBehaviour {
        [SerializeField]
        private Character Character;
        
        protected Character _character;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private BattleController _battleController;
        private QuickInfoUi _quickInfoCurrentUnit;
        private AudioManager _audioManager;

#if UNITY_EDITOR
        void OnValidate() {
            FixGridPosition();

            var sprite = GetComponent<SpriteRenderer>();
            var clip = Character.AnimatorSprite.animationClips[0];
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).FirstOrDefault();
            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding).FirstOrDefault();
            sprite.sprite = (Sprite)keyframes.value;
        }
#endif

        protected void Awake() {
            _character = Instantiate(Character);
            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = _character.AnimatorSprite;
            _animator.SetInteger("moveDirection", 2);
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            FixGridPosition();
        }

        protected void Start() {
            _battleController = BattleController.Instance;
            _quickInfoCurrentUnit = QuickInfoUi.Instance;
            _audioManager = AudioManager.Instance;
        }

        private void FixGridPosition() {
            // fix gridPosition
            var position = transform.position;
            var x = position.x >= 0 ? (int) position.x + 0.5f : (int)position.x - 0.5f;
            var y = position.y >= 0 ? (int) position.y + 0.75f : (int)position.y - 0.25f;
            transform.position = new Vector3(x, y);
           
        }

        public Character GetCharacter() {
            return _character;
        }

        public void SetCharacter(Character character) {
            _character = character;
            _animator.runtimeAnimatorController = _character.AnimatorSprite;
        }

        public void SetAnimatorDirection(DirectionType direction) {
            _animator.SetInteger("moveDirection", (int) direction);
        }
        public void SetAnimatorSpeed(int speed) {
            _animator.speed = speed;
        }

        public void SetUnitFlicker() {
            StartCoroutine(FlickerAnimator());
        }
        public void ClearUnitFlicker() {
            StopAllCoroutines();
            _spriteRenderer.color = Constants.Visible;
        }

        public void KillUnit() {
            gameObject.layer = 0;
            gameObject.tag = "Untagged";
            _character.StatusEffects = EnumStatusEffect.dead;
            StartCoroutine(DestroyAnimation());
        }

        void OnTriggerEnter2D(Collider2D collider) {
            if (_battleController.GetCurrentState() == EnumBattleState.freeCursor) {
                if (collider.gameObject.tag.Equals("Player")) {
                    Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                    _quickInfoCurrentUnit.ShowQuickInfo(_character);
                }
            }
        }
        void OnTriggerExit2D(Collider2D collider) {
            if (_battleController.GetCurrentState() == EnumBattleState.freeCursor) {
                if (collider.gameObject.tag.Equals("Player")) {
                    Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                    _quickInfoCurrentUnit.CloseQuickInfo();
                }
            }
        }

        IEnumerator DestroyAnimation() {
            for (int j = 0; j < 4; j++) {
                _animator.SetInteger("moveDirection", 0);
                yield return new WaitForSeconds(0.1f);
                _animator.SetInteger("moveDirection", 1);
                yield return new WaitForSeconds(0.1f);
                _animator.SetInteger("moveDirection", 2);
                yield return new WaitForSeconds(0.1f);
                _animator.SetInteger("moveDirection", 3);
                yield return new WaitForSeconds(0.1f);
                j++;
            }
            _audioManager.PlaySFX(Constants.SfxExplosion);
            var explosion = Instantiate(Resources.Load<GameObject>(Constants.PrefabExplosion), transform);
            _spriteRenderer.color = Constants.Invisible;
            yield return new WaitForSeconds(explosion.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);
            
            Destroy(explosion);
            Destroy(gameObject);
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
