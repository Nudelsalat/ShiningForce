using System;
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

        public override void ExecuteMagicAtLevel(Character caster, Character target, int magicLevel) {
            var warpGameObject = new GameObject();
            warpGameObject.AddComponent<WarpToScene>();
            var warp = warpGameObject.GetComponent<WarpToScene>();
            GameData data = SaveLoadGame.Load();
            warp.sceneToWarpTo = data.SceneName;
            warp.DoWarp();
            BattleController.Instance.EndBattle();
        }
    }
}

