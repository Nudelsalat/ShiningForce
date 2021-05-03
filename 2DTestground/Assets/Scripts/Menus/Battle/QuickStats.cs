using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.GameData.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus.Battle {
    class QuickStats : MonoBehaviour {
        private Animator _animQuickStats;

        private Text _maxHp;
        private Text _maxMp;
        private Text _attack;
        private Text _defense;
        private Text _agility;
        private Text _movement;
        private Text _newMaxHp;
        private Text _newMaxMp;
        private Text _newAttack;
        private Text _newDefense;
        private Text _newAgility;
        private Text _newMovement;


        private bool _showUi;

        public static QuickStats Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }

            _animQuickStats = transform.GetComponent<Animator>();

            _maxHp = transform.Find("HP/Value").GetComponent<Text>();
            _maxMp = transform.Find("MP/Value").GetComponent<Text>();
            _attack = transform.Find("Attack/Value").GetComponent<Text>();
            _defense = transform.Find("Defense/Value").GetComponent<Text>();
            _agility = transform.Find("Agility/Value").GetComponent<Text>();
            _movement = transform.Find("Movement/Value").GetComponent<Text>();

            _newMaxHp = transform.Find("HP/NewValue").GetComponent<Text>();
            _newMaxMp = transform.Find("MP/NewValue").GetComponent<Text>();
            _newAttack = transform.Find("Attack/NewValue").GetComponent<Text>();
            _newDefense = transform.Find("Defense/NewValue").GetComponent<Text>();
            _newAgility = transform.Find("Agility/NewValue").GetComponent<Text>();
            _newMovement = transform.Find("Movement/NewValue").GetComponent<Text>();

        }
        void Start() {
            transform.gameObject.SetActive(false);
        }

        public void ShowQuickInfo(Character character, GameItem newItem) {
            OpenUi();

            var currentMaxHp = character.CharStats.MaxHp();
            var currentMaxMp = character.CharStats.MaxMp();
            var currentAttack = character.CharStats.Attack.GetModifiedValue();
            var currentDefense = character.CharStats.Defense.GetModifiedValue();
            var currentAgility = character.CharStats.Agility.GetModifiedValue();
            var currentMovement = character.CharStats.Movement.GetModifiedValue();

            _maxHp.text = currentMaxHp.ToString();
            _maxMp.text = currentMaxMp.ToString();
            _attack.text = currentAttack.ToString();
            _defense.text = currentDefense.ToString();
            _agility.text = currentAgility.ToString();
            _movement.text = currentMovement.ToString();

            _newMaxHp.text = currentMaxHp.ToString();
            _newMaxHp.color = Constants.Visible;
            _newMaxMp.text = currentMaxMp.ToString();
            _newMaxMp.color = Constants.Visible;
            _newAttack.text = currentAttack.ToString();
            _newAttack.color = Constants.Visible;
            _newDefense.text = currentDefense.ToString();
            _newDefense.color = Constants.Visible;
            _newAgility.text = currentAgility.ToString();
            _newAgility.color = Constants.Visible;
            _newMovement.text = currentMovement.ToString();
            _newMovement.color = Constants.Visible;

            if (!(newItem is Equipment newEquipment)) {
                return;
            }
            if (newEquipment.EquipmentForClass.All(x => x != character.ClassType)) {
                return;
            }
            var currentEquipment = character.GetCurrentEquipment(newEquipment.EquipmentType);

            if (currentEquipment == newItem) {
                if (newEquipment.HpModifier > 0) {
                    _newMaxHp.text = (currentMaxHp - newEquipment.HpModifier).ToString();
                    _newMaxHp.color = Color.red;
                }

                if (newEquipment.MpModifier > 0) {
                    _newMaxMp.text = (currentMaxMp - newEquipment.MpModifier).ToString();
                    _newMaxMp.color = Color.red;
                }

                if (newEquipment.AttackModifier > 0) {
                    _newAttack.text = (currentAttack - newEquipment.AttackModifier).ToString();
                    _newAttack.color = Color.red;
                }

                if (newEquipment.DefenseModifier > 0) {
                    _newDefense.text = (currentDefense - newEquipment.DefenseModifier).ToString();
                    _newDefense.color = Color.red;
                }

                if (newEquipment.AgilityModifier > 0) {
                    _newAgility.text = (currentAgility - newEquipment.AgilityModifier).ToString();
                    _newAgility.color = Color.red;
                }

                if (newEquipment.MovementModifier > 0) {
                    _newMovement.text = (currentMovement - newEquipment.MovementModifier).ToString();
                    _newMovement.color = Color.red;
                }
            } else if (currentEquipment == null) {
                if (newEquipment.HpModifier > 0) {
                    _newMaxHp.text = (currentMaxHp + newEquipment.HpModifier).ToString();
                    _newMaxHp.color = Color.green;
                }

                if (newEquipment.MpModifier > 0) {
                    _newMaxMp.text = (currentMaxMp + newEquipment.MpModifier).ToString();
                    _newMaxMp.color = Color.green;
                }

                if (newEquipment.AttackModifier > 0) {
                    _newAttack.text = (currentAttack + newEquipment.AttackModifier).ToString();
                    _newAttack.color = Color.green;
                }

                if (newEquipment.DefenseModifier > 0) {
                    _newDefense.text = (currentDefense + newEquipment.DefenseModifier).ToString();
                    _newDefense.color = Color.green;
                }

                if (newEquipment.AgilityModifier > 0) {
                    _newAgility.text = (currentAgility + newEquipment.AgilityModifier).ToString();
                    _newAgility.color = Color.green;
                }

                if (newEquipment.MovementModifier > 0) {
                    _newMovement.text = (currentMovement + newEquipment.MovementModifier).ToString();
                    _newMovement.color = Color.green;
                }
            } else {
                var newHp = newEquipment.HpModifier - currentEquipment.HpModifier;
                if (newHp != 0) {
                    _newMaxHp.text = (currentMaxHp + newHp).ToString();
                    _newMaxHp.color = newHp > 0 ? Color.green : Color.red;
                }

                var newMp = newEquipment.MpModifier - currentEquipment.MpModifier;
                if (newMp != 0) {
                    _newMaxMp.text = (currentMaxMp + newMp).ToString();
                    _newMaxMp.color = newMp > 0 ? Color.green : Color.red;
                }

                var newAttack = newEquipment.AttackModifier - currentEquipment.AttackModifier;
                if (newAttack != 0) {
                    _newAttack.text = (currentAttack + newAttack).ToString();
                    _newAttack.color = newAttack > 0 ? Color.green : Color.red;
                }

                var newDefense = newEquipment.DefenseModifier - currentEquipment.DefenseModifier;
                if (newDefense != 0) {
                    _newDefense.text = (currentDefense + newDefense).ToString();
                    _newDefense.color = newDefense > 0 ? Color.green : Color.red;
                }

                var newAgility = newEquipment.AgilityModifier - currentEquipment.AgilityModifier;
                if (newAgility != 0) {
                    _newAgility.text = (currentAgility + newAgility).ToString();
                    _newAgility.color = newAgility > 0 ? Color.green : Color.red;
                }

                var newMovement = newEquipment.MovementModifier - currentEquipment.MovementModifier;
                if (newMovement != 0) {
                    _newMovement.text = (currentMovement + newMovement).ToString();
                    _newMovement.color = newMovement > 0 ? Color.green : Color.red;
                }
            }
        }

        private void OpenUi() {
            _showUi = true;
            transform.gameObject.SetActive(true);
            _animQuickStats.SetBool("isOpen", true);
        }

        public void CloseQuickInfo() {
            _animQuickStats.SetBool("isOpen", false);
            _showUi = false;
            if (this.isActiveAndEnabled) {
                StartCoroutine(WaitForTenthASecond());
            }
        }

        IEnumerator WaitForTenthASecond() {
            yield return new WaitForSeconds(0.1f);
            if (!_showUi) {
                transform.gameObject.SetActive(false);
            }
        }
    }
}
