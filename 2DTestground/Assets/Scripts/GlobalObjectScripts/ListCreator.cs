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
}
