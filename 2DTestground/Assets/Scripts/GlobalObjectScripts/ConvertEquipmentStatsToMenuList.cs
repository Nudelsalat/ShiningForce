using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class ConvertEquipmentStatsToMenuList : MonoBehaviour {

    private Sprite _isActiveImage;

    void Awake() {
        
    }

    public void DoConvert(PartyMember member, Equipment equipment) {
        var image = gameObject.transform.Find("Image").GetComponent<Image>();
        image.color = member.activeParty ? Constants.Visible : Constants.Invisible;

        gameObject.transform.Find("Name").GetComponent<Text>().text = member.Name;

        if (equipment.EquipmentForClass.All(x => x != member.ClassType)) {
            gameObject.transform.Find("Unequippable/Attack").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Defense").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Agility").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Move").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable").GetComponent<Text>().text = "not possible to equip";

        } else {
            var oldEquipment = (Equipment)
                member.PartyMemberInventory.FirstOrDefault(x => x.EnumItemType == EnumItemType.equipment
                                                                && ((Equipment)x).EquipmentType == equipment.EquipmentType
                                                                && ((Equipment)x).IsEquipped);

            gameObject.transform.Find("Unequippable/Attack").GetComponent<Text>().text = 
                member.CharStats.Attack.GetModifiedValue().ToString().PadLeft(3) 
                + "→"
                + CalculateNewAttack(member,equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Defense").GetComponent<Text>().text = 
                member.CharStats.Defense.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + CalculateNewDefense(member, equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Agility").GetComponent<Text>().text = 
                member.CharStats.Agility.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + CalculateNewAgility(member, equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Move").GetComponent<Text>().text = 
                member.CharStats.Movement.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + CalculateNewMovement(member, equipment, oldEquipment).ToString().PadLeft(3);

        }
    }

    private int CalculateNewAttack(PartyMember member, Equipment newEquipment, Equipment oldEquipment) {
        if (oldEquipment != null) {
            return member.CharStats.Attack.GetModifiedValue() + newEquipment.AttackModifier - oldEquipment.AttackModifier;
        }
        return member.CharStats.Attack.GetModifiedValue() + newEquipment.AttackModifier;
    }
    private int CalculateNewDefense(PartyMember member, Equipment newEquipment, Equipment oldEquipment) {
        if (oldEquipment != null) {
            return member.CharStats.Defense.GetModifiedValue() + newEquipment.DefenseModifier - oldEquipment.DefenseModifier;
        }
        return member.CharStats.Defense.GetModifiedValue() + newEquipment.DefenseModifier;
    }
    private int CalculateNewAgility(PartyMember member, Equipment newEquipment, Equipment oldEquipment) {
        if (oldEquipment != null) {
            return member.CharStats.Agility.GetModifiedValue() + newEquipment.AgilityModifier - oldEquipment.AgilityModifier;
        }
        return member.CharStats.Agility.GetModifiedValue() + newEquipment.AgilityModifier;
    }
    private int CalculateNewMovement(PartyMember member, Equipment newEquipment, Equipment oldEquipment) {
        if (oldEquipment != null) {
            return member.CharStats.Movement.GetModifiedValue() + newEquipment.MovementModifier - oldEquipment.MovementModifier;
        }
        return member.CharStats.Movement.GetModifiedValue() + newEquipment.MovementModifier;
    }
}
