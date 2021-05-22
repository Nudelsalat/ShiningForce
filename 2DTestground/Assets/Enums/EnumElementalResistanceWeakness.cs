using System;

[Flags] public enum EnumElementalResistanceWeakness {
    none = 0,
    Fire25 = 1 << 0,
    Fire50 = 1 << 1,
    Fire75 = 1 << 2,
    Cold25 = 1 << 3,
    Cold50 = 1 << 4,
    Cold75 = 1 << 5,
    Wind25 = 1 << 6,
    Wind50 = 1 << 7,
    Wind75 = 1 << 8,
    Bolt25 = 1 << 9,
    Bolt50 = 1 << 10,
    Bolt75 = 1 << 11,
}