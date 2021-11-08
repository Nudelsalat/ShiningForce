using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class CollisionStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;

        void OnTriggerEnter2D(Collider2D collider) {
            if (collider.CompareTag("Player")) {
                if (FollowUpEvent != null) {
                    FollowUpEvent.Invoke("EventTrigger", 0);
                }
                base.SetAsTriggered();
            }
        }

        public void EventTrigger() {
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
            base.SetAsTriggered();
        }
    }
}
