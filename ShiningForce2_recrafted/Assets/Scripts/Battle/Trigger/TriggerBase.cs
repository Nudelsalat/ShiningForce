using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Trigger {
    public class TriggerBase : AreaOfEffectColliderManager {
        private bool _isTriggered = false;

        public bool WasTriggered() {
            if (_isTriggered) {
                return true;
            }
            return base.GetAllCurrentCollider(Constants.LayerMaskForce).Any();
        }
    }
}
