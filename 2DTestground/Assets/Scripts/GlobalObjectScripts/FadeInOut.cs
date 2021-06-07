using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GlobalObjectScripts {
    public class FadeInOut : MonoBehaviour {

        public static FadeInOut Instance;

        private CanvasGroup _canvasGroup;
        private bool _toBlack = false;
        private bool _doExecute = false;
        private float _speed = 2f;

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
            if (_toBlack && _canvasGroup.alpha < 1) {
                _canvasGroup.alpha += Time.deltaTime * _speed;
            } else if(!_toBlack && _canvasGroup.alpha > 0) {
                _canvasGroup.alpha -= Time.deltaTime * _speed;
            } else if (_toBlack) {
                _toBlack = false;
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
        public void FadeOutAndThenBackIn(float speed) {
            Player.InWarp = true;
            _doExecute = true;
            _canvasGroup.alpha = 0;
            _toBlack = true;
            _speed = speed;
        }
    }
}
