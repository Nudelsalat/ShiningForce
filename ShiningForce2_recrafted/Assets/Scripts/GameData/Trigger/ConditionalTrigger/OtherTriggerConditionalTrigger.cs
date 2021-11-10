using Assets.Scripts.GameData.Chests;

namespace Assets.Scripts.GameData.Trigger.ConditionalTrigger {
    public class OtherTriggerConditionalTrigger : BaseConditionalTrigger, IEventTrigger {
        public string TriggeredSceneName;
        public string TriggeredTriggerName;

        private TriggerStorage _triggerStorage;


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