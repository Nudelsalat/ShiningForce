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
        if (member.activeParty) {
            image.color = new Color(1, 1, 1, 1);
        }
        else {
            image.color = new Color(1,1,1,0);
        }

        gameObject.transform.Find("Name").GetComponent<Text>().text = member.Name;
        gameObject.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(EnumClassType), member.ClassType);
        gameObject.transform.Find("Level").GetComponent<Text>().text = member.CharStats.Level.ToString();
        gameObject.transform.Find("EXP").GetComponent<Text>().text = member.CharStats.Exp.ToString();
        gameObject.transform.Find("HP").GetComponent<Text>().text = member.CharStats.MaxHp.ToString();
        gameObject.transform.Find("MP").GetComponent<Text>().text = member.CharStats.MaxMp.ToString();
        gameObject.transform.Find("Attack").GetComponent<Text>().text = member.CharStats.Attack.GetModifiedValue().ToString();
        gameObject.transform.Find("Defense").GetComponent<Text>().text = member.CharStats.Defense.GetModifiedValue().ToString();
        gameObject.transform.Find("Agility").GetComponent<Text>().text = member.CharStats.Agility.GetModifiedValue().ToString();
        gameObject.transform.Find("Move").GetComponent<Text>().text = member.CharStats.Movement.GetModifiedValue().ToString();
    }
}
