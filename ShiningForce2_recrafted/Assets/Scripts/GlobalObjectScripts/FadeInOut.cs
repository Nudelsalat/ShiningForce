using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GlobalObjectScripts {
    public class FadeInOut : MonoBehaviour {

        public static FadeInOut Instance;

        private CanvasGroup _canvasGroup;
        private bool _toBlack = false;
        private bool _inTransition = false;
        private bool _doExecute = false;
        private float _speed = 2f;
        private string _sceneToWarpTo = "";

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }
            _canvasGroup = transform.GetComponent<CanvasGroup>();
        }

        void FixedUpdate() {
            if (_inTransition) {
                return;
            }
            if (_toBlack && _canvasGroup.alpha < 1) {
                _canvasGroup.alpha += Time.deltaTime * _speed;
            } else if(!_toBlack && _canvasGroup.alpha > 0) {
                _canvasGroup.alpha -= Time.deltaTime * _speed;
            } else if (_toBlack) {
                if (!string.IsNullOrEmpty(_sceneToWarpTo)) {
                    StartCoroutine(StayBlackUntilSceneLoaded());
                } else {
                    StartCoroutine(StayBlackForSeconds(0.25f));
                }
            } else if (_doExecute) {
                _doExecute = false;
                Player.InWarp = false;
            }
        }

        public void FadeIn(float speed) {
            Player.InWarp = true;
            _doExecute = true;
            _canvasGroup.alpha = 1;
            _toBlack = false;
            _speed = speed;
        }
        public void FadeOutAndThenBackIn(float speed, string sceneToWarpTo = null) {
            Player.InWarp = true;
            _sceneToWarpTo = sceneToWarpTo;
            _doExecute = true;
            _canvasGroup.alpha = 0;
            _toBlack = true;
            _speed = speed;
        }

        IEnumerator StayBlackUntilSceneLoaded() {
            _inTransition = true;
            Debug.Log($"Loading scene: {_sceneToWarpTo}");
            yield return SceneManager.LoadSceneAsync(_sceneToWarpTo);
            yield return new WaitForSeconds(0.2f);

            _inTransition = false;
            _sceneToWarpTo = null;
            _toBlack = false;
        }

        IEnumerator StayBlackForSeconds(float time) {
            _inTransition = true;
            yield return new WaitForSeconds(time);

            _inTransition = false;
            _toBlack = false;
        }
    }
}
