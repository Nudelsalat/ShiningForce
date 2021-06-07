using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Scripts.Battle;
using UnityEngine;

namespace Assets.Scripts.GameData.Magic.SpecialMagic {
    [CreateAssetMenu(fileName = "Egress", menuName = "Magic/Egress")]
    [Serializable]
    public class Egress : Magic {

        public Egress() {
            MagicType = EnumMagicType.Special;
        }

        public override int ExecuteMagicAtLevel(Unit caster, List<Unit> target, int magicLevel) {
            var battleController = BattleController.Instance;
            battleController.SetDoWarp();
            battleController.SetEndBattle();
            return 0;
        }
    }
}

