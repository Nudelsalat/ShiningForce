using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class ConvertStatsToMenuList : MonoBehaviour {

    private Sprite _isActiveImage;

    void Awake() {
        
    }

    public void DoConvert(PartyMember member) {
        var image = gameObject.transform.Find("Image").GetComponent<Image>();
        image.color = member.activeParty ? Constants.Visible : Constants.Invisible;
        
        gameObject.transform.Find("Name").GetComponent<Text>().text = member.Name;
        gameObject.transform.Find("Name").GetComponent<Text>().color =
            member.StatusEffects.HasFlag(EnumStatusEffect.dead) 
                ? Constants.Orange 
                : Constants.Visible;
        gameObject.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), member.ClassType);
        gameObject.transform.Find("Level").GetComponent<Text>().text = member.CharStats.Level.ToString();
        gameObject.transform.Find("EXP").GetComponent<Text>().text = member.CharStats.Exp.ToString();
        gameObject.transform.Find("HP").GetComponent<Text>().text = member.CharStats.CurrentHp.ToString();
        gameObject.transform.Find("MP").GetComponent<Text>().text = member.CharStats.CurrentMp.ToString();
        gameObject.transform.Find("Attack").GetComponent<Text>().text = member.CharStats.Attack.GetModifiedValue().ToString();
        gameObject.transform.Find("Defense").GetComponent<Text>().text = member.CharStats.Defense.GetModifiedValue().ToString();
        gameObject.transform.Find("Agility").GetComponent<Text>().text = member.CharStats.Agility.GetModifiedValue().ToString();
        gameObject.transform.Find("Move").GetComponent<Text>().text = member.CharStats.Movement.GetModifiedValue().ToString();
    }
}
