using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Battle.WinLoseCondition {
    public class WinLoseConditionBase : MonoBehaviour {
        public List<MonoBehaviour> WinTrigger;
        public List<MonoBehaviour> LoseTrigger;
        public List<Condition> WinCondition;
        public List<Condition> LoseCondition;

        private BattleController _battleController;
        private Inventory _inventory;

        void Start() {
            _battleController = BattleController.Instance;
            _inventory = Inventory.Instance;
        }

        public bool CheckLoseCondition() {
            if (LoseCondition.Any()) {
                foreach (var condition in LoseCondition) {
                    if (condition != null)
                        if (condition.IsConditionMet()) {
                            ExecuteLose();
                            return true;
                        }
                }
            }
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
                if (WinTrigger.Any()) {
                    foreach (var trigger in WinTrigger) {
                        if(trigger != null)
                            trigger.Invoke("EventTrigger", 0);
                    }
                }
                return true;
            }
            if (WinCondition.Any()) {
                foreach (var condition in WinCondition) {
                    if (condition != null)
                        if (condition.IsConditionMet()) {
                            if (WinTrigger.Any()) {
                                foreach (var trigger in WinTrigger) {
                                    if (trigger != null)
                                        trigger.Invoke("EventTrigger", 0);
                                }
                            }
                            return true;
                        }
                }
            }
            return false;
        }

        private void ExecuteLose() {
            if (LoseTrigger.Any()) {
                foreach (var trigger in LoseTrigger) {
                    if (trigger != null)
                        trigger.Invoke("EventTrigger", 0);
                }
            }
            _inventory.RemoveGold(_inventory.GetGold() / 2);
            _battleController.SetDoWarp();
            _battleController.SetEndBattle();
        }
    }
}
