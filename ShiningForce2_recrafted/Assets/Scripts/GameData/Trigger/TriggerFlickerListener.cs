using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {

    public class TriggerFlickerListener : MonoBehaviour {

        public float FlickerIntervalSeconds;
        [SerializeField]
        private SpriteRenderer SpriteRendererToFlicker;


        public void StartFlicker() {
            StartCoroutine("FlickerAnimator");
        }

        public void StopFlicker() {
            StopCoroutine("FlickerAnimator");
            SpriteRendererToFlicker.color = Constants.Visible;
        }
        IEnumerator FlickerAnimator() {
            while (true) {
                SpriteRendererToFlicker.color = Constants.Visible;
                yield return new WaitForSeconds(FlickerIntervalSeconds);
                SpriteRendererToFlicker.color = Constants.Invisible;
                yield return new WaitForSeconds(FlickerIntervalSeconds);
            }
        }

    }
}
