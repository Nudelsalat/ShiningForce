using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus {
    public class Portrait : MonoBehaviour {

        public GameObject PortraitObject;

        private static Animator _portraitAnimator;
        private bool _showPortrait = false;

        public static Portrait Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _portraitAnimator = PortraitObject.transform.GetComponent<Animator>();
            PortraitObject.SetActive(false);
        }

        public void ShowPortrait(Sprite portrait) {
            _showPortrait = true;
            var image = PortraitObject.transform.Find("PortraitPicture").GetComponent<Image>();
            image.sprite = portrait;
            PortraitObject.SetActive(true);
            _portraitAnimator.SetBool("portraitIsOpen", true);
        }

        public void HidePortrait() {
            _portraitAnimator.SetBool("portraitIsOpen", false);
            _showPortrait = false;
            if (this.isActiveAndEnabled) {
                StartCoroutine(WaitForTenthASecond());
            }
        }

        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showPortrait) {
                PortraitObject.SetActive(false);
            }
        }
    }
}

