using TMPro;
using UnityEngine;

namespace Assets.Scripts.Battle.WinLoseCondition {
    public class WinLoseConditionBase : MonoBehaviour {

        private BattleController _battleController;

        void Start() {
            _battleController = BattleController.Instance;
        }

        public bool CheckLoseCondition() {
            foreach (var unit in _battleController.GetForce()) {
                if (unit.GetCharacter() is PartyMember partyMember) {
                    if (partyMember.partyLeader) {
                        return false;
                    }
                }
            }

            ExecuteLose();
            return true;
        }

        public bool CheckWinCondition() {
            if (_battleController.GetEnemies().Count <= 0) {
                _battleController.SetEndBattle();
                return true;
            }
            return false;
        }

        private void ExecuteLose() {
            var warpGameObject = new GameObject();
            warpGameObject.AddComponent<WarpToScene>();
            var warp = warpGameObject.GetComponent<WarpToScene>();
            GameData.GameData data = SaveLoadGame.Load();
            //TODO Add sade jingle
            warp.sceneToWarpTo = data.SceneName;
            warp.DoWarp();
            _battleController.SetEndBattle();
        }
    }
}
