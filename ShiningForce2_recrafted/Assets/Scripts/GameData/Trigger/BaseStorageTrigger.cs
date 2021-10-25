using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    public abstract class BaseStorageTrigger : MonoBehaviour {
        public string PreviousSceneName;
        public string PreviousTriggerName;
        public bool RemoveObjectAfterTrigger;

        private TriggerStorage _triggerStorage;
        protected bool IsAlreadyTriggered = false;
        private string _sceneToSave;
        private int _triggerHash;

        protected void Start() {
            _triggerStorage = TriggerStorage.Instance;
            _sceneToSave = string.IsNullOrEmpty(PreviousSceneName)
                ? SceneManager.GetActiveScene().name
                : PreviousSceneName;
            _triggerHash = string.IsNullOrEmpty(PreviousTriggerName) ? gameObject.name.GetHashCode() : PreviousTriggerName.GetHashCode();
            if (_triggerStorage.AlreadyPickedUp(_sceneToSave, _triggerHash)) {
                IsAlreadyTriggered = true;
                if (RemoveObjectAfterTrigger) {
                    Destroy(gameObject);
                }
            }
        }

        protected void SetAsTriggered() {
            if (!IsAlreadyTriggered) {
                IsAlreadyTriggered = true;
                _triggerStorage.AddToPickedUpList(_sceneToSave, _triggerHash);
                if (RemoveObjectAfterTrigger) {
                    Destroy(gameObject);
                }
            }
        }
    }
}
