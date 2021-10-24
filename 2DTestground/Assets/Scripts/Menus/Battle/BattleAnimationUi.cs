using System;
using System.Collections;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.EditorScripts;
using Assets.Scripts.GameData;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Menus.Battle {
    public class BattleAnimationUi : MonoBehaviour {

        public Texture2D DissolveTexture2D;

        private Image _backgroundImage;
        private Image _transitionImage;
        private Image _blackVoid;
        private Transform _forceUnitSpawn;
        private Transform _enemyUnitSpawn;
        private Transform _spellSpawn;

        private Animator _spellAnimator;
        private Animator _targetAnimator;
        private Animator _attackerAnimator;

        private QuickInfoUi _quickInfo;
        private QuickInfoUiTarget _quickInfoUiTarget;
        private AudioManager _audioManager;
        private LandEffectBackgroundMap _backgroundMap;
        private Cursor _cursor;
        private FadeInOut _fadeInOut;

        private Unit _attacker;
        private GameObject _spawnedAttacker;
        private GameObject _spawnedTarget;
        private Image _attackImage;
        private Image _targetImage;
        private Texture2D _attackerTexture2D;
        private Texture2D _targetTexture2D;

        private bool _isAttackerForceUnit;
        private bool _unitsAreOpponents;
        private bool _spellIsTriggered;

        private readonly float _fadeInAnimationDistance = 80f;
        private readonly float _fadeInMovementPerFrame = 2f;
        private readonly float _fadeFromBlackSpeed = 1.5f;

        public static BattleAnimationUi Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }
            _blackVoid = transform.Find("Blackvoid").GetComponent<Image>();
            _backgroundImage = transform.Find("Background/Background1").GetComponent<Image>();
            _transitionImage = transform.Find("Background/Transition").GetComponent<Image>();
            _forceUnitSpawn = transform.Find("Force/Character").GetComponent<Transform>();
            _enemyUnitSpawn = transform.Find("Enemy/Character").GetComponent<Transform>();
            _spellSpawn = transform.Find("SpellAnimator").GetComponent<Transform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _quickInfo = QuickInfoUi.Instance;
            _quickInfoUiTarget = QuickInfoUiTarget.Instance;
            _audioManager = AudioManager.Instance;
            _cursor = Cursor.Instance;
            _fadeInOut = FadeInOut.Instance;
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
            _backgroundMap = FindObjectOfType<LandEffectBackgroundMap>();
            gameObject.SetActive(true);
            _attacker = attacker;
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
                var weaponImage = _spawnedTarget.transform.Find("weapon")?.GetComponent<Image>();
                var platformImage = _spawnedTarget.transform.Find("Platform")?.GetComponent<Image>();
                if (weaponImage) {
                    weaponImage.color = Constants.Invisible;
                }
                if (platformImage) {
                    platformImage.color = Constants.Invisible;
                }
            }

            LoadBackgroundsAndPlatforms(attacker, target);
            StartCoroutine(DelayedUpdate(0.1f));
            StartCoroutine(DoFadeInAnimation(_fadeFromBlackSpeed/8));
            _fadeInOut.FadeIn(_fadeFromBlackSpeed);
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
                        Debug.LogWarning($"{spellPath} was not a valid prefab path!");
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
            StartCoroutine(TransitionRoutine(nextTarget, isAttacker));
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
            _fadeInOut.FadeIn(_fadeFromBlackSpeed);
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

        private void LoadBackgroundsAndPlatforms(Unit attacker, Unit target) {
            if (_isAttackerForceUnit && _unitsAreOpponents) {
                _backgroundImage.sprite = GetBackground(target);

                var platformImage = _spawnedAttacker.transform.Find("Platform")?.GetComponent<Image>();
                if (platformImage != null) {
                    var tileName = _cursor.GetTerrainName(attacker.transform.position);
                    var terrainEnum = TerrainEffects.GetTerrainTypeByName(tileName);
                    platformImage.sprite = _backgroundMap.GetPlatformForTerrain(terrainEnum);
                }
            } else if (_isAttackerForceUnit && !_unitsAreOpponents) {
                var tileName = _cursor.GetTerrainName(target.transform.position);
                var terrainEnum = TerrainEffects.GetTerrainTypeByName(tileName);
                _backgroundImage.sprite = _backgroundMap.GetBackgroundForTerrain(terrainEnum);

                var platformImageTarget = _spawnedTarget.transform.Find("Platform")?.GetComponent<Image>();
                if (platformImageTarget != null) {
                    platformImageTarget.sprite = _backgroundMap.GetPlatformForTerrain(terrainEnum);
                }
                var platformImageAttacker = _spawnedAttacker.transform.Find("Platform")?.GetComponent<Image>();
                if (platformImageAttacker != null) {
                    tileName = _cursor.GetTerrainName(attacker.transform.position);
                    terrainEnum = TerrainEffects.GetTerrainTypeByName(tileName);
                    platformImageAttacker.sprite = _backgroundMap.GetPlatformForTerrain(terrainEnum);
                }
            } else if (!_isAttackerForceUnit && _unitsAreOpponents) {
                var tileName = _cursor.GetTerrainName(attacker.transform.position);
                var terrainEnum = TerrainEffects.GetTerrainTypeByName(tileName);
                _backgroundImage.sprite = _backgroundMap.GetBackgroundForTerrain(terrainEnum);

                var platformImage = _spawnedTarget.transform.Find("Platform")?.GetComponent<Image>();
                if (platformImage != null) {
                    _backgroundImage.sprite = GetBackground(target);
                }
            } else if (!_isAttackerForceUnit && !_unitsAreOpponents) {
                _backgroundImage.sprite = GetBackground(attacker);
            }
        }

        private Sprite GetBackground(Unit target) {
            var tileName = _cursor.GetTerrainName(target.transform.position);
            var terrainEnum = TerrainEffects.GetTerrainTypeByName(tileName);
            return _backgroundMap.GetBackgroundForTerrain(terrainEnum);
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

        IEnumerator TransitionRoutine(Unit nextTarget, bool isAttacker) {
            var unitSpawn = _isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn;
            float elapsedTime = 0;
            float time = 0.15f; //TransitionSpeed

            var transitionPos = _transitionImage.transform.position;
            var backgroundPos = _backgroundImage.transform.position;
            var targetPos = unitSpawn.transform.position;
            
            var diff = Math.Abs(transitionPos.x - backgroundPos.x);
            while (elapsedTime < time) {
                if (_isAttackerForceUnit) {
                    _transitionImage.transform.position =
                        Vector3.Lerp(transitionPos, backgroundPos, (elapsedTime / time));
                    _backgroundImage.transform.position = Vector3.Lerp(backgroundPos,
                        backgroundPos + new Vector3(diff, 0), (elapsedTime / time));
                }
                unitSpawn.transform.position = Vector3.Lerp(targetPos, targetPos + new Vector3(diff,0), (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (_isAttackerForceUnit) {
                _backgroundImage.sprite = GetBackground(nextTarget);
                _backgroundImage.transform.position = transitionPos;
                foreach (Transform transform in _enemyUnitSpawn) {
                    Destroy(transform.gameObject);
                }
                unitSpawn = _enemyUnitSpawn;
                _quickInfoUiTarget.ShowQuickInfo(nextTarget.GetCharacter());
            } else {
                foreach (Transform transform in _forceUnitSpawn) {
                    Destroy(transform.gameObject);
                }
                unitSpawn = _forceUnitSpawn;
                _quickInfo.ShowQuickInfo(nextTarget.GetCharacter());
            }

            InitializeAnimationObjects(out _spawnedTarget, out _targetImage, nextTarget, _isAttackerForceUnit, false);
            _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
            if (!_unitsAreOpponents) {
                Flip(_isAttackerForceUnit ? _enemyUnitSpawn : _forceUnitSpawn);
            }
            if (isAttacker) {
                _targetImage.color = Constants.Invisible;
                var weaponImage = _spawnedTarget.transform.Find("weapon")?.GetComponent<Image>();
                var platformImage = _spawnedTarget.transform.Find("Platform")?.GetComponent<Image>();
                if (weaponImage) {
                    weaponImage.color = Constants.Invisible;
                }
                if (platformImage) {
                    platformImage.color = Constants.Invisible;
                }
            }
            LoadBackgroundsAndPlatforms(_attacker, nextTarget);
            unitSpawn.transform.position = targetPos - new Vector3(diff, 0);
            elapsedTime = 0;
            while (elapsedTime < time) {
                if (_isAttackerForceUnit) {
                    _transitionImage.transform.position = Vector3.Lerp(backgroundPos,
                        backgroundPos + new Vector3(diff, 0), (elapsedTime / time));
                    _backgroundImage.transform.position =
                        Vector3.Lerp(transitionPos, backgroundPos, (elapsedTime / time));
                }
                unitSpawn.transform.position = Vector3.Lerp(targetPos - new Vector3(diff, 0), targetPos, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _backgroundImage.transform.position = backgroundPos;
            _transitionImage.transform.position = transitionPos;
            unitSpawn.transform.position = targetPos;
        }

        IEnumerator DoFadeInAnimation(float delay) {
            var enemyPoint = _enemyUnitSpawn.transform.position;
            var forcePoint = _forceUnitSpawn.transform.position;
            var background = _backgroundImage.transform.position;
            _enemyUnitSpawn.transform.position += new Vector3(-_fadeInAnimationDistance, 0);
            _forceUnitSpawn.transform.position += new Vector3(_fadeInAnimationDistance, 0);
            _backgroundImage.transform.position += new Vector3(-_fadeInAnimationDistance/2, 0);

            yield return new WaitForSeconds(delay);
            while (Math.Abs(_enemyUnitSpawn.transform.position.x - enemyPoint.x) > 0.0001f)  {
                _enemyUnitSpawn.transform.position = Vector3.MoveTowards(_enemyUnitSpawn.transform.position, enemyPoint, _fadeInMovementPerFrame*Time.timeScale);
                _forceUnitSpawn.transform.position = Vector3.MoveTowards(_forceUnitSpawn.transform.position, forcePoint, _fadeInMovementPerFrame * Time.timeScale);
                _backgroundImage.transform.position = Vector3.MoveTowards(_backgroundImage.transform.position, background, _fadeInMovementPerFrame * Time.timeScale / 2);

                yield return null;
            }

            _enemyUnitSpawn.transform.position = enemyPoint;
            _forceUnitSpawn.transform.position = forcePoint;
            _backgroundImage.transform.position = background;
        }


        IEnumerator DelayedUpdate(float seconds) {
            yield return new WaitForSeconds(seconds);
            _quickInfo.UpdateInfo();
            _quickInfoUiTarget.UpdateInfo();
        }
        
        IEnumerator FlashAndWiggle(bool isKill, GameObject target) {
            _audioManager.PlaySFX(Constants.SfxHit);
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
            weaponImage.material = targetImage.material = _newMat;
            weaponImage.material.SetTexture("_DissolveTex", DissolveTexture2D);
            targetImage.material.SetTexture("_DissolveTex", DissolveTexture2D);
            var dissolve = 0f;
            while (dissolve < 1) {
                dissolve += 0.005f * Time.timeScale;
                weaponImage.material.SetFloat("_Threshold", dissolve);
                targetImage.material.SetFloat("_Threshold", dissolve);
                yield return null;
            }

        }

        IEnumerator DelayHitAnimations(bool isKill, GameObject target, float seconds) {
            yield return new WaitForSeconds(seconds);
            StartCoroutine(FlashAndWiggle(isKill, target));
            
        }
    }
}
