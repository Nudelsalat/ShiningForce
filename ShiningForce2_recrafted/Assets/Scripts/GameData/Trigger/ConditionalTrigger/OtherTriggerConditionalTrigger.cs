using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger.ConditionalTrigger {
    public abstract class OtherTriggerConditionalTrigger : BaseConditionalTrigger {
        public string TriggeredSceneName;
        public string TriggeredTriggerName;

        protected TriggerStorage _triggerStorage;
        protected bool IsAlreadyTriggered;


        public void EventTrigger() {
            _triggerStorage = TriggerStorage.Instance;
            if (_triggerStorage.AlreadyPickedUp(TriggeredSceneName, TriggeredTriggerName.GetHashCode())) {
                if (TriggeredFollowUpEvent != null) {
                    TriggeredFollowUpEvent.Invoke("EventTrigger", 0);
                }
                return;
            }
            if (NotTriggeredFollowUpEvent != null) {
                NotTriggeredFollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
