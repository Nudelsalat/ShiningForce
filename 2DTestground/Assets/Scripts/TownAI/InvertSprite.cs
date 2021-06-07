using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.TownAI {
    class InvertSprite : MonoBehaviour {
        public float FlickerFrequencyInSec = 0.2f;

        private float _elapsedTime = 0.0f;
        private float _randomFrequency;

        void Awake() {
            _randomFrequency = UnityEngine.Random.Range(FlickerFrequencyInSec * 0.90f, FlickerFrequencyInSec * 1.10f);
        }

        void FixedUpdate() {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > _randomFrequency) {
                transform.Rotate(0, 180, 0);
                _randomFrequency = UnityEngine.Random.Range(FlickerFrequencyInSec * 0.90f, FlickerFrequencyInSec * 1.10f);
                _elapsedTime = 0f;
            }
        }
    }
}
