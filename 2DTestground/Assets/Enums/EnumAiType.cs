using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Enums {
    public enum EnumAiType {
        Idle = 0,
        Patrole = 1,
        InRangeAttack = 2,
        Aggressive = 3,
        InRangeMage = 4,
        AggressiveMage = 5,
        InRangeDebuff = 6,
        Healer = 7,
        Follow = 8,
        MoveTowardPoint = 9,
        MoveTowardTarget = 10,
        MoveTowardTargetBerserk = 11,
    }
}
