using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.GameData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.Battle {
    public class BattleAnimationUi : MonoBehaviour {

        private Image _backgroundImage;
        private Image _blackVoid;
        private Image _platformImage;
        private Transform _forceUnitSpawn;
        private Transform _enemyUnitSpawn;

        private Animator _targetAnimator;
        private Animator _attackerAnimator;

        private AudioManager _audioManager;
        private BattleController _battleController;
        private QuickInfoUi _quickInfo;
        private QuickInfoUiTarget _quickInfoUiTarget;

        private bool _showUi;
        private EnumAiState _state;

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
            
        }

        // Start is called before the first frame update
        void Start()
        {
            _audioManager = AudioManager.Instance;
            _battleController = BattleController.Instance;
            _quickInfo = QuickInfoUi.Instance;
            _quickInfoUiTarget = QuickInfoUiTarget.Instance;

            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            switch (_state) {
                case EnumAiState.None:
                    break;
                case EnumAiState.MoveCursorToUnit:
                    _attackerAnimator.SetInteger("Animation",(int)EnumAttackAnimations.Attack);
                    var randomness = Random.Range(0, 2);
                    if (randomness <= 0) {
                        _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Dodge);
                    }
                    StartCoroutine(WaitSeconds(1, EnumAiState.MoveUnit));
                    break;
                case EnumAiState.MoveUnit:
                    _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                    _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                    StartCoroutine(WaitSeconds(1, EnumAiState.ExecuteAction));
                    break;
                case EnumAiState.SelectTarget:
                    break;
                case EnumAiState.ExecuteAction:
                    EndAttackAnimation();
                    break;
            }
        }

        public void DoAttackAnimation(Unit attacker, List<Unit> targets, Image background, Image platform, 
            AttackOption attackOption, bool forceAttack) {

            Player.InputDisabledAiBattle = true;
            gameObject.SetActive(true);
            var firstTarget = targets.First();
                _quickInfo.ShowQuickInfo(attacker.GetCharacter());
                _quickInfoUiTarget.ShowQuickInfo(firstTarget.GetCharacter());
                var targetPrefab = Resources.Load<GameObject>(firstTarget.GetCharacter().GetBattleAnimationPath());
                var attackerPrefab = Resources.Load<GameObject>(attacker.GetCharacter().GetBattleAnimationPath());

                var spawnedTarget = Instantiate(targetPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                var spawnedAttacker = Instantiate(attackerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                if (attacker.GetCharacter().CharacterType != EnumCharacterType.monster) {
                    spawnedAttacker.transform.SetParent(_forceUnitSpawn, false);
                    spawnedTarget.transform.SetParent(_enemyUnitSpawn, false);
                }
                else {
                    spawnedTarget.transform.SetParent(_forceUnitSpawn, false);
                    spawnedAttacker.transform.SetParent(_enemyUnitSpawn, false);
                }

                _attackerAnimator = spawnedAttacker.GetComponent<Animator>();
                _targetAnimator = spawnedTarget.GetComponent<Animator>();
                _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

                SetWeapon(spawnedAttacker, attacker.GetCharacter());
                SetWeapon(spawnedTarget, firstTarget.GetCharacter());

            StartCoroutine(WaitSeconds(1, EnumAiState.MoveCursorToUnit));

        }

        public void EndAttackAnimation() {
            foreach (Transform transform in _forceUnitSpawn) {
                Destroy(transform.gameObject);
            }
            foreach (Transform transform in _enemyUnitSpawn) {
                Destroy(transform.gameObject);
            }

            gameObject.SetActive(false);
            Player.InputDisabledAiBattle = false;
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

        IEnumerator WaitSeconds(float seconds, EnumAiState nextState) {
            _state = EnumAiState.None;
            yield return new WaitForSeconds(seconds);
            _state = nextState;
        }
    }
}
