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
        // Gold Drop
        public int GoldDropForKill = -1;
        // Trigger in Battle map, which changes the AI
        public TriggerBase Trigger;
        // Primary Target for Ai Script. e.g:
        // Follow: follows this target.
        // AttackSpecificUnit: primarily targets that unit
        public Unit TargetUnit;
        // For move toward command
        public GameObject TargetPoint;
        // Used for healer when to heal, or chance AI
        public float PercentChance = 0.5f;
        // This Ai Type is the first go to behaviour
        public EnumAiType InitialAiTypePrimary;
        // This Ai Type is a fallback behaviour if primary does not meet precondition
        public EnumAiType InitialAiTypeSecondary;
        // This Ai type is set after the trigger
        public EnumAiType TriggeredAiTypePrimary;
        // This Ai Type is a fallback behaviour
        public EnumAiType TriggeredAiTypeSecondary;
        // If unit is a boss, it will get double turns and it's own music
        public bool IsBoss = false;
        // Can drop item. Will be show in ItemSlot, if a space is free
        [SerializeField]
        private GameItem ItemToDrop;

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
            if (TargetPoint != null) {
                _aiData.TargetPoint = TargetPoint?.transform.position;
            }

            _aiData.PercentChance = PercentChance;
            if (ItemToDrop != null) {
                _character.TryAddItem(UnityEngine.Object.Instantiate(ItemToDrop));
            }
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

        public GameItem GetDropItem() {
            if (ItemToDrop != null) {
                return UnityEngine.Object.Instantiate(ItemToDrop);
            }
            return null;
        }

        public int GetGoldDrop() {
            if (GoldDropForKill >= 0) {
                return GoldDropForKill;
            } 
            if (_character is Monster monster) {
                        return monster.Gold;
            }
            return 0;
        }

        private void DoTrigger() {
            _aiData.PrimaryAiType = TriggeredAiTypePrimary;
            _aiData.SecondaryAiType = TriggeredAiTypeSecondary;
        }
    }
}
