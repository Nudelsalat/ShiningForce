using System;
using Assets.Enums;
using Random = UnityEngine.Random;

namespace Assets.Scripts.GameData.Characters {
    public class LevelUpGrowth {
        private static readonly float[] _linearGrowth = new[] {
            4.17f, 4.17f, 4.16f, 4.17f, 4.17f, 4.16f, 4.17f, 4.17f, 4.16f, 4.17f, 4.17f, 4.16f, 4.17f, 4.17f,
            4.16f, 4.17f, 4.17f, 4.16f, 4.17f, 4.17f, 4.16f, 4.17f, 4.17f, 4.16f
        };

        private static readonly float[] _lateGrowth = new[] {
            2.63f, 2.77f, 2.90f, 3.03f, 3.17f, 3.30f, 3.43f, 3.57f, 3.70f, 3.83f, 3.97f, 4.10f, 4.23f, 4.37f,
            4.50f, 4.63f, 4.77f, 4.90f, 5.03f, 5.17f, 5.30f, 5.43f, 5.57f, 5.70f
        };

        private static readonly float[] _earlyGrowth = new[] {
            5.70f, 5.57f, 5.43f, 5.30f, 5.17f, 5.03f, 4.90f, 4.77f, 4.63f, 4.50f, 4.37f, 4.23f, 4.10f, 3.97f,
            3.83f, 3.70f, 3.57f, 3.43f, 3.30f, 3.17f, 3.03f, 2.90f, 2.77f, 2.63f
        };

        private static readonly float[] _middleGrowth = new[] {
            2.5f, 2.75f, 3.05f, 3.35f, 3.65f, 3.95f, 4.25f, 4.55f, 4.85f, 5.15f, 5.45f, 5.45f, 5.45f, 5.45f,
            5.2f, 4.95f, 4.7f, 4.45f, 4.15f, 3.9f, 3.65f, 3.35f, 3.05f, 2.75f
        };

        private static readonly float[] _earlyLateGrowth = new[] {
            5.5f, 5.25f, 5f, 4.75f, 4.5f, 4.25f, 4f, 3.75f, 3.5f, 3.25f, 3.05f, 2.85f, 2.85f, 2.85f, 3.05f,
            3.25f, 3.45f, 3.65f, 3.85f, 4.05f, 4.25f, 4.5f, 4.7f, 4.9f
        };

        public static int GetStatIncrease(int minLevel, int maxLevel, int currentBaseStat, int currentLevel, EnumStatGrowth statGrowth) {
            if (currentLevel >= 25) {
                return Random.Range(1,3);
            }

            var statBasis = maxLevel - minLevel;
            var growth = new float[24];
            switch (statGrowth) {
                case EnumStatGrowth.Linear:
                    growth = _linearGrowth;
                    break;
                case EnumStatGrowth.Early:
                    growth = _earlyGrowth;
                    break;
                case EnumStatGrowth.Late:
                    growth = _lateGrowth;
                    break;
                case EnumStatGrowth.Middle:
                    growth = _middleGrowth;
                    break;
                case EnumStatGrowth.EarlyLate:
                    growth = _earlyLateGrowth;
                    break;
                default:
                    growth = _linearGrowth;
                    break;
            }

            var growthForLevel = growth[currentLevel - 1];
            var statIncreaseFloat = (float)statBasis / 100 * growthForLevel;
            statIncreaseFloat += Random.Range(0f, 1f);
            var increase = (int) statIncreaseFloat;
            currentBaseStat += increase;

            var shouldBePercent = 0f;
            for (int i = 0; i < currentLevel; i++) {
                shouldBePercent += growth[i];
            }

            var statShouldAtLeastBe = (int) ((float)statBasis / 100 * shouldBePercent) + minLevel;
            if (currentBaseStat < statShouldAtLeastBe) {
                increase += 1;
            }

            return increase;
        }

    }
}
