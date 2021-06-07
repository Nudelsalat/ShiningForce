using System.Collections;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class TimedStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public float SecondsAfterWhichToTrigger = 0f;

        public new void Start() {
            base.Start();
            StartCoroutine(StartTriggerAfterSeconds(SecondsAfterWhichToTrigger));
        }

        IEnumerator StartTriggerAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);
            EventTrigger();
        }

        public void EventTrigger() {
            SetAsTriggered();
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
