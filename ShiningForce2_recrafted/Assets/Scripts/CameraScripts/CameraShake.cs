using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.CameraScripts {
    public class CameraShake : MonoBehaviour {

        private bool _doShake;

        public static CameraShake Instance;
        

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }
        }

        public void StartCameraShake(float magnitude) {
            _doShake = true;
            StartCoroutine(Shake(0, magnitude));
        }

        public void StopCameraShake() {
            _doShake = false;
        }

        public IEnumerator Shake(float duration, float magnitude) {
            var originalPos = transform.localPosition;
            var elapsed = 0.0f;

            while (elapsed < duration && _doShake) {
                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = transform.localPosition + new Vector3(x,y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPos;
        }
    }
}
