using System.Collections;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class SwapPositionTrigger : MonoBehaviour, IEventTrigger {
        public Transform TransformOne;
        public Transform TransformTwo;
        public MonoBehaviour FollowUpEvent;

        public void EventTrigger() {
            if (TransformOne != null && TransformTwo != null) {
                var posOne = TransformOne.position;
                TransformOne.position = TransformTwo.position;
                TransformTwo.position = posOne;
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
