using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {

    public class TriggerFlicker : MonoBehaviour, IEventTrigger {

        public TriggerFlickerListener FlickerListener;
        public float FlickerIntervalSeconds = 0.2f;
        public bool StartFlicker;
        public MonoBehaviour FollowUpEvent;

        public void EventTrigger() {
            FlickerListener.FlickerIntervalSeconds = FlickerIntervalSeconds;
            if (StartFlicker) {
                FlickerListener.StartFlicker();
            } else {
                FlickerListener.StopFlicker();
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
