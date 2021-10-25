using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.GlobalObjectScripts {
    class HideTileMapOnStart : MonoBehaviour {

        void Start() {
           this.GetComponent<Tilemap>().color = Constants.Invisible;
        }
    }
}
