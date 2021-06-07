using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    class RetriggerIfAlreadyTriggered : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour EventToTrigger;

        public new void Start() {
            base.Start();
            if (IsAlreadyTriggered) {
                EventTrigger();
            }
        }

        public void EventTrigger() {
            EventToTrigger?.Invoke("EventTrigger",0f);
            base.SetAsTriggered();
        }
    }
}
