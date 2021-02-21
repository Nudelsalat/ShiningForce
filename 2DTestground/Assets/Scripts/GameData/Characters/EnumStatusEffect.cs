using System;

[Flags] public enum EnumStatusEffect {
    none = 0,
    poisoned = 1,
    confused = 2,
    slowed = 4,
    silent = 8,
    sleep = 16,
    boosted = 32,
    cursed = 64,
    hasted = 128,
    paralysed = 256,
    notUsed = 512,
    dead = 1028,
}