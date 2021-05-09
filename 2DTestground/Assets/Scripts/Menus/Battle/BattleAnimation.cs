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
    public class BattleAnimation : MonoBehaviour {

        private Image _backgroundImage;
        private Image _blackVoid;
        private Image _platformImage;
        private Transform _forceUnitSpawn;
        private Transform _enemyUnitSpawn;
        
        #region Tempstuff for testing
        private string _tempEnemyAnimationPath = "SharedObjects/Prefab/Battle/Animations/Enemies/Dwarf";
        private string _tempForceAnimationPath = "SharedObjects/Prefab/Battle/Animations/Heroes/Sarah";
        private string _weapon1 = "ShiningForce/images/weapon/sword5";
        private string _weapon2 = "ShiningForce/images/weapon/axe4";
        private string _weapon3 = "ShiningForce/images/weapon/sword6";
        #endregion

        private Animator _targetAnimator;
        private Animator _attackerAnimator;

        private AudioManager _audioManager;
        private BattleController _battleController;
        private QuickInfoUi _quickInfo;
        private QuickInfoUiTarget _quickInfoUiTarget;

        private bool _showUi;
        private EnumAiState _state;

        public static BattleAnimation Instance;

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
                    StartCoroutine(WaitSeconds(2, EnumAiState.MoveUnit));
                    break;
                case EnumAiState.MoveUnit:
                    _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                    _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                    StartCoroutine(WaitSeconds(2, EnumAiState.ExecuteAction));
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
            if (forceAttack) {
                _quickInfo.ShowQuickInfo(attacker.GetCharacter());
                _quickInfoUiTarget.ShowQuickInfo(firstTarget.GetCharacter());
                var left = Resources.Load<GameObject>(_tempEnemyAnimationPath);
                var right = Resources.Load<GameObject>(_tempForceAnimationPath);
                var spawnedRight = Instantiate(right, new Vector3(0, 0, 0), Quaternion.identity);
                spawnedRight.transform.SetParent(_forceUnitSpawn, false);
                var spawnedLeft = Instantiate(left, new Vector3(0, 0,0), Quaternion.identity);
                spawnedLeft.transform.SetParent(_enemyUnitSpawn, false);

                _attackerAnimator = spawnedRight.GetComponent<Animator>();
                _targetAnimator = spawnedLeft.GetComponent<Animator>();
                _attackerAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);
                _targetAnimator.SetInteger("Animation", (int)EnumAttackAnimations.Idle);

                SetWeapon(spawnedRight, attacker.GetCharacter());
            }

            StartCoroutine(WaitSeconds(2, EnumAiState.MoveCursorToUnit));

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

        private void SetWeapon(GameObject animation, Character character) {
            var randomness = Random.Range(0, 4);
            Sprite sprite = null; //TODO: empty sprite?
            if (randomness == 0) {
                sprite = Resources.Load<Sprite>(_weapon1);
            } else if (randomness == 1) {
                sprite = Resources.Load<Sprite>(_weapon2);
            } else if (randomness == 2) {
                sprite = Resources.Load<Sprite>(_weapon3);
            }

            
            //image.transform.position = new Vector3(0, 0, 0);
            if (sprite) {
                var image = animation.transform.Find("weapon").GetComponent<Image>();
                image.sprite = sprite;
                image.SetNativeSize();
                var normalizedPivotX = sprite.pivot.x / sprite.rect.width;
                var normalizedPivotY = sprite.pivot.y / sprite.rect.height;
                image.rectTransform.pivot = new Vector2(normalizedPivotX, normalizedPivotY);
            }
        }

        IEnumerator WaitSeconds(float seconds, EnumAiState nextState) {
            _state = EnumAiState.None;
            yield return new WaitForSeconds(seconds);
            _state = nextState;
        }
    }
}
