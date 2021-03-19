using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class QuickInfoUi : MonoBehaviour
{
    /*private GameObject _nameTypeLevelGameObject;
    private GameObject _hpGameObject;
    private GameObject _mpGameObject;*/
    private Animator _menuAnimator;

    private Text _currentHP;
    private Text _maxHP;
    private Text _currentMP;
    private Text _maxMP;
    private Text _charName;
    private Text _charTypeLevel;

    private RectTransform _hpRectTransform;
    private RectTransform _hpContainerRectTransform;
    private RectTransform _mpRectTransform;
    private RectTransform _mpContainerRectTransform;
    private RectTransform _quickInfoRectTransform;

    private Transform _hpSliders;
    private Transform _mpSliders;

    private bool _showUi;
    private readonly int _uiMinSize = 90;
    private readonly int _uiMaxSize = 530;
    private readonly int _uiSizeHealthBarDifference = 210;
    private readonly float _uiSizePerPoint = 5.3f;


    public static QuickInfoUi Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        /*_nameTypeLevelGameObject = transform.Find("QuickInfo/Name").GetComponent<GameObject>();
        _hpGameObject = transform.Find("QuickInfo/HP").GetComponent<GameObject>();
        _mpGameObject = transform.Find("QuickInfo/MP").GetComponent<GameObject>();*/
        _quickInfoRectTransform = transform.Find("QuickInfo").GetComponent<RectTransform>();
        _menuAnimator = transform.GetComponent<Animator>();

        _currentHP = transform.Find("QuickInfo/HP/Values/CurrentHP").GetComponent<Text>();
        _currentMP = transform.Find("QuickInfo/MP/Values/CurrentMP").GetComponent<Text>();
        _maxHP = transform.Find("QuickInfo/HP/Values/MaxHP").GetComponent<Text>();
        _maxMP = transform.Find("QuickInfo/MP/Values/MaxMP").GetComponent<Text>();

        _charName = transform.Find("QuickInfo/Name/Name").GetComponent<Text>();
        _charTypeLevel = transform.Find("QuickInfo/Name/TypeLevel").GetComponent<Text>();

        _hpRectTransform = transform.Find("QuickInfo/HP/BarContainer/Bar").GetComponent<RectTransform>();
        _mpRectTransform = transform.Find("QuickInfo/MP/BarContainer/Bar").GetComponent<RectTransform>();
        _hpContainerRectTransform = transform.Find("QuickInfo/HP/BarContainer").GetComponent<RectTransform>();
        _mpContainerRectTransform = transform.Find("QuickInfo/MP/BarContainer").GetComponent<RectTransform>();

        _hpSliders = transform.Find("QuickInfo/HP/BarContainer/Bar").GetComponent<Transform>();
        _mpSliders = transform.Find("QuickInfo/MP/BarContainer/Bar").GetComponent<Transform>();
    }
    void Start() {
        transform.gameObject.SetActive(false);
    }

    public void ShowQuickInfo(Character character) {
        OpenUi();
        var stats = character.CharStats;
        float maxValue = stats.MaxHp > stats.MaxMp
            ? stats.MaxHp
            : stats.MaxMp;
        maxValue *= _uiSizePerPoint;

        if (maxValue >= _uiMaxSize) {
            maxValue = _uiMaxSize;
        } else if (maxValue <= _uiMinSize) {
            maxValue = _uiMinSize;
        }

        _quickInfoRectTransform.sizeDelta = 
            new Vector2(maxValue + _uiSizeHealthBarDifference, _quickInfoRectTransform.rect.height);
        _hpContainerRectTransform.sizeDelta =
            new Vector2(maxValue, _hpContainerRectTransform.rect.height);
        _mpContainerRectTransform.sizeDelta =
            new Vector2(maxValue, _mpContainerRectTransform.rect.height);

        var healthBarSize = (stats.MaxHp * _uiSizePerPoint > 530) ? 530 : stats.MaxHp * _uiSizePerPoint;
        var mpBarSize = (stats.MaxMp * _uiSizePerPoint > 530) ? 530 : stats.MaxMp * _uiSizePerPoint;
        _hpRectTransform.sizeDelta = new Vector2(healthBarSize, _hpRectTransform.rect.height);
        _mpRectTransform.sizeDelta = new Vector2(mpBarSize, _mpRectTransform.rect.height);

        _charName.text = character.Name;
        _charTypeLevel.text = $"{Enum.GetName(typeof(EnumClassType),character.ClassType)}{stats.Level}";

        _currentHP.text = stats.CurrentHp <= 999 ? stats.CurrentHp.ToString() : "???";
        _maxHP.text = stats.MaxHp <= 999 ? stats.MaxHp.ToString() : "???";
        _currentMP.text = stats.CurrentMp <= 999 ? stats.CurrentMp.ToString() : "???";
        _maxMP.text = stats.MaxMp <= 999 ? stats.MaxMp.ToString() : "???";

        SettingSliders(stats);

    }

    private void SettingSliders(CharacterStatistics stats) {
        var lowerHpValue = stats.MaxHp <= 100 ? stats.MaxHp : 100;
        var lowerMpValue = stats.MaxMp <= 100 ? stats.MaxMp : 100;

        foreach (Transform child in _hpSliders) {
            child.GetComponent<Slider>().value = 0;
        }
        foreach (Transform child in _mpSliders) {
            child.GetComponent<Slider>().value = 0;
        }

        if (stats.CurrentHp <= 100) {
            _hpSliders.GetChild(0).GetComponent<Slider>().value = (float)stats.CurrentHp / lowerHpValue;
        } else if (stats.CurrentHp <= 200) {
            _hpSliders.GetChild(0).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(1).GetComponent<Slider>().value = (float)stats.CurrentHp%100 / lowerHpValue;
        } else if (stats.CurrentHp <= 300) {
            _hpSliders.GetChild(1).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(2).GetComponent<Slider>().value = (float)stats.CurrentHp % 100 / lowerHpValue;
        } else if (stats.CurrentHp <= 400) {
            _hpSliders.GetChild(2).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(3).GetComponent<Slider>().value = (float)stats.CurrentHp % 100 / lowerHpValue;
        } else if (stats.CurrentHp <= 500) {
            _hpSliders.GetChild(3).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(4).GetComponent<Slider>().value = (float)stats.CurrentHp % 100 / lowerHpValue;
        } else if (stats.CurrentHp <= 600) {
            _hpSliders.GetChild(4).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(5).GetComponent<Slider>().value = (float)stats.CurrentHp % 100 / lowerHpValue;
        } else {
            _hpSliders.GetChild(5).GetComponent<Slider>().value = 1;
            _hpSliders.GetChild(6).GetComponent<Slider>().value = (float)(stats.CurrentHp - 600) / lowerHpValue;
        }


        if (stats.MaxMp <= 0) {
            return;
        }
        if (stats.CurrentMp <= 100) {
            _mpSliders.GetChild(0).GetComponent<Slider>().value = (float)stats.CurrentMp / lowerMpValue;
        } else if (stats.CurrentMp <= 200) {
            _mpSliders.GetChild(0).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(1).GetComponent<Slider>().value = (float)stats.CurrentMp % 100 / lowerMpValue;
        } else if (stats.CurrentMp <= 300) {
            _mpSliders.GetChild(1).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(2).GetComponent<Slider>().value = (float)stats.CurrentMp % 100 / lowerMpValue;
        } else if (stats.CurrentMp <= 400) {
            _mpSliders.GetChild(2).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(3).GetComponent<Slider>().value = (float)stats.CurrentMp % 100 / lowerMpValue;
        } else if (stats.CurrentMp <= 500) {
            _mpSliders.GetChild(3).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(4).GetComponent<Slider>().value = (float)stats.CurrentMp % 100 / lowerMpValue;
        } else if (stats.CurrentMp <= 600) {
            _mpSliders.GetChild(4).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(5).GetComponent<Slider>().value = (float)stats.CurrentMp % 100 / lowerMpValue;
        } else {
            _mpSliders.GetChild(5).GetComponent<Slider>().value = 1;
            _mpSliders.GetChild(6).GetComponent<Slider>().value = (float)(stats.CurrentMp - 600) / lowerMpValue;
        }
    }

    private void OpenUi() {
        _showUi = true;
        transform.gameObject.SetActive(true);
        _menuAnimator.SetBool("isOpen", true);
    }

    public void CloseQuickInfo() {
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
