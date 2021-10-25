using System;
using System.Collections.Generic;

namespace Assets.Scripts.GameData.Chests {
    [Serializable]
    public class PickedUpItemStorage {
        private List<Tuple<string, int>> _listOfPickedUpItems = new List<Tuple<string, int>>();

        private static readonly Lazy<PickedUpItemStorage>
            lazy =
                new Lazy<PickedUpItemStorage>
                    (() => new PickedUpItemStorage());

        public static PickedUpItemStorage Instance { get { return lazy.Value; } }

        private PickedUpItemStorage() {
        }

        public bool AlreadyPickedUp(string sceneName, int chestId) {
            if (_listOfPickedUpItems.Contains(new Tuple<string, int>(sceneName, chestId))) {
                return true;
            }

            return false;
        }

        public void AddToPickedUpList(string sceneName, int chestId) {
            var newEntry = new Tuple<string, int>(sceneName, chestId);
            if (_listOfPickedUpItems.Contains(newEntry)) {
                return;
            }
            _listOfPickedUpItems.Add(newEntry);
        }

        public void LoadData(PickedUpItemStorage dataToLoad) {
            _listOfPickedUpItems = dataToLoad._listOfPickedUpItems;
        }
    }
}
