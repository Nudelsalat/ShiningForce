using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.HelperScripts;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Character : ScriptableObject {
    public int Id;
    public string Name;
    public bool IsForce;
    public bool IsPromoted;
    public Sprite PortraitSprite;
    public RuntimeAnimatorController AnimatorSprite;
    [SerializeField]
    private string _pathToBattleAnimationPrefab;
    public Texture2D ColorPaletteFieldAnimation;
    public Texture2D ColorPaletteBattleAnimation;
    public int SkinId;

    public EnumVoicePitch Voice = EnumVoicePitch.middle;
    public EnumClassType ClassType = EnumClassType.SDMN;
    public EnumCharacterType CharacterType;
    public EnumMovementType MovementType;
    public CharacterStatistics CharStats;
    public EnumStatusEffect StatusEffects = EnumStatusEffect.none;
    public EnumStatusEffect StatusImmunities = EnumStatusEffect.none;
    public EnumElementalResistanceWeakness ElementalWeakness = EnumElementalResistanceWeakness.none;
    public EnumElementalResistanceWeakness ElementalResistance = EnumElementalResistanceWeakness.none;
    public float CritDamageMultiplier = 1.25f;
    public EnumChance CritChance = EnumChance.OneIn16;
    public EnumChance CounterChance = EnumChance.OneIn32;
    public EnumChance DoubleAttackChance = EnumChance.OneIn32;
    public EnumChance DodgeBaseChance = EnumChance.OneIn32;
    public EnumChance InflictStatusEffectChance = EnumChance.Never;
    public EnumStatusEffect InflictStatusEffect = EnumStatusEffect.none;
    public MagicLevelUpAtLevel[] SpellLevelUps;

    [SerializeField]
    private Tuple<GameItem, bool>[] CharacterInventory = new Tuple<GameItem, bool>[4];
    [SerializeField]
    private Tuple<Magic, int>[] Magic = new Tuple<Magic, int>[4];

    private GameItem[] _characterInventory = new GameItem[4];
    private Magic[] _magic = new Magic[4];

    private float _sleepWakeUpChance = 0f;

    void OnEnable() {
        CharStats.ClearModifiers();
        for (int i = 0; i < CharacterInventory.Length; i++) {
            if (CharacterInventory[i] == null || CharacterInventory[i].Item1 == null) {
                _characterInventory[i] = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
                continue;
            }
            var addItem = Object.Instantiate(CharacterInventory[i].Item1);
            if (addItem is Equipment equipment) {
                if (CharacterInventory[i].Item2) {
                    CharStats.Equip(equipment);
                }
            }
            _characterInventory[i] = addItem;
        }

        for (int i = 0; i < Magic.Length; i++) {
            if (Magic[i] == null || Magic[i].Item1 == null) {
                _magic[i] = Object.Instantiate(Resources.Load<Magic>(Constants.MagicEmpty));
                continue;
            }

            var addMagic = Object.Instantiate(Magic[i].Item1);
            addMagic.CurrentLevel = Magic[i].Item2;
            _magic[i] = addMagic;
        }
    }

    public void RemoveItem(GameItem item, bool addToDealsIfUnique = true) {
        if (item.IsUnique && addToDealsIfUnique) {
            Inventory.Instance.AddToDeals(item);
        }
        var itemToDrop = _characterInventory[(int) item.PositionInInventory];
        if (itemToDrop is Equipment equipment) {
            if (equipment.IsEquipped) {
                CharStats.UnEquip(equipment);
            }
        }

        _characterInventory[(int)item.PositionInInventory] = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
    }
    public Equipment GetCurrentEquipment(EnumEquipmentType equipmentType) {
        return (Equipment) _characterInventory.FirstOrDefault(x => x.EnumItemType == EnumItemType.equipment
                                                        && ((Equipment)x).EquipmentType == equipmentType
                                                        && ((Equipment)x).IsEquipped);
    }

    public GameItem[] GetInventory() {
        return _characterInventory;
    }

    public Magic[] GetMagic() {
        return _magic;
    }

    public EnumAttackRange GetAttackRange() {
        var weapon = GetCurrentEquipment(EnumEquipmentType.weapon);
        if (weapon == null) {
            return EnumAttackRange.Melee;
        }
        return weapon.AttackRange;
    }
    
    public EnumAreaOfEffect GetAttackAreaOfEffect() {
        var weapon = GetCurrentEquipment(EnumEquipmentType.weapon);
        if (weapon == null) {
            return EnumAreaOfEffect.Single;
        }
        return weapon.AreaOfEffect;
    }

    public string GetBattleAnimationPath() {
        return string.IsNullOrEmpty(_pathToBattleAnimationPrefab) ? "SharedObjects/Prefab/Battle/Animations/Enemies/Dwarf" : _pathToBattleAnimationPrefab;
    }

    public void FullyHeal() {
        CharStats.CurrentHp = CharStats.MaxHp();
        CharStats.CurrentMp = CharStats.MaxMp();
    }

    public List<string> AddExp(int expToAdd) {
        var sentencesToReturn = new List<string>();
        sentencesToReturn.Add($"{Name.AddColor(Constants.Orange)} gained {expToAdd} EXP.");
        if (!CharStats.AddExp(expToAdd)) {
            return sentencesToReturn;
        }
        var sentences = CharStats.LevelUp();
        sentencesToReturn.Add($"{Name.AddColor(Constants.Orange)} reached level {CharStats.Level}");
        sentencesToReturn.Add(sentences);

        sentencesToReturn.AddRange(LevelUpMagic());

        return sentencesToReturn;
    }

    public bool CheckWakeup() {
        var sleepRoll = Random.Range(0f, 1f);
        var result = sleepRoll < _sleepWakeUpChance;
        Debug.Log($"WakupChance: {_sleepWakeUpChance}, wakupRoll: {sleepRoll}");
        if (result) {
            _sleepWakeUpChance = 0f;
            StatusEffects = StatusEffects.Remove(EnumStatusEffect.asleep);
        } else {
            _sleepWakeUpChance += 0.3334f;
        }
        return result;
    }

    private List<string> LevelUpMagic() {
        var sentences = new List<string>();
        var currentLevel = IsPromoted ? CharStats.Level + 20 : CharStats.Level;
        var relevantNewMagic = SpellLevelUps.Where(x => x.Level == currentLevel);

        foreach (var newMagic in relevantNewMagic) {
            var magicType = newMagic.Magic;
            if (_magic.Any(x => x.SpellName.Equals(magicType.SpellName))) {
                var currentMagic = _magic.SingleOrDefault(x => x.SpellName.Equals(magicType.SpellName));
                if (currentMagic != null && currentMagic.CurrentLevel < newMagic.MagicLevel) {
                    currentMagic.CurrentLevel = newMagic.MagicLevel;
                    sentences.Add($"{Name.AddColor(Constants.Orange)} learned {newMagic.Magic.SpellName.AddColor(Constants.Violet)} " +
                                  $"level {newMagic.MagicLevel}");
                }
            } else {
                for (var i = 0; i < 4; i++) {
                    if (_magic[i].IsEmpty()) {
                        _magic[i] = newMagic.Magic;
                        _magic[i].CurrentLevel = newMagic.MagicLevel;
                        sentences.Add($"{Name.AddColor(Constants.Orange)} learned {newMagic.Magic.SpellName.AddColor(Constants.Violet)} " +
                                      $"level {newMagic.MagicLevel}");
                        break;
                    }
                }
            }
        }
        return sentences;
    }

}

[Serializable]
public struct MagicLevelUpAtLevel {
    public int Level;
    public Magic Magic;
    public int MagicLevel;
}

[Serializable]
public class Tuple<T1, T2> : IEquatable<Tuple<T1,T2>> {
    [SerializeField]
    private T1 item1;

    [SerializeField]
    private T2 item2;

    public T1 Item1 => item1;
    public T2 Item2 => item2;

    public Tuple(T1 item1, T2 item2) {
        this.item1 = item1;
        this.item2 = item2;
    }

    public override int GetHashCode() {
        return item1.GetHashCode() ^ item2.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        return Equals((Tuple<T1, T2>)obj);
    }

    public bool Equals(Tuple<T1, T2> other) {
        return other.item1.Equals(item1) && other.item2.Equals(item2);
    }
}

