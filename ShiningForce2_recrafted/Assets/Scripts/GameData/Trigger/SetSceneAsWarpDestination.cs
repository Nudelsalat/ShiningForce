using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.GameData.Trigger {
    class SetSceneAsWarpDestination : MonoBehaviour, IEventTrigger {
        public void Start() {
            Inventory.Instance.SetWarpSceneName(SceneManager.GetActiveScene().name);
        }

        public void EventTrigger() {
            Inventory.Instance.SetWarpSceneName(SceneManager.GetActiveScene().name);
        }
    }
}
