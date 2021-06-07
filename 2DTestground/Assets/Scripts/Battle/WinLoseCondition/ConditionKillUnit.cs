using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.WinLoseCondition {
    public class ConditionKillUnit : Condition {
        public Unit UnitCheckIfKilled;

        public override bool IsConditionMet() {
            return UnitCheckIfKilled == null || UnitCheckIfKilled.GetCharacter().CharStats.CurrentHp <= 0;
        }
    }
}
