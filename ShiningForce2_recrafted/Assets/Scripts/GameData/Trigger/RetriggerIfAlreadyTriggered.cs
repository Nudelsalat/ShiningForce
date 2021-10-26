using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    class RetriggerIfAlreadyTriggered : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour EventToTrigger;

        public new void Start() {
            _triggerStorage = TriggerStorage.Instance;
            _sceneToSave = string.IsNullOrEmpty(PreviousSceneName)
                ? SceneManager.GetActiveScene().name
                : PreviousSceneName;
            _triggerHash = string.IsNullOrEmpty(PreviousTriggerName) ? gameObject.name.GetHashCode() : PreviousTriggerName.GetHashCode();
            if (_triggerStorage.AlreadyPickedUp(_sceneToSave, _triggerHash)) {
                IsAlreadyTriggered = true;
                if (IsAlreadyTriggered) {
                    EventTrigger();
                }
                if (RemoveObjectAfterTrigger) {
                    Destroy(gameObject);
                }
            }
        }

        public void EventTrigger() {
            EventToTrigger?.Invoke("EventTrigger",0f);
            base.SetAsTriggered();
        }
    }
}
