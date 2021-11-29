using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameSelector : MonoBehaviour
{
    public Transform SpawnPoint = null;
    public RectTransform content = null;
    public Scrollbar scrollbar = null;

    private FileInfo[] _saveStates;
    private int _currentSelected;
    private int _quantity;

    public static LoadGameSelector Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        string path = Application.persistentDataPath + "/";
        DirectoryInfo d = new DirectoryInfo(path);

        _saveStates = d.GetFiles("*.dat");
        _quantity = _saveStates.Length;
        transform.gameObject.SetActive(true);
        LoadCharacterList(0);
    }

    void Update() {
        if (Input.GetButtonDown("Vertical")) {
            if (Input.GetAxisRaw("Vertical") > 0.05f) {
                _currentSelected = _currentSelected == 0 ? _currentSelected : _currentSelected - 1;
                if (_currentSelected <= _quantity) {
                    SetScrollbar();
                }
            }
            else if (Input.GetAxisRaw("Vertical") < -0.05f) {
                _currentSelected = _currentSelected == _quantity ? _currentSelected : _currentSelected + 1;
                if (_currentSelected <= _quantity - 1) {
                    SetScrollbar();
                }
            }
        }
    }

    private void OnEnable() {
        StartCoroutine(WaitForButton());
    }

    void Start() {
        var child = SpawnPoint.transform.GetChild(0);
        if (child) {
            child.GetComponent<Button>().Select();
        }
    }

    // Use this for initialization
    public void LoadCharacterList(int currentlySelectedListItem) {
        ClearCharacterList();

        GameObject item = Resources.Load(Constants.PrefabTextElement) as GameObject;
        //setContent Holder Height;
        content.sizeDelta = new Vector2(0, _quantity * 42 + 10);
        int count = 0;
        foreach (var gameName in _saveStates) {

            var spawnY = count * 42;
            var pos = new Vector3(0, -spawnY, SpawnPoint.position.z);
            var spawnedItem = Instantiate(item, pos, SpawnPoint.rotation);
            spawnedItem.transform.SetParent(SpawnPoint, false);
            spawnedItem.transform.Find("Text").GetComponent<Text>().text = gameName.Name.Replace(".dat","");
            if (count == 0) {
                spawnedItem.GetComponent<Button>().Select();
                _currentSelected = 0;
            }

            count++;
        }
        SetScrollbar();
    }

    public void ClearCharacterList() {
        foreach (Transform child in SpawnPoint) {
            GameObject.Destroy(child.gameObject);
        }
    }
    public void SetScrollbar() {
        scrollbar.value =  1.0f - ((float)_currentSelected / (_quantity - 1.0f));
    }
    IEnumerator WaitForButton() {
        yield return null;
        Start();
    }
}
