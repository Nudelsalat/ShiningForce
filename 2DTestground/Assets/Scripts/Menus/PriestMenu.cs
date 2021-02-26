using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Assets.Scripts.Menus {
    public class PriestMenu : MonoBehaviour {
        
        private AnimatorController _animatorRaiseButton;
        private AnimatorController _animatorCureButton;
        private AnimatorController _animatorSaveButton;
        private AnimatorController _animatorPromoteButton;

        private AudioClip _menuSwish;
        private AudioClip _menuDing;

        private Dialogue _tempDialogue = new Dialogue {
            Name = "Itemtext",
            Sentences = new List<string>() {
                "SENTENCE NOT REPLACED!"
            },
        };

        private string _currentlyAnimatedButton;
        private Portrait _portrait;
        private DialogManager _dialogManager;
        private AudioManager _audioManager;
        private List<PartyMember> _party;
        private DirectionType _inputDirection;
        private DirectionType _lastInputDirection;
        private EnumCurrentPriestMenu _enumCurrentMenuType;

        private Inventory _inventory;
        private FourWayButtonMenu _fourWayButtonMenu;

        public static PriestMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }
            
            _animatorRaiseButton = Resources.Load<AnimatorController>(Constants.AnimationsButtonRaise);
            _animatorCureButton = Resources.Load<AnimatorController>(Constants.AnimationsButtonCure);
            _animatorSaveButton = Resources.Load<AnimatorController>(Constants.AnimationsButtonSave);
            _animatorPromoteButton = Resources.Load<AnimatorController>(Constants.AnimationsButtonPromote);


            _menuSwish = Resources.Load<AudioClip>(Constants.SoundMenuSwish);
            _menuDing = Resources.Load<AudioClip>(Constants.SoundMenuDing);
        }

        void Start() {
            _inventory = Inventory.Instance;
            _dialogManager = DialogManager.Instance;
            _portrait = Portrait.Instance;
            _audioManager = AudioManager.Instance;
            _fourWayButtonMenu = FourWayButtonMenu.Instance;
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


            _currentlyAnimatedButton = _fourWayButtonMenu.SetDirection(_inputDirection);
            switch (_enumCurrentMenuType) {
                case EnumCurrentPriestMenu.raise:
                case EnumCurrentPriestMenu.cure:
                    break;
                case EnumCurrentPriestMenu.save:
                    break;
                case EnumCurrentPriestMenu.promote:
                    break;
                case EnumCurrentPriestMenu.none:
                    break;
            }
        }
        #endregion

        public void OpenPriestWindow() {
            if (Player.PlayerIsInMenu != EnumMenuType.none) {
                return;
            }
            Player.PlayerIsInMenu = EnumMenuType.priestMenu;
            _audioManager.PlaySFX(_menuSwish);
            _party = _inventory.GetParty();
            OpenPriestMenu();
        }

        private void OpenPriestMenu() {
            _fourWayButtonMenu.InitializeButtons(_animatorRaiseButton, _animatorCureButton,
                _animatorSaveButton, _animatorPromoteButton,
                "Raise", "Cure", "Save", "Promote");
        }
        private void ClosePriestMenu() {
        }

        void EventTrigger() {
            OpenPriestWindow();
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

    }

    public enum EnumCurrentPriestMenu {
        none,
        raise,
        cure,
        save,
        promote
    }
}
