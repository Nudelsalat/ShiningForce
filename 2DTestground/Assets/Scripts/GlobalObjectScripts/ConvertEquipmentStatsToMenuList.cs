using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class ConvertEquipmentStatsToMenuList : MonoBehaviour {

    private Sprite _isActiveImage;

    private Text _hp;
    private Text _mp;
    private Text _attack;
    private Text _defense;
    private Text _agility;
    private Text _movement;


    void Awake() {
        _hp = gameObject.transform.Find("Unequippable/Hp").GetComponent<Text>();
        _mp = gameObject.transform.Find("Unequippable/Mp").GetComponent<Text>();
        _attack = gameObject.transform.Find("Unequippable/Attack").GetComponent<Text>();
        _defense = gameObject.transform.Find("Unequippable/Defense").GetComponent<Text>();
        _agility = gameObject.transform.Find("Unequippable/Agility").GetComponent<Text>();
        _movement = gameObject.transform.Find("Unequippable/Move").GetComponent<Text>();
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
            _hp.text = "";
            _mp.text = "";
            _attack.text = "";
            _defense.text = "";
            _agility.text = "";
            _movement.text = "";
            gameObject.transform.Find("Unequippable").GetComponent<Text>().text = "not possible to equip";

        } else {
            var oldEquipment = member.GetCurrentEquipment(equipment.EquipmentType);
            var currentHp = member.CharStats.Hp.GetModifiedValue();
            var newHp = member.CharStats.CalculateNewHp(equipment, oldEquipment);

            var currentMp = member.CharStats.Mp.GetModifiedValue();
            var newMp = member.CharStats.CalculateNewMp(equipment, oldEquipment);

            var currentAttack = member.CharStats.Attack.GetModifiedValue();
            var newAttack = member.CharStats.CalculateNewAttack(equipment, oldEquipment);

            var currentDefense = member.CharStats.Defense.GetModifiedValue();
            var newDefense = member.CharStats.CalculateNewDefense(equipment, oldEquipment);

            var currentAgility = member.CharStats.Agility.GetModifiedValue();
            var newAgility = member.CharStats.CalculateNewAgility(equipment, oldEquipment);

            var currentMovement = member.CharStats.Movement.GetModifiedValue();
            var newMovement = member.CharStats.CalculateNewMovement(equipment, oldEquipment);

            _hp.text = currentHp.ToString().PadLeft(3) + "→" + newHp.ToString().PadLeft(3);
            _hp.color = currentHp > newHp ? Color.red : currentHp < newHp ? Color.green : Constants.Visible;

            _mp.text = currentMp.ToString().PadLeft(3) + "→" + newMp.ToString().PadLeft(3);
            _mp.color = currentMp > newMp ? Color.red : currentMp < newMp ? Color.green : Constants.Visible;

            _attack.text = currentAttack.ToString().PadLeft(3) + "→" + newAttack.ToString().PadLeft(3);
            _attack.color = currentAttack > newAttack ? Color.red : currentAttack < newAttack ? Color.green : Constants.Visible;

            _defense.text = currentDefense.ToString().PadLeft(3) + "→" + newDefense.ToString().PadLeft(3);
            _defense.color = currentDefense > newDefense ? Color.red : currentDefense < newDefense ? Color.green : Constants.Visible;

            _agility.text = currentAgility.ToString().PadLeft(3) + "→" + newAgility.ToString().PadLeft(3);
            _agility.color = currentAgility > newAgility ? Color.red : currentAgility < newAgility ? Color.green : Constants.Visible;

            _movement.text = currentMovement.ToString().PadLeft(3) + "→" + newMovement.ToString().PadLeft(3);
            _movement.color = currentMovement > newMovement ? Color.red : currentMovement < newMovement ? Color.green : Constants.Visible;

        }
    }
}
