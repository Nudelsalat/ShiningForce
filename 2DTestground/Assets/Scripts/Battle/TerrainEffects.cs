using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.GameData.Characters;

namespace Assets.Scripts.GameData {
    public static class TerrainEffects {
        private static readonly Dictionary<EnumTerrainType, int> LandEffectDictionary
            = new Dictionary<EnumTerrainType, int>() {
            { EnumTerrainType.block, 0},
            { EnumTerrainType.desert, 30},
            { EnumTerrainType.forest, 30},
            { EnumTerrainType.grass, 30},
            { EnumTerrainType.hill, 30},
            { EnumTerrainType.hover, 0},
            { EnumTerrainType.plain, 15},
            { EnumTerrainType.road, 0},
            { EnumTerrainType.sky, 0},
            { EnumTerrainType.water, 30},
        };

        private static readonly Dictionary<Tuple<EnumMovementType,EnumTerrainType>, float> MovementCostDictionary =
            new Dictionary<Tuple<EnumMovementType, EnumTerrainType>, float>() {
                //amphibian
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.forest), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.hill), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.water), 1},

                //fast
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.water), 99},

                //floating
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.water), 1},

                //fly
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.sky), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.water), 1},

                //free
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.sky), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.water), 1},

                //foot
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.desert), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.forest), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.hill), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.water), 99},

                //horse
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.forest), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.hill), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.water), 99},

                //tires
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.desert), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.forest), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.hill), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.water), 99},

                //water
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.desert), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.forest), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.grass), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.hill), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.plain), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.road), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.water), 1},
            };

        public static int GetLandEffect(EnumTerrainType terrain) {
            if (LandEffectDictionary.TryGetValue(terrain, out var returnValue)) {
                return returnValue;
            }

            var terrainTypeName = Enum.GetName(typeof(EnumTerrainType), terrain);
            Debug.LogError($"Terraintype '{terrainTypeName}' could not be found!");
            return returnValue;
        }

        public static EnumTerrainType GetTerrainTypeByName(string terrain) {
            if (!Enum.TryParse(terrain, true, out EnumTerrainType enumTerrain)) {
                Debug.LogError($"Terraintype '{terrain}' could not be found!");
            }
            return enumTerrain;
        }

        public static int GetLandEffect(string terrain) {
            if (!Enum.TryParse(terrain, true, out EnumTerrainType enumTerrain)) {
                Debug.LogError($"Terraintype '{terrain}' could not be found!");
            }
            if (LandEffectDictionary.TryGetValue(enumTerrain, out var returnValue)) {
                return returnValue;
            }

            Debug.LogError($"Terraintype '{terrain}' could not be found!");
            return returnValue;
        }

        public static float GetMovementCost(EnumMovementType movementType, EnumTerrainType terrainType) {
            if (MovementCostDictionary.TryGetValue(
                new Tuple<EnumMovementType, EnumTerrainType>(movementType, terrainType),
                out var resultValue)) {
                return resultValue;
            }

            var movementTypeName = Enum.GetName(typeof(EnumMovementType), movementType);
            var terrainTypeName = Enum.GetName(typeof(EnumTerrainType), terrainType);
            Debug.LogError($"MovementCost not found for MovementType '{movementTypeName}' " +
                           $"and TerrainType '{terrainTypeName}'!");
            return 99;
        }
    }
}
