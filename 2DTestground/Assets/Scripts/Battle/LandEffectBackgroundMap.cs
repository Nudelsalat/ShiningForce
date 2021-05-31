using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

namespace Assets.Scripts.Battle {
    public class LandEffectBackgroundMap : MonoBehaviour {
        public Sprite Road;
        public Sprite Plain;
        public Sprite Grass;
        public Sprite Forest;
        public Sprite Hill;
        public Sprite Sand;
        public Sprite Water;
        public Sprite Hover;
        public Sprite Sky;

        public Sprite PlatformRoad;
        public Sprite PlatformPlain;
        public Sprite PlatformGrass;
        public Sprite PlatformForest;
        public Sprite PlatformHill;
        public Sprite PlatformSand;
        public Sprite PlatformWater;
        public Sprite PlatformHover;
        public Sprite PlatformSky;

        public Sprite GetBackgroundForTerrain(EnumTerrainType enumTerrain) {
            switch (enumTerrain) {
                case EnumTerrainType.Desert:
                    return Sand;
                case EnumTerrainType.Forest:
                    return Forest;
                case EnumTerrainType.Grass:
                    return Grass;
                case EnumTerrainType.Hill:
                    return Hill;
                case EnumTerrainType.Hover:
                    return Hover;
                case EnumTerrainType.Plain:
                    return Plain;
                case EnumTerrainType.Road:
                    return Road;
                case EnumTerrainType.Sky:
                    return Sky;
                case EnumTerrainType.Water:
                    return Water;
                case EnumTerrainType.Block:
                default:
                    return Grass;
            }
        }

        public Sprite GetPlatformForTerrain(EnumTerrainType enumTerrain) {
            switch (enumTerrain) {
                case EnumTerrainType.Desert:
                    return PlatformSand;
                case EnumTerrainType.Forest:
                    return PlatformForest;
                case EnumTerrainType.Grass:
                    return PlatformGrass;
                case EnumTerrainType.Hill:
                    return PlatformHill;
                case EnumTerrainType.Hover:
                    return PlatformHover;
                case EnumTerrainType.Plain:
                    return PlatformPlain;
                case EnumTerrainType.Road:
                    return PlatformRoad;
                case EnumTerrainType.Sky:
                    return PlatformSky;
                case EnumTerrainType.Water:
                    return PlatformWater;
                case EnumTerrainType.Block:
                default:
                    return PlatformGrass;
            }
        }
    }
}
