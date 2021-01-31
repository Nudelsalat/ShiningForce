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

        gameObject.transform.Find("Name").GetComponent<Text>().text = member.name;
        gameObject.transform.Find("Class").GetComponent<Text>().text = Enum.GetName(typeof(ClassType), member.classType);
        gameObject.transform.Find("Level").GetComponent<Text>().text = member.charStats.level.ToString();
        gameObject.transform.Find("EXP").GetComponent<Text>().text = member.charStats.exp.ToString();
        gameObject.transform.Find("HP").GetComponent<Text>().text = member.charStats.maxHp.ToString();
        gameObject.transform.Find("MP").GetComponent<Text>().text = member.charStats.maxMp.ToString();
        gameObject.transform.Find("Attack").GetComponent<Text>().text = member.charStats.attack.ToString();
        gameObject.transform.Find("Defense").GetComponent<Text>().text = member.charStats.defense.ToString();
        gameObject.transform.Find("Agility").GetComponent<Text>().text = member.charStats.agility.ToString();
        gameObject.transform.Find("Move").GetComponent<Text>().text = member.charStats.movement.ToString();
    }
}
