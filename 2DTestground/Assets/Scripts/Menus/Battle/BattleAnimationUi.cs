using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.EditorScripts;
using Assets.Scripts.GameData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Menus.Battle {
    public class BattleAnimationUi : MonoBehaviour {

        private Image _backgroundImage;
        private Image _blackVoid;
        private Image _platformImage;
        private Transform _forceUnitSpawn;
        private Transform _enemyUnitSpawn;
        private Animator _spellAnimator;

        private Animator _targetAnimator;
        private Animator _attackerAnimator;

        private QuickInfoUi _quickInfo;
        private QuickInfoUiTarget _quickInfoUiTarget;

        private GameObject _spawnedAttacker;
        private GameObject _spawnedTarget;
        private Image _attackImage;
        private Image _targetImage;
        private Texture2D _attackerTexture2D;
        private Texture2D _targetTexture2D;

        private bool _isAttackerForceUnit;
        private bool _unitsAreOpponents;

        public static BattleAnimationUi Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }
            _blackVoid = transform.Find("Blackvoid").GetComponent<Image>();
            _backgroundImage = transform.Find("Background").GetComponent<Image>();
            _platformImage = transform.Find("Force/Platform").GetComponent<Image>();
            _forceUnitSpawn = transform.Find("Force/Character").GetComponent<Transform>();
            _enemyUnitSpawn = transform.Find("Enemy/Character").GetComponent<Transform>();
            _spellAnimator = transform.Find("SpellAnimator").GetComponent<Animator>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _quickInfo = QuickInfoUi.Instance;
            _quickInfoUiTarget = QuickInfoUiTarget.Instance;
            gameObject.SetActive(false);
        }

        void LateUpdate() {
            if (_targetTexture2D != null) {
                _targetImage.sprite = Sprite.Create(_targetTexture2D, _targetImage.sprite.rect, _targetImage.sprite.pivot);
            }
            if (_attackerTexture2D != null) {
                _attackImage.sprite = Sprite.Create(_attackerTexture2D, _attackImage.sprite.rect, _attackImage.sprite.pivot);
            }
        }

        public void Load(bool isAttackerForceUnit, bool unitsAreOpponents, Unit attacker, Unit target) {
            gameObject.SetActive(true);
            _spellAnimator.runtimeAnimatorController = null;
            FlipToNormal(_enemyUnitSpawn);
            FlipToNormal(_forceUnitSpawn);
            var targetPrefab = Resources.Load<GameObject>(target.GetCharacter().GetBattleAnimationPath());
            var attackerPrefab = Resources.Load<GameObject>(attacker.GetCharacter().GetBattleAnimationPath());

            _isAttackerForceUnit = isAttackerForceUnit;
            _unitsAreOpponents = unitsAreOpponents;
            _spawnedTarget = Instantiate(targetPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _spawnedAttacker = Instantiate(attackerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _attackImage = _spawnedAttacker.transform.Find("Character").GetComponent<Image>();
            _targetImage = _spawnedTarget.transform.Find("Character").GetComponent<Image>();
            if (isAttackerForceUnit) {
                _quickInfo.ShowQuickInfo(attacker.GetCharacter());
                _quickInfoUiTarget.ShowQuickInfo(target.GetCharacter());
                _spawnedAttacker.transform.SetParent(_forceUnitSpawn, false);
                _spawnedTarget.transform.SetParent(_enemyUnitSpawn, false);
            } else {
                _quickInfo.ShowQuickInfo(target.GetCharacter());
                _quickInfoUiTarget.ShowQuickInfo(attacker.GetCharacter());
                _spawnedTarget.transform.SetParent(_forceUnitSpawn, false);
                _spawnedAttacker.transform.SetParent(_enemyUnitSpawn, false);
            }

            SetWeapon(_spawnedAttacker, attacker.GetCharacter());
            SetWeapon(_spawnedTarget, target.GetCharacter());
            SetTexture2D(_attackImage, attacker.GetCharacter(), out _attackerTexture2D);
            SetTexture2D(_targetImage, target.GetCharacter(), out _targetTexture2D);

            _attackerAnimator = _spawnedAttacker.GetComponent<Animator>();
            _targetAnimator = _spawnedTarget.GetComponent<Animator>();
            _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

            if (!unitsAreOpponents) {
                Flip(isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn);
            }

            StartCoroutine(DelayedUpdate(0.1f));
        }

        public void DoAttackAnimation(bool isDodge, bool isKill, RuntimeAnimatorController spellAnimatorController) {
            if (spellAnimatorController != null) {
                _spellAnimator.runtimeAnimatorController = spellAnimatorController;
                _spellAnimator.SetInteger("spellAnimation", 0);
            }
            _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Attack);
            if (isDodge) {
                _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Dodge);
            } else {
                //TODO Damage wiggle
            }
            StartCoroutine(DelayedUpdate(0.5f));
        }

        public void DoCounterAnimation(bool isDodge, bool isKill) {
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Attack);
            if (isDodge) {
                _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Dodge);
            } else {
                //TODO Damage wiggle
            }
            StartCoroutine(DelayedUpdate(0.5f));
        }

        public void Transition(Unit nextTarget) {
            //TODO transition
            if (_isAttackerForceUnit) {
                foreach (Transform transform in _enemyUnitSpawn) {
                    Destroy(transform.gameObject);
                }
                _quickInfoUiTarget.ShowQuickInfo(nextTarget.GetCharacter());
            } else {
                foreach (Transform transform in _forceUnitSpawn) {
                    Destroy(transform.gameObject);
                }
                _quickInfo.ShowQuickInfo(nextTarget.GetCharacter());
            }
            var targetPrefab = Resources.Load<GameObject>(nextTarget.GetCharacter().GetBattleAnimationPath());
            _spawnedTarget = Instantiate(targetPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            _targetImage = _spawnedTarget.transform.Find("Character").GetComponent<Image>();
            if (_isAttackerForceUnit) {
                _spawnedTarget.transform.SetParent(_enemyUnitSpawn, false);
            } else {
                _spawnedTarget.transform.SetParent(_forceUnitSpawn, false);
            }

            SetWeapon(_spawnedTarget, nextTarget.GetCharacter());
            SetTexture2D(_targetImage, nextTarget.GetCharacter(), out _targetTexture2D);

            _targetAnimator = _spawnedTarget.GetComponent<Animator>();
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

            if (!_unitsAreOpponents) {
                Flip(_isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn);
            }
            StartCoroutine(DelayedUpdate(0.1f));
        }

        public void EndAttackAnimation() {
            foreach (Transform transform in _forceUnitSpawn) {
                Destroy(transform.gameObject);
            }
            foreach (Transform transform in _enemyUnitSpawn) {
                Destroy(transform.gameObject);
            }
            _quickInfo.CloseQuickInfo();
            _quickInfoUiTarget.CloseQuickInfo();
            gameObject.SetActive(false);
        }

        private void SetWeapon(GameObject battleAnimation, Character character) {
            var image = battleAnimation.transform.Find("weapon").GetComponent<Image>();
            if (character.CharacterType == EnumCharacterType.monster) {
                image.enabled = false;
                return;
            }

            Sprite sprite = null;
            var equipment = (Equipment) character.GetInventory().FirstOrDefault(
                x => x.EnumItemType == EnumItemType.equipment
                     && ((Equipment)x).EquipmentType == EnumEquipmentType.weapon
                     && ((Equipment)x).IsEquipped);
            if (equipment) {
                sprite = equipment.BattleSprite;
            }
            if (sprite) {
                image.enabled = true;
                image.sprite = sprite;
                image.SetNativeSize();
                var normalizedPivotX = sprite.pivot.x / sprite.rect.width;
                var normalizedPivotY = sprite.pivot.y / sprite.rect.height;
                image.rectTransform.pivot = new Vector2(normalizedPivotX, normalizedPivotY);
            } else {
                image.enabled = false;
            }
        }

        private void SetTexture2D(Image battleImage, Character character, out Texture2D target) {
            if (character.ColorPaletteBattleAnimation == null) {
                target = null;
                return;
            }
            var colorPalette = character.ColorPaletteBattleAnimation;
            var skinId = character.SkinId;
            if (colorPalette == null || colorPalette.height - 1 <= skinId) {
                target = null;
                return;
            }
            target = PaletteSwapNoShader.CopyTexture2D(battleImage.sprite.texture, colorPalette, skinId);
        }
        
        private void FlipToNormal(Transform transform) {
            var scale = transform.localScale;
            scale.x = Math.Abs(scale.x);
            transform.localScale = scale;
        }
        private void Flip(Transform transform) {
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        IEnumerator DelayedUpdate(float seconds) {
            yield return new WaitForSeconds(seconds);
            _quickInfo.UpdateInfo();
            _quickInfoUiTarget.UpdateInfo();
        }
    }
}
