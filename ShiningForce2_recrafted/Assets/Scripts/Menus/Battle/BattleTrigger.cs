using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Battle;
using Assets.Scripts.GameData.Trigger;
using UnityEngine;

namespace Assets.Scripts.Menus.Battle {
    class BattleTrigger : MonoBehaviour, IEventTrigger {
        //Negative number will wait until triggerd via event.
        public float TriggerInSeconds = 0;
        public MonoBehaviour FollowUpEvent;

        private BattleController _battleController;
        void Start() {
            if (TriggerInSeconds >= 0) {
                StartCoroutine(StartTriggerAfterSeconds(TriggerInSeconds));
            }
        }
        public void EventTrigger() {
            _battleController = BattleController.Instance;
            _battleController.BeginBattle();
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }

        }

        IEnumerator StartTriggerAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            EventTrigger();
        }

    }
}
