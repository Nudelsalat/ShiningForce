using System.Collections;
using Assets.Scripts.GameData.Magic;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.Battle {
    public class FourWayMagicMenu : MonoBehaviour {
        public GameObject TopItem;
        public GameObject LeftItem;
        public GameObject RightItem;
        public GameObject BottomItem;
        public GameObject CurrentSelectedItemLabel;

        private Animator _inventoryAnimator;
        private Sprite _blankSprite;
        private GameObject[] _itemList;
        private GameObject _currentSelectedItem;
        private GameItem[] _gameItemList;
        private Magic[] _magicList;
        private GameItem _currentSelectedGameItem;
        private Magic _currentSelectedMagic;
        private Character _partyMember;
        private bool _isEquipment;
        private bool _showInventory;
        private int _currentSelectedMagicLevel;

        public static FourWayMagicMenu Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            }
            else {
                Instance = this;
            }

            _itemList = new GameObject[] {
                TopItem, LeftItem, BottomItem, RightItem
            };

            _blankSprite = Resources.Load<Sprite>("ShiningForce/images/icon/sfitems");
            _inventoryAnimator = transform.GetComponent<Animator>();
        }

        void Start() {
            _currentSelectedItem = TopItem;
            transform.gameObject.SetActive(false);
        }

        public void LoadMemberInventory(Character character) {
            OpenButtons();
            var gameItems = character.GetInventory();

            _isEquipment = false;
            _partyMember = null;
            _gameItemList = gameItems;
            for (int i = 0; i < gameItems.Length; i++) {
                var itemSprite = gameItems[i].ItemSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;

                _gameItemList[i].PositionInInventory = (DirectionType) i;

                _itemList[i].transform.Find("ItemName").gameObject.GetComponent<Text>().text = gameItems[i].ItemName;

                if (gameItems[i] is Equipment equipment && equipment.IsEquipped) {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Visible;
                }
                else {
                    _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color =
                        Constants.Invisible;
                }
            }
            SelectObject(DirectionType.up);
        }

        public void LoadMemberEquipmentInventory(Character member) {
            OpenButtons();
            
            _currentSelectedItem.transform.Find("ItemName").gameObject.SetActive(true);
            _partyMember = member;
            _isEquipment = true;
            _gameItemList = member?.GetInventory();

            for (int i = 0; i < _gameItemList.Length; i++) {
                var itemSprite = _gameItemList[i].ItemSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
                _itemList[i].transform.Find("Equipped").gameObject.GetComponent<Image>().color = Constants.Invisible;
            }

            SelectObject(_currentSelectedGameItem != null ? _currentSelectedGameItem.PositionInInventory : 0);
        }

        public void LoadMemberMagic(Character character) {
            OpenButtons();
            var gameItems = character.GetMagic();

            _isEquipment = false;
            _partyMember = null;
            _magicList = gameItems;
            for (int i = 0; i < gameItems.Length; i++) {
                var itemSprite = gameItems[i].SpellSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite =
                    (itemSprite != null && gameItems[i].CurrentLevel != 0) ? itemSprite : _blankSprite;
                _magicList[i].PositionInInventory = (DirectionType) i;
            }
            SelectMagic(DirectionType.up);
        }

        public void SelectObject(DirectionType direction) {
            switch (direction) {
                case DirectionType.up:
                    SetCurrentSelectedItem(TopItem, _gameItemList[0]);
                    break;
                case DirectionType.left:
                    SetCurrentSelectedItem(LeftItem, _gameItemList[1]);
                    break;
                case DirectionType.down:
                    SetCurrentSelectedItem(BottomItem, _gameItemList[2]);
                    break;
                case DirectionType.right:
                    SetCurrentSelectedItem(RightItem, _gameItemList[3]);
                    break;
            }
        }


        public void SelectMagic(DirectionType direction) {
            switch (direction) {
                case DirectionType.up:
                    SetCurrentSelectedMagic(TopItem, _magicList[0]);
                    break;
                case DirectionType.left:
                    SetCurrentSelectedMagic(LeftItem, _magicList[1]);
                    break;
                case DirectionType.down:
                    SetCurrentSelectedMagic(BottomItem, _magicList[2]);
                    break;
                case DirectionType.right:
                    SetCurrentSelectedMagic(RightItem, _magicList[3]);
                    break;
            }
        }

        public void SetItemNameOrange() {
            CurrentSelectedItemLabel.transform.Find("ItemName").gameObject.GetComponent<Text>()
                .color = Constants.Orange;
        }
        public void UnSetItemNameOrange() {
            CurrentSelectedItemLabel.transform.Find("ItemName").gameObject.GetComponent<Text>()
                .color = Constants.Visible;
        }

        public void UpdateMagicLevel(DirectionType direction) {
            switch (direction) {
                case DirectionType.down:
                case DirectionType.left:
                    _currentSelectedMagicLevel = (_currentSelectedMagicLevel-1 == 0)
                        ? _currentSelectedMagic.CurrentLevel
                        : _currentSelectedMagicLevel -1;
                    DoMagicLevelUpdate();
                    break;
                case DirectionType.up:
                case DirectionType.right:
                    _currentSelectedMagicLevel = (_currentSelectedMagicLevel + 1 > _currentSelectedMagic.CurrentLevel)
                        ? 1
                        : _currentSelectedMagicLevel + 1;
                    DoMagicLevelUpdate();
                    break;
            }
        }

        private void DoMagicLevelUpdate() {
            for (int j = 1; j <= _currentSelectedMagic.CurrentLevel; j++) {
                var spellLevelHealthBar = CurrentSelectedItemLabel.transform.Find("SpellLevel/" + j.ToString());

                if (j > _currentSelectedMagicLevel) {
                    spellLevelHealthBar.GetChild(0).GetComponent<Slider>().value = 0;
                } else {
                    spellLevelHealthBar.GetChild(0).GetComponent<Slider>().value = 1;
                }
            }
            CurrentSelectedItemLabel.transform.Find("MPCost").gameObject.GetComponent<Text>()
                .text = $"MP {_currentSelectedMagic.ManaCost[_currentSelectedMagicLevel - 1]}";
        }

        public void UnselectObject() {
            _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            _currentSelectedItem.transform.Find("ItemName").GetComponent<Text>().color = Color.white;
        }

        public GameItem GetSelectedGameItem() {
            return _currentSelectedGameItem;
        }

        public Magic GetSelectedMagic() {
            return _currentSelectedMagic;
        }

        public int GetSelectedMagicLevel() {
            return _currentSelectedMagicLevel;
        }

        public void CloseButtons() {
            _inventoryAnimator.SetBool("isOpen", false);
            _showInventory = false;
            StartCoroutine(WaitForTenthASecond());
        }

        public void OpenButtons() {
            transform.gameObject.SetActive(true);
            _showInventory = true;
            _inventoryAnimator.SetBool("isOpen", true);
        }

        private void SetCurrentSelectedItem(GameObject selectedGameObject, GameItem selectedItem) {
            if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
                _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            }

            _currentSelectedItem = selectedGameObject;
            selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
            _currentSelectedGameItem = selectedItem;

            if (_isEquipment) {
                LoadEquipmentStats();
            }
        }

        private void SetCurrentSelectedMagic(GameObject selectedGameObject, Magic selectedMagic) {
            if (selectedMagic.IsEmpty()) {
                return;
            }
            if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
                _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            }

            CurrentSelectedItemLabel.transform.Find("ItemName").gameObject.GetComponent<Text>()
                .text = selectedMagic.SpellName;

            for (int j = 1; j <= 4; j++) {
                var spawnPoint = CurrentSelectedItemLabel.transform.Find("SpellLevel/" + j.ToString());

                if (selectedMagic.CurrentLevel >= j && spawnPoint.childCount == 0) {
                    var item = Resources.Load(Constants.PrefabHealthBar) as GameObject;
                    var pos = new Vector3(0, 0, spawnPoint.transform.position.z);
                    var spawnedItem = Instantiate(item, pos, spawnPoint.transform.rotation);
                    spawnedItem.transform.SetParent(spawnPoint.transform, false);
                } 
                if (spawnPoint.childCount > 0) {
                    spawnPoint.GetChild(0).GetComponent<Slider>().value = 1;
                } 
                if (selectedMagic.CurrentLevel < j) {
                    foreach (Transform child in spawnPoint.transform) {
                        Destroy(child.gameObject);
                    }
                }
            }
            CurrentSelectedItemLabel.transform.Find("MPCost").gameObject.GetComponent<Text>()
                .text = $"MP {selectedMagic.ManaCost[selectedMagic.CurrentLevel - 1]}";
            

            _currentSelectedMagicLevel = selectedMagic.CurrentLevel;
            _currentSelectedItem = selectedGameObject;
            selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
            _currentSelectedMagic = selectedMagic;
        }

        private void LoadEquipmentStats() {
            Equipment newEquipment = null;
            Equipment oldEquipment = null;
            if (_currentSelectedGameItem is Equipment equipment) {
                newEquipment = equipment;
                oldEquipment = _partyMember.GetCurrentEquipment(equipment.EquipmentType);

                var text = CurrentSelectedItemLabel.transform.Find("ItemName").GetComponent<Text>();
                text.text = _currentSelectedGameItem.ItemName;
                text.color = Constants.Visible;


                CurrentSelectedItemLabel.transform.Find("Equipped").gameObject.GetComponent<Image>().color =
                    equipment.IsEquipped ? Constants.Visible : Constants.Invisible;

            }
            else {
                CurrentSelectedItemLabel.transform.Find("Equipped").gameObject.GetComponent<Image>().color =
                    Constants.Invisible;
                var text = CurrentSelectedItemLabel.transform.Find("ItemName").GetComponent<Text>();
                text.text = "Select Equipment";
                text.color = Color.red;
            }

            _itemList[0].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
                "ATTACK" + _partyMember.CharStats.CalculateNewAttack(newEquipment, oldEquipment).ToString().PadLeft(6);

            _itemList[1].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
                "DEFENSE" + _partyMember.CharStats.CalculateNewDefense(newEquipment, oldEquipment).ToString()
                    .PadLeft(5);

            _itemList[2].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
                "AGILITY" + _partyMember.CharStats.CalculateNewAgility(newEquipment, oldEquipment).ToString()
                    .PadLeft(5);

            _itemList[3].transform.Find("ItemName").gameObject.GetComponent<Text>().text =
                "MOVEMENT" + _partyMember.CharStats.CalculateNewMovement(newEquipment, oldEquipment).ToString()
                    .PadLeft(4);

        }

        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showInventory) {
                transform.gameObject.SetActive(false);
            }
        }
    }
}

