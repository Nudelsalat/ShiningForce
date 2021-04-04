using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class PriestMenu : MonoBehaviour {
        
        private Sprite PriestSprite;
        private GameObject Gold;

        private RuntimeAnimatorController _animatorRaiseButton;
        private RuntimeAnimatorController _animatorCureButton;
        private RuntimeAnimatorController _animatorSaveButton;
        private RuntimeAnimatorController _animatorPromoteButton;

        private Animator _animatorGold;

        private AudioClip _menuSwish;
        private AudioClip _menuDing;

        private Dialogue _tempDialogue;

        private string _currentlyAnimatedButton;
        private bool _inMenu = false;
        private bool _showUi = false;
        private int _currentPrice;
        private List<PartyMember> _party; 
        private List<string> _sentences = new List<string>();
        private Queue<PartyMember> _affectedMembersQueue;
        private Queue<EnumStatusEffect> _statusEffectsToCureQueue;
        private PartyMember _currentPartyMember;
        private EnumStatusEffect _currentStatusEffect;
        private DirectionType _inputDirection;
        private DirectionType _lastInputDirection;
        private EnumCurrentPriestMenu _currentPriestMenu;

        private Inventory _inventory;
        private FourWayButtonMenu _fourWayButtonMenu;
        private DialogManager _dialogManager;
        private AudioManager _audioManager;

        public static PriestMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _tempDialogue = new Dialogue {
                Name = "Itemtext",
                Sentences = new List<string>() {
                    "SENTENCE NOT REPLACED!"
                },
                Portrait = PriestSprite
            };

            _affectedMembersQueue = new Queue<PartyMember>();
            _statusEffectsToCureQueue = new Queue<EnumStatusEffect>();

            _animatorRaiseButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonRaise);
            _animatorCureButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonCure);
            _animatorSaveButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonSave);
            _animatorPromoteButton = Resources.Load<RuntimeAnimatorController>(Constants.AnimationsButtonPromote);

            Gold = GameObject.Find("Gold");
            _animatorGold = Gold.transform.GetComponent<Animator>();

            _menuSwish = Resources.Load<AudioClip>(Constants.SoundMenuSwish);
            _menuDing = Resources.Load<AudioClip>(Constants.SoundMenuDing);
        }

        void Start() {
            _inventory = Inventory.Instance;
            _dialogManager = DialogManager.Instance;
            _audioManager = AudioManager.Instance;
            _fourWayButtonMenu = FourWayButtonMenu.Instance;
            transform.gameObject.SetActive(false);
        }

        #region OverAllInput
        
        // Update is called once per frame
        void Update() {
            if (Player.PlayerIsInMenu != EnumMenuType.priestMenu) {
                return;
            }
            GetInputDirection();
            if (Player.IsInDialogue || Player.InputDisabledInDialogue || Player.InputDisabledInEvent) {
                if ((Input.GetButtonUp("Interact") || Input.GetButtonUp("Back")) 
                    && !Player.InputDisabledInDialogue && !Player.InputDisabledInEvent) {
                    _dialogManager.DisplayNextSentence();
                }
                return;
            }

            if (!_inMenu) {
                switch (_currentPriestMenu) {
                    case EnumCurrentPriestMenu.none:
                        HandlePriestMenu();
                        break;
                    case EnumCurrentPriestMenu.raise:
                        HandleRaise();
                        break;
                    case EnumCurrentPriestMenu.cure:
                        HandleCure();
                        break;
                    case EnumCurrentPriestMenu.save:
                        EndSave();
                        break;
                    case EnumCurrentPriestMenu.promote:
                        HandlePriestMenu();
                        break;
                }
            }
        }
        #endregion

        public void OpenPriestWindow(Sprite priestSprite) {
            if (Player.PlayerIsInMenu != EnumMenuType.none) {
                return;
            }

            PriestSprite = priestSprite;
            _tempDialogue.Portrait = priestSprite;
            transform.gameObject.SetActive(true);
            Gold.SetActive(true);
            Player.PlayerIsInMenu = EnumMenuType.priestMenu;
            _audioManager.PlaySFX(_menuSwish);
            _party = _inventory.GetParty();
            _showUi = true;
            OpenPriestMenu();
        }

        private void OpenPriestMenu() {
            _fourWayButtonMenu.InitializeButtons(_animatorRaiseButton, _animatorCureButton,
                _animatorSaveButton, _animatorPromoteButton,
                "Raise", "Cure", "Save", "Promote");
        }

        private void HandlePriestMenu() {
            _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);

            if (Input.GetButtonUp("Interact")) {
                _fourWayButtonMenu.CloseButtons();
                switch (_currentlyAnimatedButton) {
                    case "Raise":
                        InitializeRaise();
                        break;
                    case "Cure":
                        InitializeCure();
                        break;
                    case "Save":
                        InitializeSave();
                        break;
                    case "Promote":
                        //TODO HandlePromote();
                        break;
                }
            }

            if (Input.GetButtonUp("Back")) {
                CloseMenuForGood();
            }
        }

        private void InitializeRaise() {
            _sentences.Clear();
            _sentences.Add("Let me have a look...\n");
            var deadMembers = _party.Where(x => x.StatusEffects.HasFlag(EnumStatusEffect.dead));
            if (deadMembers.ToList().Count <= 0) {
                _sentences.Add("You look all fine to me!\n");
                _fourWayButtonMenu.OpenButtons();
                EvokeSentencesDialogue(_sentences);
                return;
            } else {
                OpenGold();
                _affectedMembersQueue.Clear();
                foreach (var member in deadMembers) {
                    _affectedMembersQueue.Enqueue(member);
                }
            }
            _currentPriestMenu = EnumCurrentPriestMenu.raise;
        }

        private void HandleRaise() {
            if (_affectedMembersQueue.Count == 0) {
                _sentences.Add("That's all.");
                _currentPriestMenu = EnumCurrentPriestMenu.none;
                _fourWayButtonMenu.OpenButtons();
                CloseGold();
                EvokeSentencesDialogue(_sentences);
                return;
            }
            _currentStatusEffect = EnumStatusEffect.dead;
            _currentPartyMember = _affectedMembersQueue.Dequeue();
            _currentPrice = _currentPartyMember.CharStats.Level * 10 + (_currentPartyMember.IsPromoted ? 200 : 0);
            _sentences.Add($"Gosh! {_currentPartyMember.Name.AddColor(Constants.Orange)} is exhausted!\n" +
                           $"But I can recall the soul.\n It Will cost {_currentPrice} gold\n" +
                           $"OK?");
            var raiseCallback = new QuestionCallback {
                Name = "Raise",
                Sentences = _sentences,
                OnAnswerAction = CureOrNot,
                Portrait = PriestSprite
            };
            _inMenu = true;
            _dialogManager.StartDialogue(raiseCallback);
        }

        private void InitializeCure() {
            OpenGold();
            _sentences.Clear();
            _sentences.Add("Let me have a look...");
            var statesToCure = new List<EnumStatusEffect>() {
                EnumStatusEffect.poisoned,
                EnumStatusEffect.cursed,
                EnumStatusEffect.paralyzed,
                EnumStatusEffect.confused,
                EnumStatusEffect.sleep,
                EnumStatusEffect.silent,
                EnumStatusEffect.slowed
            };
            _affectedMembersQueue.Clear();
            _statusEffectsToCureQueue.Clear();
            foreach (var status in statesToCure) {
                _statusEffectsToCureQueue.Enqueue(status);
            }
            _currentPriestMenu = EnumCurrentPriestMenu.cure;
        }

        private void HandleCure() {
            if (_affectedMembersQueue.Count == 0) {
                if (_statusEffectsToCureQueue.Count == 0) {
                    _sentences.Add("That's all.");
                    _currentPriestMenu = EnumCurrentPriestMenu.none;
                    _fourWayButtonMenu.OpenButtons();
                    CloseGold();
                    EvokeSentencesDialogue(_sentences);
                    return;
                }

                _currentStatusEffect = _statusEffectsToCureQueue.Dequeue();
                var statusEffectName = Enum.GetName(typeof(EnumStatusEffect), _currentStatusEffect);
                var effectedMember = _party.Where(x => x.StatusEffects.HasFlag(_currentStatusEffect));
                if (effectedMember.ToList().Count <= 0) {
                    _sentences.Add($"Nobody is {statusEffectName.AddColor(Color.gray)}");
                }
                else {
                    foreach (var member in effectedMember) {
                        _affectedMembersQueue.Enqueue(member);
                    }
                    return;
                }
            }
            else {
                _currentPartyMember = _affectedMembersQueue.Dequeue();
                _currentPrice = 100;
                var statusEffectName = Enum.GetName(typeof(EnumStatusEffect), _currentStatusEffect);
                _sentences.Add($"Gosh! {_currentPartyMember.Name.AddColor(Constants.Orange)} is " +
                               $"{statusEffectName.AddColor(Color.grey)}!\n" +
                               $"But I can heal that.\n It Will cost {_currentPrice} gold\n" +
                               $"OK?");

                var raiseCallback = new QuestionCallback {
                    Name = "Raise",
                    Sentences = _sentences,
                    OnAnswerAction = CureOrNot,
                    Portrait = PriestSprite
                };
                _inMenu = true;
                _dialogManager.StartDialogue(raiseCallback);
            }
        }

        private void CureOrNot(bool answer) {
            _sentences.Clear();
            if (answer) {
                if (_inventory.GetGold() < _currentPrice) {
                    _sentences.Add("You don't have enough gold...\n");
                    _inMenu = false;
                    return;
                }

                _currentPartyMember.StatusEffects = 
                    _currentStatusEffect == EnumStatusEffect.dead 
                        ? EnumStatusEffect.none 
                        : _currentPartyMember.StatusEffects.Remove(_currentStatusEffect);

                _inventory.RemoveGold(_currentPrice);
                OpenGold();
                var statusEffectName = Enum.GetName(typeof(EnumStatusEffect), _currentStatusEffect);
                _sentences.Add($"{_currentPartyMember.Name.AddColor(Constants.Orange)} is no longer " +
                               $"{statusEffectName.AddColor(Color.grey)}!\n");
            } else {
                _sentences.Add("O...okay\n");
            }
            _inMenu = false;
        }

        private void InitializeSave() {
            _currentPriestMenu = EnumCurrentPriestMenu.save;
            _sentences.Clear();
            _sentences.Add($"Should I write down your journey so far?");

            var raiseCallback = new QuestionCallback {
                Name = "Save",
                Sentences = _sentences,
                OnAnswerAction = SaveOrNot,
                Portrait = PriestSprite
            };
            _inMenu = true;
            _dialogManager.StartDialogue(raiseCallback);
        }

        private void EndSave() {
            EvokeSentencesDialogue(_sentences);
            _fourWayButtonMenu.OpenButtons();
            _currentPriestMenu = EnumCurrentPriestMenu.none;
        }

        private void SaveOrNot(bool answer) {
            _sentences.Clear();
            if (answer) {
                SaveLoadGame.Save();
                StartCoroutine(SaveGame());
            }
            else {
                _sentences.Add("O...okay\n");
            }
            _inMenu = false;
        }

        private void CloseMenuForGood() {
            Gold.SetActive(false);
            _showUi = false;
            _fourWayButtonMenu.CloseButtons();
            Player.PlayerIsInMenu = EnumMenuType.none;
            StartCoroutine(WaitForTenthASecond());
        }

        private void GetInputDirection() {
            var currentDirection = DirectionType.none;
            if (Input.GetAxisRaw("Vertical") > 0.05f) {
                currentDirection = DirectionType.up;
            } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
                currentDirection = DirectionType.left;
            } else if (Input.GetAxisRaw("Vertical") < -0.05f) {
                currentDirection = DirectionType.down;
            } else if (Input.GetAxisRaw("Horizontal") > 0.05f) {
                currentDirection = DirectionType.right;
            } else {
                _inputDirection = DirectionType.none;
            }

            if (currentDirection == _lastInputDirection) {
                _inputDirection = DirectionType.none;
            } else {
                _lastInputDirection = _inputDirection = currentDirection;
                if (_inputDirection != DirectionType.none) {
                    _audioManager.PlaySFX(_menuDing);
                }
            }

            if (Input.GetButtonUp("Back") || Input.GetButtonUp("Interact")) {
                _audioManager.PlaySFX(_menuSwish);
            }
        }

        private void OpenGold() {
            _animatorGold.SetBool("isOpen", true);
            Gold.transform.Find("GoldText").GetComponent<Text>().text = _inventory.GetGold().ToString();
        }

        private void CloseGold() {
            _animatorGold.SetBool("isOpen", false);
        }

        private void EvokeSentencesDialogue(List<string> sentence) {
            _tempDialogue.Sentences = (sentence);
            _dialogManager.StartDialogue(_tempDialogue);
        }

        IEnumerator SaveGame() {
            Player.InputDisabledInEvent = true;
            _audioManager.PauseAll();
            var duration = _audioManager.Play("save", false);
            yield return new WaitForSecondsRealtime(duration);
            Player.InputDisabledInEvent = false;
            _audioManager.UnPauseAll();
            _sentences.Clear();
            _sentences.Add("The light allows you to resume your adventure.");
        }

        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showUi) {
                transform.gameObject.SetActive(false);
                Gold.SetActive(false);
            }
        }

        public enum EnumCurrentPriestMenu {
            none,
            raise,
            cure,
            save,
            promote
        }
    }
}
