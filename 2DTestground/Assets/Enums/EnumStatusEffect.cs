using System;

[Flags] public enum EnumStatusEffect {
    none = 0,
    poisoned = 1 << 0,
    confused = 1 << 1,
    slowed = 1 << 2,
    silent = 1 << 3,
    sleep = 1 << 4,
    boosted = 1 << 5,
    cursed = 1 << 6,
    hasted = 1 << 7,
    paralyzed = 1 << 8,
    notUsed = 1 << 9,
    dead = 1 << 10,
}