using System;
using System.Collections;
using System.Linq;
using Assets.Scripts.EditorScripts;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
            _character = Instantiate(Character);
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            FixGridPosition();
            DoColorSwap();

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
            DoColorSwap();
        }

        protected void Start() {
            _battleController = BattleController.Instance;
            _quickInfoCurrentUnit = QuickInfoUi.Instance;
            _audioManager = AudioManager.Instance;

            var delay = Random.Range(0, 2f);
            StartCoroutine(DelayedAnimation(delay));
        }

        private void FixGridPosition() {
            // fix gridPosition
            var position = transform.position;
            var x = position.x >= 0 ? (int) position.x + 0.5f : (int)position.x - 0.5f;
            var y = position.y >= 0 ? (int) position.y + 0.75f : (int)position.y - 0.25f;
            transform.position = new Vector3(x, y);

        }

        private void DoColorSwap() {
            var texture2D = GetFieldAnimationTexture2D();
            if (!texture2D) {
                return;
            }
            var _swapShader = Shader.Find("Custom/SwapTwo");

            var _newMat = new Material(_swapShader);
            _spriteRenderer.sharedMaterial = _newMat;
            _spriteRenderer.sharedMaterial.SetTexture("_MainTex2", texture2D);

        }

        public Texture2D GetFieldAnimationTexture2D() {
            if (_character.ColorPaletteFieldAnimation == null) {
                return null;
            }
            var colorPalette = _character.ColorPaletteFieldAnimation;
            var skinId = _character.SkinId;
            if (colorPalette == null || colorPalette.height - 1 <= skinId) {
                return null;
            }
            return PaletteSwapNoShader.CopyTexture2D(_spriteRenderer.sprite.texture, colorPalette, skinId);
        }

        public Character GetCharacter() {
            return _character;
        }

        public void SetCharacter(Character character) {
            _character = character;
            _animator.runtimeAnimatorController = _character.AnimatorSprite;
            var delay = Random.Range(0, 2f);
            StartCoroutine(DelayedAnimation(delay));
        }

        public void SetHackCharacter(Character character) {
            _character = character;
        }

        public void SetAnimatorDirection(DirectionType direction) {
            _animator.SetInteger("moveDirection", (int) direction);
        }
        public void SetAnimatorSpeed(int speed) {
            _animator.speed = speed;
        }

        public void SetUnitFlicker() {
            StartCoroutine("FlickerAnimator");
        }

        public void ClearUnitFlicker() {
            StopCoroutine("FlickerAnimator");
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
                if (collider.CompareTag("Player")) {
                    Debug.Log($"Current HP: " + _character.CharStats.CurrentHp);
                    _quickInfoCurrentUnit.ShowQuickInfo(_character);
                }
            }
        }
        void OnTriggerExit2D(Collider2D collider) {
            if (_battleController.GetCurrentState() == EnumBattleState.freeCursor) {
                if (collider.CompareTag("Player")) {
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
            _audioManager.Play(Constants.SfxExplosion, false, _audioManager.GetSFXVolume());
            var explosion = Instantiate(Resources.Load<GameObject>(Constants.PrefabExplosion), transform);
            _spriteRenderer.color = Constants.Invisible;
            yield return new WaitForSeconds(explosion.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);

            Destroy(explosion);
            Destroy(gameObject);
        }


        // The delay coroutine
        IEnumerator DelayedAnimation(float randomDelay) {
            yield return new WaitForSeconds(randomDelay);
            _animator.SetInteger("moveDirection", 0);
            yield return null;
            _animator.SetInteger("moveDirection", 2);
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
