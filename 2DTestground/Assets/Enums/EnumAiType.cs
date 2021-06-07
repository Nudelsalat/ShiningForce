using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Enums {
    public enum EnumAiType {
        Idle = 0,
        // TODO:
        Patrole = 1,
        // try to physically attack unit within range. Else do nothing.
        InRangeAttack = 2,
        // try to physically attack unit within range, else move toward closest target.
        Aggressive = 3,
        // try to physically or magically attack unit within range. Else do nothing.
        InRangeMage = 4,
        // try to physically or magically attack unit within range, else move toward closest target.
        AggressiveMage = 5,
        // try to debuff unit within range or physically attack it, else move toward closest target.
        InRangeDebuff = 6,
        // Have Secondary action defined
        // If enough MP and HealSpell and at least one Target in reach misses X% HP (where x is defined by percentage in EnemyUnit) heal that unit,
        // else: secondary action.
        Healer = 7,
        // Use follow target as Secondary action.
        // Will follow target unit. if target unit not reachable get closer to it or no point defined, do nothing.
        Follow = 8,
        // Use follow target as Secondary action.
        // Will move toward a defined point. if point not reachable get closer to it or no point defined, do nothing.
        MoveTowardPoint = 9,
        // Tries to get to target and attack target. If Target is not reachable, execute best attack option
        MoveTowardTarget = 10,
        // Tries to get to target and attack target. If Target is not reachable, ignore all other enemies and move toward Target.
        MoveTowardTargetBerserk = 11,
        // like InRageAttack, but if no target in range, get closer 2 fields
        InRangeAttackOrMoveTwo = 12,
        // like InRageMage, but if no target in range, get closer 2 fields
        InRangeMageOrMoveTwo = 13,

    }
}
