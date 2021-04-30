using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.Battle.Trigger;
using Assets.Scripts.GameData.Magic;
using UnityEngine;

namespace Assets.Scripts.Battle.AI {
    public class AiData {
        public Unit TargetUnit;
        public Vector3? TargetPoint = null;

        public Magic DamageMagic;
        public Magic HealingMagic;
        public Magic StatusEffectMagic;

        public float PercentChance = 0.5f;

        public EnumAiType PrimaryAiType;
        public EnumAiType SecondaryAiType;

        public void SetCharacter(Character character) {
            DamageMagic = character.GetMagic().FirstOrDefault(x => x.MagicType == EnumMagicType.Damage);
            HealingMagic = character.GetMagic().FirstOrDefault(x => x.MagicType == EnumMagicType.Heal);
            StatusEffectMagic = character.GetMagic().FirstOrDefault(x => x.MagicType == EnumMagicType.Special);
        }
    }
}
