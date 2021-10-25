using System;
using System.Collections.Generic;
using Assets.Enums;
using UnityEngine;
using Assets.Scripts.GameData.Characters;

namespace Assets.Scripts.GameData {
    public static class TerrainEffects {
        private static readonly Dictionary<EnumTerrainType, int> LandEffectDictionary
            = new Dictionary<EnumTerrainType, int>() {
            { EnumTerrainType.Block, 0},
            { EnumTerrainType.Desert, 30},
            { EnumTerrainType.Forest, 30},
            { EnumTerrainType.Grass, 30},
            { EnumTerrainType.Hill, 30},
            { EnumTerrainType.Hover, 0},
            { EnumTerrainType.Plain, 15},
            { EnumTerrainType.Road, 0},
            { EnumTerrainType.Sky, 0},
            { EnumTerrainType.Water, 30},
        };

        private static readonly Dictionary<Tuple<EnumMovementType,EnumTerrainType>, float> MovementCostDictionary =
            new Dictionary<Tuple<EnumMovementType, EnumTerrainType>, float>() {
                //amphibian
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Forest), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Hill), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.amphibian,EnumTerrainType.Water), 1},

                //fast
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fast,EnumTerrainType.Water), 99},

                //floating
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.floating,EnumTerrainType.Water), 1},

                //fly
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Sky), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.fly,EnumTerrainType.Water), 1},

                //free
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Desert), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Forest), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Hill), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Hover), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Sky), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.free,EnumTerrainType.Water), 1},

                //foot
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Desert), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Forest), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Hill), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.foot,EnumTerrainType.Water), 99},

                //horse
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Desert), 2},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Forest), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Grass), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Hill), 2.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.horse,EnumTerrainType.Water), 99},

                //tires
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Desert), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Forest), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Grass), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Hill), 1.5f},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Plain), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Road), 1},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.tires,EnumTerrainType.Water), 99},

                //Water
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Block), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Desert), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Forest), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Grass), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Hill), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Hover), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Plain), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Road), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Sky), 99},
                { new Tuple<EnumMovementType, EnumTerrainType>(EnumMovementType.water,EnumTerrainType.Water), 1},
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

        public static List<string> GetImpassableTerrainNameOfMovementType(EnumMovementType movementType) {
            var resultList = new List<string>();
            var allTerrainTypes = Enum.GetValues(typeof(EnumTerrainType));
            foreach (var terrainType in allTerrainTypes) {
                MovementCostDictionary.TryGetValue(
                    new Tuple<EnumMovementType, EnumTerrainType>(movementType, (EnumTerrainType) terrainType),
                    out var resultValue);
                if (resultValue >= 5) {
                    resultList.Add(Enum.GetName(typeof(EnumTerrainType), terrainType));
                }
            }

            return resultList;
        }
    }
}
