using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandeffectUi : MonoBehaviour {

    private Text _terrainEffect;
    private Animator _menuAnimator;
    private bool _showUi;

    public static LandeffectUi Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        _terrainEffect = transform.Find("TerrainEffect").GetComponent<Text>();
        _menuAnimator = transform.GetComponent<Animator>();
    }

    void Start() {
        transform.gameObject.SetActive(false);
    }

    public void ShowLandEffect(int terrainValue) {
        _showUi = true;
        transform.gameObject.SetActive(true);
        _menuAnimator.SetBool("isOpen", true);
        _terrainEffect.text = $"{terrainValue}%";
    }

    public void CloseLandEffect() {
        _menuAnimator.SetBool("isOpen", false);
        _showUi = false;
        StartCoroutine(WaitForTenthASecond());
    }

    IEnumerator WaitForTenthASecond() {
        yield return new WaitForSeconds(0.1f);
        if (!_showUi) {
            transform.gameObject.SetActive(false);
        }
    }
}
