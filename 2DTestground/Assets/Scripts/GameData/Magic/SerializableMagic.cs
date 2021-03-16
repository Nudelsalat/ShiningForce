using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.GameData.Magic {
    [Serializable]
    public class SerializableMagic {
        public string ResourcePath;
        public int CurrentLevel;
        public DirectionType PositionInInventory;

        public SerializableMagic(Magic magic) {
            CurrentLevel = magic.CurrentLevel;
            PositionInInventory = magic.PositionInInventory;
            ResourcePath = string.Concat("SharedObjects/Magic/", magic.name.Replace("(Clone)",""));
        }

        public Magic GetMagic() {
            var magic = Object.Instantiate(Resources.Load<Magic>(this.ResourcePath));
            magic.PositionInInventory = this.PositionInInventory;
            magic.CurrentLevel = this.CurrentLevel;

            return magic;
        }
    }
}
