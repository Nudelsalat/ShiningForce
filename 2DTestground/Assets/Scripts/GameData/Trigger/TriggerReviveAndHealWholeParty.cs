using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.HelperScripts;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.GameData.Trigger {
    public class TriggerReviveAndHealWholeParty : MonoBehaviour, IEventTrigger {
        public void EventTrigger() {
            var inventory = Inventory.Instance;

            var force = inventory.GetParty().Where(x => x.activeParty);
            foreach (var unit in force) {
                unit.StatusEffects = unit.StatusEffects.Remove(EnumStatusEffect.dead);
                unit.FullyHeal();
            }
        }
    }
}
