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
        gameObject.transform.Find("Name").GetComponent<Text>().color = 
            member.StatusEffects.HasFlag(EnumStatusEffect.dead)
                ? Constants.Orange
                : Constants.Visible;
        if (equipment.EquipmentForClass.All(x => x != member.ClassType)) {
            gameObject.transform.Find("Unequippable/Attack").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Defense").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Agility").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable/Move").GetComponent<Text>().text = "";
            gameObject.transform.Find("Unequippable").GetComponent<Text>().text = "not possible to equip";

        } else {
            var oldEquipment = member.GetCurrentEquipment(equipment.EquipmentType);

            gameObject.transform.Find("Unequippable/Attack").GetComponent<Text>().text = 
                member.CharStats.Attack.GetModifiedValue().ToString().PadLeft(3) 
                + "→"
                + member.CharStats.CalculateNewAttack(equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Defense").GetComponent<Text>().text = 
                member.CharStats.Defense.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + member.CharStats.CalculateNewDefense(equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Agility").GetComponent<Text>().text = 
                member.CharStats.Agility.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + member.CharStats.CalculateNewAgility(equipment, oldEquipment).ToString().PadLeft(3);

            gameObject.transform.Find("Unequippable/Move").GetComponent<Text>().text = 
                member.CharStats.Movement.GetModifiedValue().ToString().PadLeft(3)
                + "→"
                + member.CharStats.CalculateNewMovement(equipment, oldEquipment).ToString().PadLeft(3);

        }
    }
}
