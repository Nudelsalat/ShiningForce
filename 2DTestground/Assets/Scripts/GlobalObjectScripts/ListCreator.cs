using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListCreator : MonoBehaviour
{
    public Transform SpawnPoint = null;
    public GameObject item = null;
    public RectTransform content = null;
    public Scrollbar scrollbar = null;

    // Use this for initialization
    public void LoadCharacterList(List<PartyMember> party) {

        //setContent Holder Height;
        content.sizeDelta = new Vector2(0, party.Count * 32);
        int count = 0;
        foreach (var partyMember in party) {

            var spawnY = count * 32;
            var pos = new Vector3(0, -spawnY, SpawnPoint.position.z);
            var spawnedItem = Instantiate(item, pos, SpawnPoint.rotation);
            spawnedItem.transform.SetParent(SpawnPoint, false);
            var itemDetails = spawnedItem.GetComponent<ConvertStatsToMenuList>();
            itemDetails.DoConvert(partyMember);
            count++;
        }
    }

    public void ClearCharacterList() {
        foreach (Transform child in SpawnPoint) {
            GameObject.Destroy(child.gameObject);
        }
    }
    public void SetScrollbar(int previous, int target, int qty) {
        scrollbar.value =  1.0f - ((float)target / (qty-1.0f));
        DrawBoundary(previous, target);
    }
    public void DrawBoundary(int oldTarget, int newTarget) {

        var oldSelectedChild = SpawnPoint.GetChild(oldTarget);
        oldSelectedChild.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.0f);

        var newSelectedChild = SpawnPoint.GetChild(newTarget);
        newSelectedChild.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.70f);

        
    }

}
