using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class Character {
    public int Id;
    public string Name;
    public Sprite PortraitSprite;
    public GameItem[] PartyMemberInventory = new GameItem[4];

    public EnumClassType ClassType = EnumClassType.SDMN;
    public EnumCharacterType CharacterType;
    public CharacterStatistics CharStats;
    public EnumStatusEffect StatusEffects = EnumStatusEffect.none;


    public void RemoveItem(GameItem item) {
        PartyMemberInventory[(int)item.positionInInventory] = Object.Instantiate(Resources.Load<GameItem>(Constants.PathEmptyItem));
    }
    public Equipment GetCurrentEquipment(EnumEquipmentType equipmentType) {
        return (Equipment) PartyMemberInventory.FirstOrDefault(x => x.EnumItemType == EnumItemType.equipment
                                                        && ((Equipment)x).EquipmentType == equipmentType
                                                        && ((Equipment)x).IsEquipped);
    }
}


