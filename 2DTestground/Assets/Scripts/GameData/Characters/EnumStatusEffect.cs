using System;

[Flags] public enum EnumStatusEffect {
    none = 0,
    poisoned = 1,
    muddled = 2,
    stunned = 4,
    dead = 8,
}