using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.TownAI {
    class FlickerLightEffect : MonoBehaviour {
        public float FlickerFrequencyInSec = 0.2f;

        private float _origScaleX;
        private float _elapsedTime = 0.0f;
        private float _randomFrequency;

        void Awake() {
            _origScaleX = transform.localScale.x;
            _randomFrequency = UnityEngine.Random.Range(FlickerFrequencyInSec * 0.90f, FlickerFrequencyInSec * 1.10f);
        }

        void FixedUpdate() {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > _randomFrequency) {
                var newScale = UnityEngine.Random.Range(_origScaleX * 0.90f, _origScaleX * 1.10f);
                transform.localScale = new Vector3(newScale, newScale, 1);

                _elapsedTime = 0.0f;
                _randomFrequency = UnityEngine.Random.Range(FlickerFrequencyInSec * 0.90f, FlickerFrequencyInSec * 1.10f);
            }
        }
    }
}
