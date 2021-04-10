using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Battle;
using UnityEngine;

namespace Assets.Scripts.Menus.Battle {
    class BattleTrigger : MonoBehaviour {

        private BattleController _battleController;
        void Start() {
            _battleController = BattleController.Instance;
            //TODO: remove
            StartCoroutine(StartTriggerAfterSeconds(1));
        }
        public void EventTrigger() {
            _battleController.BeginBattle();
        }

        IEnumerator StartTriggerAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            EventTrigger();
        }

    }
}
