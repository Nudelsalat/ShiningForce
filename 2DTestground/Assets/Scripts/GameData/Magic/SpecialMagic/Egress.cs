﻿using System;
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
            var warpGameObject = new GameObject();
            warpGameObject.AddComponent<WarpToScene>();
            var warp = warpGameObject.GetComponent<WarpToScene>();
            GameData data = SaveLoadGame.Load();
            warp.sceneToWarpTo = data.SceneName;
            warp.DoWarp();
            BattleController.Instance.SetEndBattle();
            return 0;
        }
    }
}

