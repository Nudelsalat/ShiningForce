using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger.ConditionalTrigger {
    public abstract class BaseConditionalTrigger : MonoBehaviour {
        public MonoBehaviour TriggeredFollowUpEvent;
        public MonoBehaviour NotTriggeredFollowUpEvent;

    }
}
