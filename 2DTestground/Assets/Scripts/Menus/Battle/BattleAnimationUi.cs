using System;
using System.Collections;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.EditorScripts;
using Assets.Scripts.GameData;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Menus.Battle {
    public class BattleAnimationUi : MonoBehaviour {

        public Texture2D DissolveTexture2D;

        private Image _backgroundImage;
        private Image _blackVoid;
        private Transform _forceUnitSpawn;
        private Transform _enemyUnitSpawn;
        private Transform _spellSpawn;

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
        private bool _spellIsTriggered;

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
            _forceUnitSpawn = transform.Find("Force/Character").GetComponent<Transform>();
            _enemyUnitSpawn = transform.Find("Enemy/Character").GetComponent<Transform>();
            _spellSpawn = transform.Find("SpellAnimator").GetComponent<Transform>();
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
            _spellAnimator = null;
            _spellIsTriggered = false;
            FlipToNormal(_enemyUnitSpawn);
            FlipToNormal(_forceUnitSpawn);
            FlipToNormal(_spellSpawn);

            _isAttackerForceUnit = isAttackerForceUnit;
            _unitsAreOpponents = unitsAreOpponents;

            InitializeAnimationObjects(out _spawnedAttacker, out _attackImage, attacker, !_isAttackerForceUnit, true);
            InitializeAnimationObjects(out _spawnedTarget, out _targetImage, target, _isAttackerForceUnit, false);
            
            _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

            if (!unitsAreOpponents) {
                Flip(isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn);
            }

            if (attacker == target) {
                _targetImage.color = Constants.Invisible;
                var image = _spawnedTarget.transform.Find("weapon").GetComponent<Image>();
                image.color = Constants.Invisible;
            }

            StartCoroutine(DelayedUpdate(0.1f));
        }

        public void DoAttackAnimation(bool isDodge, bool isKill, string spellPath, EnumMagicType? magicAttackType) {
            if (!_spellIsTriggered) {
                _spellIsTriggered = true;
                if (!string.IsNullOrEmpty(spellPath)) {
                    var spellPrefab = Resources.Load<GameObject>(spellPath);
                    if (spellPrefab != null) {
                        var spellInstance = Instantiate(spellPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        spellInstance.transform.SetParent(_spellSpawn, false);
                        _spellAnimator = spellInstance.GetComponent<Animator>();
                        if (!_isAttackerForceUnit) {
                            Flip(_spellSpawn);
                            if (_unitsAreOpponents) {
                                spellInstance.transform.Translate(0,-50,0);
                            }
                        }
                    } else {
                        Debug.LogError($"{spellPath} was not a valid prefab path!");
                    }
                }
            }
            _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Attack);
            if (isDodge) {
                _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Dodge);
            }

            var isAttack = magicAttackType == null || magicAttackType == EnumMagicType.Damage;
            if (!isDodge && isAttack) {
                var clip = _attackerAnimator.runtimeAnimatorController.animationClips.SingleOrDefault(x => x.name.Contains("Attack"));
                var length = clip != null ? clip.length : 0.5f;
                StartCoroutine(DelayHitAnimations(isKill, _spawnedTarget, length - 0.15f));
            }
            StartCoroutine(DelayedUpdate(0.5f));
        }

        public void DoCounterAnimation(bool isDodge, bool isKill) {
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Attack);
            _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
            if (isDodge) {
                _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Dodge);
            }
            if (!isDodge) {
                var clip = _targetAnimator.runtimeAnimatorController.animationClips.SingleOrDefault(x => x.name.Contains("Attack"));
                var length = clip != null ? clip.length : 0.5f;
                StartCoroutine(DelayHitAnimations(isKill, _spawnedAttacker, length - 0.15f));
            }
            StartCoroutine(DelayedUpdate(0.5f));
        }

        public void Transition(Unit nextTarget, bool isAttacker) {
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
            InitializeAnimationObjects(out _spawnedTarget, out _targetImage, nextTarget, _isAttackerForceUnit, false);
            
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

            if (!_unitsAreOpponents) {
                Flip(_isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn);
            }

            if (isAttacker) {
                _targetImage.color = Constants.Invisible;
                var image = _spawnedTarget.transform.Find("weapon").GetComponent<Image>();
                image.color = Constants.Invisible;
            }

            StartCoroutine(DelayedUpdate(0.1f));
        }

        public void StopAttackAnimation() {
            _spellAnimator?.SetBool("EndAnimation", true);
            _targetAnimator?.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
            _attackerAnimator?.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
        }

        public void EndAttackAnimation() {
            foreach (Transform transformObject in _forceUnitSpawn) {
                Destroy(transformObject.gameObject);
            }
            foreach (Transform transformObject in _enemyUnitSpawn) {
                Destroy(transformObject.gameObject);
            }
            foreach (Transform transformObject in _spellSpawn) {
                Destroy(transformObject.gameObject);
            }
            _quickInfo.CloseQuickInfo();
            _quickInfoUiTarget.CloseQuickInfo();
            gameObject.SetActive(false);
        }

        private void InitializeAnimationObjects(out GameObject spawn, out Image image, Unit unit, bool isLeft, bool isAttacker) {
            var prefab = Resources.Load<GameObject>(unit.GetCharacter().GetBattleAnimationPath());
            spawn = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            image = spawn.transform.Find("Character").GetComponent<Image>();
            if (isLeft) {
                _quickInfoUiTarget.ShowQuickInfo(unit.GetCharacter());
                spawn.transform.SetParent(_enemyUnitSpawn, false);
            } else {
                _quickInfo.ShowQuickInfo(unit.GetCharacter());
                spawn.transform.SetParent(_forceUnitSpawn, false);
            }

            if (unit.GetCharacter().IsForce) {
                spawn.transform.Translate(0, -50, 0);
            }

            SetWeapon(spawn, unit.GetCharacter());

            if (isAttacker) {
                SetTexture2D(image, unit.GetCharacter(), out _attackerTexture2D);
                _attackerAnimator = spawn.GetComponent<Animator>();
            } else {
                SetTexture2D(image, unit.GetCharacter(), out _targetTexture2D);
                _targetAnimator = spawn.GetComponent<Animator>();
            }
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

        IEnumerator FlashAndWiggle(bool isKill, GameObject target) {
            var targetTransform = target.transform.Find("Character");
            var targetImage = target.transform.Find("Character").GetComponent<Image>();
            var weaponImage = target.transform.Find("weapon").GetComponent<Image>();
            var originalPos = targetTransform.transform.localPosition;
            var elapsed = 0.0f;
            var guiTextMaterial = new Material(Shader.Find("GUI/Text Shader"));
            var normalMaterial = targetImage.material;
 
            targetImage.material = guiTextMaterial;
            targetImage.color = Color.white;

            while (elapsed < 0.15f) {
                targetImage.material = weaponImage.material = guiTextMaterial;
                targetImage.color = weaponImage.color = Color.white;
                var x = Random.Range(-1f, 1f) * 5f;
                var y = Random.Range(-1f, 1f) * 5f;
                targetTransform.transform.localPosition = new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            targetImage.material = weaponImage.material = normalMaterial;
            targetImage.color = weaponImage.color = Color.white;
            if (isKill) {
                StartCoroutine(DissolveImage(targetImage, weaponImage));
            }
            targetTransform.transform.localPosition = originalPos;
        }

        IEnumerator DissolveImage(Image targetImage, Image weaponImage) {
            var _swapShader = Shader.Find("Hidden/Dissolve");
            var _newMat = new Material(_swapShader);
            targetImage.material = weaponImage.material = _newMat;
            targetImage.material.SetTexture("_DissolveTex", DissolveTexture2D);
            weaponImage.material.SetTexture("_DissolveTex", DissolveTexture2D);
            var dissolve = 0f;
            while (dissolve < 1) {
                dissolve += 0.005f;
                targetImage.material.SetFloat("_Threshold", dissolve);
                weaponImage.material.SetFloat("_Threshold", dissolve);
                yield return null;
            }

        }

        IEnumerator DelayHitAnimations(bool isKill, GameObject target, float seconds) {
            yield return new WaitForSeconds(seconds);
            StartCoroutine(FlashAndWiggle(isKill, target));
            
        }
    }
}
