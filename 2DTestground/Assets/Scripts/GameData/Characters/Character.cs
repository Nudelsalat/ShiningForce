using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class Character {
    public int Id;
    public string Name;
    public Sprite PortraitSprite;
    public GameItem[] CharacterInventory = new GameItem[4];
    public Magic[] Magic = new Magic[4];
    public AnimatorController AnimatorSprite;

    public int Kills = 0;
    public int Defeats = 0;
    public EnumClassType ClassType = EnumClassType.SDMN;
    public EnumCharacterType CharacterType;
    public CharacterStatistics CharStats;
    public EnumStatusEffect StatusEffects = EnumStatusEffect.none;


    public void RemoveItem(GameItem item) {
        CharacterInventory[(int)item.positionInInventory] = Object.Instantiate(Resources.Load<GameItem>(Constants.ItemEmptyItem));
    }
    public Equipment GetCurrentEquipment(EnumEquipmentType equipmentType) {
        return (Equipment) CharacterInventory.FirstOrDefault(x => x.EnumItemType == EnumItemType.equipment
                                                        && ((Equipment)x).EquipmentType == equipmentType
                                                        && ((Equipment)x).IsEquipped);
    }
}


