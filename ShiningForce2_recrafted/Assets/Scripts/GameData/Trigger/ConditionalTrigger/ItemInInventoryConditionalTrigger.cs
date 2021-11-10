using UnityEngine;

namespace Assets.Scripts.GameData.Trigger.ConditionalTrigger {
    public class ItemInInventoryConditionalTrigger : BaseConditionalTrigger, IEventTrigger {
        public GameItem ItemToFind;
        public bool RemoveItemWhenFound;

        public void EventTrigger() {
            var inventory = Inventory.Instance;
            var party = inventory.GetParty();
            foreach (var partyMember in party) {
                var memberInventory = partyMember.GetInventory();
                foreach (var item in memberInventory) {
                    if (item.ItemName.Equals(ItemToFind.ItemName)) {
                        if (RemoveItemWhenFound) {
                            memberInventory[(int)item.PositionInInventory] = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
                        }
                        if (TriggeredFollowUpEvent != null) {
                            TriggeredFollowUpEvent.Invoke("EventTrigger", 0);
                        }
                        return;
                    }
                }
            }
            if (NotTriggeredFollowUpEvent != null) {
                NotTriggeredFollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
