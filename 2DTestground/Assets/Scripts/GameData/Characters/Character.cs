using System;
using System.Collections;
using System.Linq;
using Assets.Enums;
using Assets.Scripts.GameData.Characters;
using Assets.Scripts.GameData.Magic;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(fileName = "Monster", menuName = "Character/new Monster")]
public class Character : ScriptableObject {
    public int Id;
    public string Name;
    public bool IsPromoted;
    public Sprite PortraitSprite;
    public RuntimeAnimatorController AnimatorSprite;

    public EnumClassType ClassType = EnumClassType.SDMN;
    public EnumCharacterType CharacterType;
    public EnumMovementType MovementType;
    public CharacterStatistics CharStats;
    public EnumStatusEffect StatusEffects = EnumStatusEffect.none;

    [SerializeField]
    private Tuple<GameItem, bool>[] CharacterInventory = new Tuple<GameItem, bool>[4];
    [SerializeField]
    private Tuple<Magic, int>[] Magic = new Tuple<Magic, int>[4];

    private GameItem[] _characterInventory = new GameItem[4];
    private Magic[] _magic = new Magic[4];

    void OnEnable() {
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

    public void RemoveItem(GameItem item) {
        if (item.IsUnique) {
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

