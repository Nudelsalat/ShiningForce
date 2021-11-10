using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.TownAI;
using UnityEngine;

namespace Assets.Scripts.GameData.Trigger {
    public class TriggerUpdateLookDirection : MonoBehaviour, IEventTrigger {
        public GameObject Unit;
        public DirectionType Direction = DirectionType.none;
        public MonoBehaviour FollowUpEvent;

        public void EventTrigger() {
            var lookDirection = Unit.transform.GetComponent<LookDirection>();
            var animator = Unit.transform.GetComponent<Animator>();

            if (lookDirection != null) {
                lookDirection.Direction = Direction;
            }
            if (animator != null) {
                animator.SetInteger("moveDirection", (int)Direction);
            }
            
            if (FollowUpEvent != null) {
                FollowUpEvent.Invoke("EventTrigger", 0);
            }
        }
    }
}
