using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public Transform SpawnPoint = null;
    public Transform HeaderSpawnPoint = null;
    public RectTransform content = null;
    public Scrollbar scrollbar = null;

    private Animator _characterSelector;

    public static CharacterSelector Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        _characterSelector = transform.GetComponent<Animator>();
        transform.gameObject.SetActive(false);
    }


    // Use this for initialization
    public void LoadCharacterList(List<PartyMember> party, Equipment equipmentItem, int currentlySelectedListItem) {
        ClearCharacterList();
        transform.gameObject.SetActive(true);
        _characterSelector.SetBool("characterSelectorIsOpen", true);

        GameObject item;
        GameObject header;

        switch (equipmentItem != null) {
            case true:
                item = Resources.Load(Constants.PrefabListCharacterEquipment) as GameObject;
                header = Resources.Load(Constants.PrefabHeaderEquipment) as GameObject;
                break;
            case false:
                default:
                item = Resources.Load(Constants.PrefabListCharacterObject) as GameObject;
                header = Resources.Load(Constants.PrefabHeader) as GameObject;
                break;

        }

        var spawnedHeader = Instantiate(header, new Vector3(0,0, HeaderSpawnPoint.position.z), HeaderSpawnPoint.rotation);
        spawnedHeader.transform.SetParent(HeaderSpawnPoint, false);

        //setContent Holder Height;
        content.sizeDelta = new Vector2(0, party.Count * 32 + 10);
        int count = 0;
        foreach (var partyMember in party) {

            var spawnY = count * 32;
            var pos = new Vector3(0, -spawnY, SpawnPoint.position.z);
            var spawnedItem = Instantiate(item, pos, SpawnPoint.rotation);
            spawnedItem.transform.SetParent(SpawnPoint, false);
            if (equipmentItem == null) {
                var itemDetails = spawnedItem.GetComponent<ConvertStatsToMenuList>();
                itemDetails.DoConvert(partyMember);
            }
            else {
                var itemDetails = spawnedItem.GetComponent<ConvertEquipmentStatsToMenuList>();
                itemDetails.DoConvert(partyMember, equipmentItem);
            }
            count++;
        }
        SetScrollbar(currentlySelectedListItem, currentlySelectedListItem, party.Count);
    }

    public void ClearCharacterList() {
        foreach (Transform child in HeaderSpawnPoint) {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in SpawnPoint) {
            GameObject.Destroy(child.gameObject);
        }
        _characterSelector.SetBool("characterSelectorIsOpen", false);
        transform.gameObject.SetActive(false);
    }
    public void SetScrollbar(int previous, int target, int qty) {
        scrollbar.value =  1.0f - ((float)target / (qty-1.0f));
        DrawBoundary(previous, target);
    }

    public void DrawBoundary(int oldTarget, int newTarget) {
        //This is so dump. Destroy takes to long, so when we want to select the boarder of the new list
        //the old list elements still exist.
        StartCoroutine(WaitOneFrame(oldTarget, newTarget));
    }
    
    IEnumerator WaitOneFrame(int oldTarget, int newTarget) {
        //returning 0 will make it wait 1 frame
        yield return 0;

        var oldSelectedChild = SpawnPoint.GetChild(oldTarget);
        oldSelectedChild.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.0f);

        var newSelectedChild = SpawnPoint.GetChild(newTarget);
        newSelectedChild.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.70f);
    }
}

public enum CharacterListType{
    stats,
    equipment,
}
