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
            _currentSelectedMagic = null;
            var gameItems = character.GetInventory();

            _gameItemList = gameItems;
            for (int i = 0; i < gameItems.Length; i++) {
                var itemSprite = gameItems[i].ItemSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite = itemSprite != null ? itemSprite : _blankSprite;
                _gameItemList[i].PositionInInventory = (DirectionType) i;
            }
            SelectObject(DirectionType.up);
        }
        
        public void LoadMemberMagic(Character character) {
            OpenButtons();
            _currentSelectedGameItem = null;
            var gameItems = character.GetMagic();

            _magicList = gameItems;
            for (int i = 0; i < gameItems.Length; i++) {
                var itemSprite = gameItems[i].SpellSprite;
                _itemList[i].gameObject.GetComponent<Image>().sprite =
                    (itemSprite != null && gameItems[i].CurrentLevel != 0) ? itemSprite : _blankSprite;
                _magicList[i].PositionInInventory = (DirectionType) i;
            }
            SelectMagic(DirectionType.up);
        }

        public void ReloadSelection() {
            if (_currentSelectedGameItem) {
                SetCurrentSelectedItem(_currentSelectedItem, _currentSelectedGameItem);
            }

            if (_currentSelectedMagic) {
                SetCurrentSelectedMagic(_currentSelectedItem, _currentSelectedMagic);
            }
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
            if (this.isActiveAndEnabled) {
                StartCoroutine(WaitForTenthASecond());
            }
        }

        public void OpenButtons() {
            transform.gameObject.SetActive(true);
            _showInventory = true;
            _inventoryAnimator.SetBool("isOpen", true);
        }

        private void SetCurrentSelectedItem(GameObject selectedGameObject, GameItem selectedItem) {
            if (selectedItem.IsEmpty()) {
                return;
            }
            if (_currentSelectedItem != null && _currentSelectedItem != selectedGameObject) {
                _currentSelectedItem.transform.GetComponent<Image>().color = Color.white;
            }
            CurrentSelectedItemLabel.transform.Find("ItemName").gameObject.GetComponent<Text>()
                .text = selectedItem.ItemName;

            for (int j = 1; j <= 4; j++) {
                var spawnPoint = CurrentSelectedItemLabel.transform.Find("SpellLevel/" + j.ToString());
                foreach (Transform child in spawnPoint.transform) {
                    Destroy(child.gameObject);
                }
            }
            var mpCostText = CurrentSelectedItemLabel.transform.Find("MPCost").gameObject.GetComponent<Text>();
            if (selectedItem is Equipment equipment && equipment.IsEquipped) {
                mpCostText.text = $"EQUIPPED";
                mpCostText.color = Constants.Orange;
            } else {
                mpCostText.text = "";
                mpCostText.color = Constants.Visible;
            }

            _currentSelectedItem = selectedGameObject;
            selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
            _currentSelectedGameItem = selectedItem;
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

            var mpCostText = CurrentSelectedItemLabel.transform.Find("MPCost").gameObject.GetComponent<Text>();
            mpCostText.text = $"MP {selectedMagic.ManaCost[selectedMagic.CurrentLevel - 1]}";
            mpCostText.color = Constants.Visible;

            _currentSelectedMagicLevel = selectedMagic.CurrentLevel;
            _currentSelectedItem = selectedGameObject;
            selectedGameObject.transform.GetComponent<Image>().color = Constants.Orange;
            _currentSelectedMagic = selectedMagic;
        }
        
        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showInventory) {
                transform.gameObject.SetActive(false);
            }
        }
    }
}

