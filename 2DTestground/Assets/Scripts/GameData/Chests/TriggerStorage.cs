using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Battle;
using UnityEngine;

namespace Assets.Scripts.GameData.Chests {
    [Serializable]
    public class TriggerStorage {
        private List<Tuple<string, int>> _listOfTriggeredTriggers = new List<Tuple<string, int>>();

        private static readonly Lazy<TriggerStorage>
            lazy =
                new Lazy<TriggerStorage>
                    (() => new TriggerStorage());

        public static TriggerStorage Instance { get { return lazy.Value; } }

        private TriggerStorage() {
        }

        public bool AlreadyPickedUp(string sceneName, int chestId) {
            if (_listOfTriggeredTriggers.Contains(new Tuple<string, int>(sceneName, chestId))) {
                return true;
            }

            return false;
        }

        public void AddToPickedUpList(string sceneName, int chestId) {
            var newEntry = new Tuple<string, int>(sceneName, chestId);
            if (_listOfTriggeredTriggers.Contains(newEntry)) {
                return;
            }
            _listOfTriggeredTriggers.Add(newEntry);
        }

        public void LoadData(TriggerStorage dataToLoad) {
            _listOfTriggeredTriggers = dataToLoad._listOfTriggeredTriggers;
        }
    }
}
