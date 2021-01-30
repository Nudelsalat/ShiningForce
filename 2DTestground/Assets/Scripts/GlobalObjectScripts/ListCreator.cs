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

            float spawnY = count * 32;
            Vector3 pos = new Vector3(0, -spawnY, SpawnPoint.position.z);
            GameObject SpawnedItem = Instantiate(item, pos, SpawnPoint.rotation);
            SpawnedItem.transform.SetParent(SpawnPoint, false);
            ConvertStatsToMenuList itemDetails = SpawnedItem.GetComponent<ConvertStatsToMenuList>();
            itemDetails.DoConvert(partyMember);
            count++;
        }
    }

    public void WhipeCharacterList() {
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
