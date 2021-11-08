using System.Collections;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public class NewGameStorageTrigger : BaseStorageTrigger, IEventTrigger {
        public MonoBehaviour FollowUpEvent;

        public new void Start() {
            base.Start();
        }

        public void EventTrigger() {
            if (!base.IsAlreadyTriggered) {
                var inventory = Inventory.Instance;
                var fileName = inventory.GetSaveFileName();
                if (string.IsNullOrEmpty(fileName)) {
                    fileName = "Bowie";
                }
                SaveLoadGame.Save(fileName);
                inventory.SetWarpSceneName(SceneManager.GetActiveScene().name);
                SetAsTriggered();
            }
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
