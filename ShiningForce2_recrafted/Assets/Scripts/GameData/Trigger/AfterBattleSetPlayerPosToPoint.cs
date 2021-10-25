using System.Collections;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class AfterBattleSetPlayerPosToPoint : MonoBehaviour, IEventTrigger {
        public Transform NewPos;
        public MonoBehaviour FollowUpEvent;

        public void EventTrigger() {
            if (NewPos != null) {
                Player.Instance.SetPosition(NewPos.position);
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
