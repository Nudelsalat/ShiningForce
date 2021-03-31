using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Battle {
    class BattleCalculator {

        public int GetBaseDamageWeaponAttack(Unit attacker, Unit attacked, float landEffect) {
            float attack = attacker.GetCharacter().CharStats.Attack.GetModifiedValue();
            float defense = attacked.GetCharacter().CharStats.Defense.GetModifiedValue();
            float maxDamage = (attack - defense) * (1f - (landEffect / 100));
            float minDamage = maxDamage * 0.8f;
            var result = (int)Math.Round(Random.Range(minDamage, maxDamage), MidpointRounding.AwayFromZero);
            return result <= 0 ? 1 : result;
        }

        public bool RollForCrit(Unit attacker) {
            //TODO CRITICAL HITS attacker.Character.CharStats.CritChance
            var critChance = 12f;
            var critRole = Random.Range(0, 100);
            return critRole <= critChance;
        }

        public int GetCritDamage(Unit attacker, int baseDamage) {
            //TODO attacker.Character.CharStats.CritMultiplier
            var critMuliplier = 1.25f;
            var result = baseDamage * critMuliplier;
            return (int)Math.Round(result, MidpointRounding.AwayFromZero);
        }

    }
}
