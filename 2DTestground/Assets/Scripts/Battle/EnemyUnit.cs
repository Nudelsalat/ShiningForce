using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.Battle.Trigger;
using Assets.Scripts.GameData.Magic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class EnemyUnit : Unit {
        // Trigger in Battle map, which changes the AI
        public TriggerBase Trigger;
        // Primary Target for Ai Script. e.g:
        // Follow: follows this target.
        // AttackSpecificUnit: primarily targets that unit
        public Unit TargetUnit;
        // For move toward command
        public GameObject TargetPoint;
        // This Ai Type is the first go to behaviour
        public EnumAiType InitialAiTypePrimary;
        // This Ai Type is a fallback behaviour if primary does not meet precondition
        public EnumAiType InitialAiTypeSecondary;
        // This Ai type is set after the trigger
        public EnumAiType TriggeredAiTypePrimary;
        // This Ai Type is a fallback behaviour
        public EnumAiType TriggeredAiTypeSecondary;

        private bool _isTriggered = false;
        private readonly AiData _aiData = new AiData();

        private new void Awake() {
            base.Awake();
        }

        private new void Start() {
            base.Start();
            _aiData.SetCharacter(_character);
            _aiData.PrimaryAiType = InitialAiTypePrimary;
            _aiData.SecondaryAiType = InitialAiTypeSecondary;

            _aiData.TargetUnit = TargetUnit;
            _aiData.TargetPoint = TargetPoint?.transform.position;
        }

        public void CheckTrigger() {
            if (_isTriggered || Trigger == null) {
                return;
            }
            if (Trigger.WasTriggered()) {
                DoTrigger();
                _isTriggered = true;
            }
        }

        public AiData GetAiData() {
            return _aiData;
        }

        private void DoTrigger() {
            _aiData.PrimaryAiType = TriggeredAiTypePrimary;
            _aiData.SecondaryAiType = TriggeredAiTypeSecondary;
        }
    }
}
