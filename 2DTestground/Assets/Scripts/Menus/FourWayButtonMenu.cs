using System.Collections;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class FourWayButtonMenu : MonoBehaviour{
        public GameObject Up;
        public GameObject Left;
        public GameObject Down;
        public GameObject Right;

        private Animator _buttonsAnimator;
        private Animator _animatorUpButton;
        private Animator _animatorLeftButton;
        private Animator _animatorDownButton;
        private Animator _animatorRightButton;
        private Text _textLabel;
        private string _labelUp;
        private string _labelLeft;
        private string _labelDown;
        private string _labelRight;
        private bool _showUi = false;

        public static FourWayButtonMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _animatorUpButton = Up.transform.GetComponent<Animator>();
            _animatorLeftButton = Left.transform.GetComponent<Animator>();
            _animatorDownButton = Down.transform.GetComponent<Animator>();
            _animatorRightButton = Right.transform.GetComponent<Animator>();
            _buttonsAnimator = transform.GetComponent<Animator>();

            _textLabel = transform.Find("Label/LabelText").GetComponent<Text>();
        }

        void Start() {
            transform.gameObject.SetActive(false);
        }

        public void InitializeButtons(
            RuntimeAnimatorController up,
            RuntimeAnimatorController left,
            RuntimeAnimatorController down,
            RuntimeAnimatorController right, 
            string labelUp, 
            string labelLeft, 
            string labelDown, 
            string labelRight) 
        {
            _buttonsAnimator.SetBool("isOpen", true);
            _labelUp = labelUp;
            _labelDown = labelDown;
            _labelLeft = labelLeft;
            _labelRight = labelRight;

            _animatorUpButton.runtimeAnimatorController = up;
            _animatorLeftButton.runtimeAnimatorController = left;
            _animatorDownButton.runtimeAnimatorController = down;
            _animatorRightButton.runtimeAnimatorController = right;

            OpenButtons();
        }

        public void OpenButtons() {
            transform.gameObject.SetActive(true);
            _buttonsAnimator.SetBool("isOpen", true);
            _showUi = true;
            SetDirection(DirectionType.up);
        }

        public string SetDirection(DirectionType direction) {
            if (direction == DirectionType.none) {
                return _textLabel.text;
            }
            
            _animatorUpButton.SetBool("selected", false);
            _animatorDownButton.SetBool("selected", false);
            _animatorLeftButton.SetBool("selected", false);
            _animatorRightButton.SetBool("selected", false);

            switch (direction) {
                case DirectionType.up:
                    _textLabel.text = _labelUp;
                    _animatorUpButton.SetBool("selected", true);
                    break;
                case DirectionType.left:
                    _textLabel.text = _labelLeft;
                    _animatorLeftButton.SetBool("selected", true);
                    break;
                case DirectionType.down:
                    _textLabel.text = _labelDown;
                    _animatorDownButton.SetBool("selected", true);
                    break;
                case DirectionType.right:
                    _textLabel.text = _labelRight;
                    _animatorRightButton.SetBool("selected", true);
                    break;
            }

            return _textLabel.text;
        }
        public void CloseButtons() {
            _buttonsAnimator.SetBool("isOpen", false);
            _showUi = false;
            StartCoroutine(WaitForTenthASecond());
        }

        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showUi) {
                transform.gameObject.SetActive(false);
            }
        }
    }
}
