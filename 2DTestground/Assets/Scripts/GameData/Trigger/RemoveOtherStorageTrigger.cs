using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class RemoveOtherStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;
        public GameObject GameObjectToRemove;

        public new void Start() {
            base.Start();
            if (IsAlreadyTriggered) {
                Destroy(GameObjectToRemove);
            }
        }

        public void EventTrigger() {
            Destroy(GameObjectToRemove);
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
            base.SetAsTriggered();
        }
    }
}
