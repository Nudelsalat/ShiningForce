using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Enums;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.AI;
using Assets.Scripts.HelperScripts;
using UnityEngine;

namespace Assets.Scripts.Menus.Battle {
    public class AttackPhase : MonoBehaviour {

        private EnumAttackPhase _state = EnumAttackPhase.None;
        private EnumAttackPhase _followUpState = EnumAttackPhase.None;

        private EnumCurrentBattleMenu _attackType = EnumCurrentBattleMenu.none;
        private AttackOption _attackOption;
        private Queue<Unit> _targetQueue;
        private Unit _attacker;
        private Unit _nextTarget;
        private bool _isAttackerForceUnit;
        private bool _doubleAttack = false;
        private string _currentAudioFile;
        private string _itemName;
        private string _spellAnimationPath;

        private readonly List<string> _sentences = new List<string>();
        private int _exp;
        private int _gold;
        private readonly List<Unit> _unitsKilled = new List<Unit>();

        private BattleCalculator _battleCalculator;
        private BattleController _battleController;
        private AudioManager _audioManager;
        private Cursor _cursor;
        private QuickInfoUi _quickInfo;
        private QuickInfoUiTarget _quickInfoUiTarget;
        private BattleAnimationUi _battleAnimationUi;
        private DialogManager _dialogManager;

        public static AttackPhase Instance;

        void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
                return;
            } else {
                Instance = this;
            }
        }
        void Start() {
            _audioManager = AudioManager.Instance;
            _cursor = Cursor.Instance;
            _battleController = BattleController.Instance;
            _quickInfo = QuickInfoUi.Instance;
            _quickInfoUiTarget = QuickInfoUiTarget.Instance;
            _battleAnimationUi = BattleAnimationUi.Instance;
            _dialogManager = DialogManager.Instance;
            _battleCalculator = new BattleCalculator();

            gameObject.SetActive(false);
        }

        void Update() {
            if (Player.InputDisabledInDialogue || Player.IsInDialogue) {
                return;
            }
            switch (_state) {
                case EnumAttackPhase.None:
                    break;

                case EnumAttackPhase.AttackFadinDone:
                    switch (_attackType) {
                        case EnumCurrentBattleMenu.magic:
                            _attacker.GetCharacter().CharStats.CurrentMp -=
                                _attackOption.GetMagic().ManaCost[_attackOption.GetMagicLevel() - 1];
                            _dialogManager.EvokeSingleSentenceDialogue(
                                $"{_attacker.GetCharacter().Name.AddColor(Constants.Orange)} uses " +
                                $"{_attackOption.GetMagic().SpellName.AddColor(Constants.Violet)} at level {_attackOption.GetMagicLevel()}!");
                            break;
                        case EnumCurrentBattleMenu.item:
                            _dialogManager.EvokeSingleSentenceDialogue(
                                $"{_attacker.GetCharacter().Name.AddColor(Constants.Orange)} uses {_itemName.AddColor(Color.green)}!");
                            break;
                        case EnumCurrentBattleMenu.none:
                        case EnumCurrentBattleMenu.attack:
                        case EnumCurrentBattleMenu.stay:
                        default:
                            _dialogManager.EvokeSingleSentenceDialogue(
                                $"{_attacker.GetCharacter().Name.AddColor(Constants.Orange)} attacks!");
                            break;
                    }
                    //TODO: check if cursed
                    _state = EnumAttackPhase.AttackAnimation;
                    break;

                case EnumAttackPhase.AttackAnimation:
                    var isDodge = false;
                    if (_attackType != EnumCurrentBattleMenu.magic && _attackType != EnumCurrentBattleMenu.item) {
                        isDodge = _battleCalculator.RollForDodge(_attacker.GetCharacter(), _nextTarget.GetCharacter());
                    }
                    if (isDodge) {
                        _battleAnimationUi.DoAttackAnimation(isDodge, false, _spellAnimationPath, _attackOption.GetMagic()?.MagicType);
                        _sentences.Add($"{_nextTarget.GetCharacter().Name.AddColor(Constants.Orange)} dodged the attack!");
                        _followUpState = EnumAttackPhase.CheckCounter;
                        StartCoroutine(WaitSeconds(1, EnumAttackPhase.DisplayTextUpdateQuickInfo));
                        break;
                    }
                    _exp += ExecuteAttack();
                    var isKilled = _unitsKilled.Contains(_nextTarget);
                    _battleAnimationUi.DoAttackAnimation(false, isKilled, _spellAnimationPath,  _attackOption.GetMagic()?.MagicType);
                    _followUpState = isKilled ? EnumAttackPhase.GetNextTarget : EnumAttackPhase.CheckCounter;
                    StartCoroutine(WaitSeconds(1, EnumAttackPhase.DisplayTextUpdateQuickInfo));
                    break;

                case EnumAttackPhase.CheckCounter:
                    if (_attackType == EnumCurrentBattleMenu.magic || _attackType == EnumCurrentBattleMenu.item) {
                        _state = EnumAttackPhase.GetNextTarget;
                        break;
                    }
                    _battleAnimationUi.StopAttackAnimation();
                    var spacesApart = GetSpacesApart(_attacker, _nextTarget);
                    if (_battleCalculator.RollForCounter(_nextTarget.GetCharacter(), spacesApart)) {
                        _sentences.Add($"Counterattack!");
                        _state = EnumAttackPhase.DoCounter;
                        break;
                    }

                    StartCoroutine(_doubleAttack
                        ? WaitSeconds(1, EnumAttackPhase.GetNextTarget)
                        : WaitSeconds(1, EnumAttackPhase.CheckDoubleAttack));
                    break;

                case EnumAttackPhase.DoCounter:
                    isDodge = false;
                    if (_attackType != EnumCurrentBattleMenu.magic && _attackType != EnumCurrentBattleMenu.item) {
                        isDodge = _battleCalculator.RollForDodge(_attacker.GetCharacter(), _nextTarget.GetCharacter());
                    }
                    if (isDodge) {
                        // isDodge, isKill, spellAnimation
                        _battleAnimationUi.DoCounterAnimation(isDodge, false);
                        _sentences.Add($"{_attacker.GetCharacter().Name.AddColor(Constants.Orange)} dodged the attack!");
                        _followUpState = _doubleAttack ? EnumAttackPhase.GetNextTarget : EnumAttackPhase.CheckDoubleAttack;
                        StartCoroutine(WaitSeconds(1, EnumAttackPhase.DisplayTextUpdateQuickInfo));
                        break;
                    }
                    var counterExp = ExecuteRegularAttack(_nextTarget, _attacker);
                    if (!_isAttackerForceUnit) {
                        if (counterExp >= 50) {
                            counterExp = 49;
                        } else if (counterExp <= 0) {
                            counterExp = 1;
                        }
                        _sentences.AddRange(_nextTarget.GetCharacter().AddExp(counterExp));
                    }
                    isKilled = _unitsKilled.Contains(_attacker);
                    _battleAnimationUi.DoCounterAnimation(false, isKilled);
                    if (isKilled) {
                        _followUpState = EnumAttackPhase.EndAttackPhase;
                        StartCoroutine(WaitSeconds(1, EnumAttackPhase.DisplayTextUpdateQuickInfo));
                        return;
                    }
                    _followUpState = _doubleAttack ? EnumAttackPhase.GetNextTarget : EnumAttackPhase.CheckDoubleAttack;
                    StartCoroutine(WaitSeconds(1, EnumAttackPhase.DisplayTextUpdateQuickInfo));
                    break;

                case EnumAttackPhase.CheckDoubleAttack:
                    if (_attackType == EnumCurrentBattleMenu.magic || _attackType == EnumCurrentBattleMenu.item) {
                        _state = EnumAttackPhase.GetNextTarget;
                    }
                    _battleAnimationUi.StopAttackAnimation();
                    if (_battleCalculator.RollForDoubleAttack(_attacker.GetCharacter())) {
                        _doubleAttack = true;
                        _sentences.Add($"Second Attack!");
                        _state = EnumAttackPhase.AttackAnimation;
                    } else {
                        _state = EnumAttackPhase.GetNextTarget;
                    }
                    break;
                
                case EnumAttackPhase.GetNextTarget:
                    _doubleAttack = false;
                    if (_targetQueue.Count > 0) {
                        _nextTarget = _targetQueue.Dequeue();
                        _state = EnumAttackPhase.Transition;
                    } else {
                        _state = EnumAttackPhase.EndAttackPhase;
                        _battleAnimationUi.StopAttackAnimation();
                    }
                    break;

                case EnumAttackPhase.Transition:
                    if (_attackType != EnumCurrentBattleMenu.magic && _attackType != EnumCurrentBattleMenu.item) {
                        _battleAnimationUi.StopAttackAnimation();
                    }
                    _battleAnimationUi.Transition(_nextTarget, _nextTarget == _attacker);
                    StartCoroutine(WaitSeconds(1, EnumAttackPhase.AttackAnimation));
                    break;

                case EnumAttackPhase.DisplayTextUpdateQuickInfo:
                    if (_isAttackerForceUnit) {
                        _quickInfo.ShowQuickInfo(_attacker.GetCharacter());
                        _quickInfoUiTarget.ShowQuickInfo(_nextTarget.GetCharacter());
                    } else {
                        _quickInfo.ShowQuickInfo(_nextTarget.GetCharacter());
                        _quickInfoUiTarget.ShowQuickInfo(_attacker.GetCharacter());
                    }
                    _dialogManager.EvokeSentenceDialogue(_sentences);
                    _sentences.Clear();
                    _state = _followUpState;
                    break;

                case EnumAttackPhase.EndAttackPhase:
                    if (_exp >= 50) {
                        _exp = 49;
                    } else if (_exp <= 0) {
                        _exp = 1;
                    } else if (_attackType == EnumCurrentBattleMenu.magic &&
                               _attackOption.GetMagic().MagicType == EnumMagicType.Heal && _exp < 10) {
                        _exp = 10;
                    }
                    if (_isAttackerForceUnit) {
                        _sentences.AddRange(_attacker.GetCharacter().AddExp(_exp));
                    }

                    if (_gold > 0) {
                        _sentences.Add($"Found {_gold} gold coins!");
                    }
                    _dialogManager.EvokeSentenceDialogue(_sentences);
                    _sentences.Clear();
                    StartCoroutine(WaitSeconds(1f, EnumAttackPhase.DoEndAttackPhase));
                    break;
                case EnumAttackPhase.DoEndAttackPhase:
                    _state = EnumAttackPhase.None;
                    EndAttackPhase();
                    break;
            }
        }

        public void ExecuteAttackPhase(EnumCurrentBattleMenu attackType, AttackOption attackOption, Unit attacker, 
            string itemName, bool bossAttack = false) {
            Player.InputDisabledInAttackPhase = true;
            gameObject.SetActive(true);
            _unitsKilled.Clear();
            _exp = 0;
            _gold = 0;

            _attackOption = attackOption;
            _itemName = itemName;
            _isAttackerForceUnit = attacker.GetCharacter().IsForce;
            _attackType = attackType;
            _targetQueue = new Queue<Unit>(attackOption.GetTargetList());
            _attacker = attacker;
            _spellAnimationPath = "";

            if (_attackType == EnumCurrentBattleMenu.magic || _attackType == EnumCurrentBattleMenu.item) {
                var magic = attackOption.GetMagic();
                var spellLevel = attackOption.GetMagicLevel();
                _spellAnimationPath = $"{Constants.PrefabSpellPrefix}{magic.SpellName}/{magic.SpellName}{spellLevel}";
            }

            _currentAudioFile = bossAttack ? Constants.SoundBossAttack :
                _isAttackerForceUnit ? Constants.SoundForceAttack : Constants.SoundEnemyAttack;
            _audioManager.PauseAll();
            _audioManager.Play(_currentAudioFile);

            _nextTarget = _targetQueue.Dequeue();
            var unitsAreOpponents = _nextTarget.GetCharacter().IsForce != _isAttackerForceUnit;
            _battleAnimationUi.Load(_isAttackerForceUnit, unitsAreOpponents, _attacker, _nextTarget);
            StartCoroutine(WaitSeconds(1, EnumAttackPhase.AttackFadinDone));
        }

        private void EndAttackPhase() {
            _audioManager.Stop(_currentAudioFile);
            _audioManager.UnPauseAll();
            _battleAnimationUi.EndAttackAnimation();
            foreach (var unit in _unitsKilled) {
                _battleController.RemoveUnitFromBattle(unit);
            }

            StartCoroutine(WaitExplosion(0.5f));
        }

        private int ExecuteAttack() {
            switch (_attackType) {
                case EnumCurrentBattleMenu.none:
                case EnumCurrentBattleMenu.stay:
                case EnumCurrentBattleMenu.attack:
                    return ExecuteRegularAttack(_attacker, _nextTarget);
                case EnumCurrentBattleMenu.magic:
                case EnumCurrentBattleMenu.item:
                    return ExecuteMagicAttack();
            }
            return 0;
        }

        private int ExecuteMagicAttack() {
            var caster = _attacker.GetCharacter();
            var magicLevel = _attackOption.GetMagicLevel();
            var magic = _attackOption.GetMagic();
            var damage = magic.Damage[magicLevel - 1];
            var levelBoost = caster.CharStats.Level / 100f;
            damage = caster.IsPromoted ? (int) (damage * (1.25f + levelBoost)) : (int) (damage * (1.0f + levelBoost));
            var expScore = 0;

            var target = _nextTarget.GetCharacter();
            var levelDifference = caster.CharStats.Level -
                                  target.CharStats.Level;
            var expBase = levelDifference <= 0 ? 50 : levelDifference >= 5 ? 0 : 50 - 10 * levelDifference;

            var critString = "";

            var wasResistantWeak = _battleCalculator.GetModifiedDamageBasedOnElement(ref damage, magic.ElementType, _nextTarget.GetCharacter());

            switch (_attackOption.GetMagic().MagicType) {
                case EnumMagicType.Damage:
                    var resistantWeakSentence = wasResistantWeak == 1 ? $"\nIt was not very effective..." :
                        wasResistantWeak == 2 ? $"\nIt was very effective!!!" : "";
                    var damageSentence = $"{critString}{target.Name.AddColor(Constants.Orange)} suffered {damage} " +
                                         $"points of damage.{resistantWeakSentence}";
                    _sentences.Add(damageSentence);
                    if (target.CharStats.CurrentHp <= damage) {
                        _sentences.Add($"{target.Name.AddColor(Constants.Orange)}" +
                                     $" was defeated!");
                        target.CharStats.CurrentHp = 0;
                        _unitsKilled.Add(_nextTarget);
                        expScore += (int) ((expBase * (float) damage / target.CharStats.MaxHp()) + expBase);
                    }
                    else {
                        target.CharStats.CurrentHp -= damage;
                        expScore += (int) (expBase * (float) damage / target.CharStats.MaxHp());
                    }

                    break;
                case EnumMagicType.Heal:
                    var diff = target.CharStats.MaxHp() -
                               target.CharStats.CurrentHp;
                    var pointsToHeal = diff < damage ? diff : damage;

                    target.CharStats.CurrentHp += pointsToHeal;
                    _sentences.Add(
                        $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                        $"points.");
                    var expPoints = 25 * (float) pointsToHeal / target.CharStats.MaxHp();
                    expScore += (int) expPoints;
                    break;
                case EnumMagicType.RestoreMP:
                    diff = target.CharStats.MaxMp() -
                           target.CharStats.CurrentMp;
                    pointsToHeal = diff < damage ? diff : damage;
                    target.CharStats.CurrentMp += damage;
                    _sentences.Add(
                        $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                        $"points.");

                    expPoints = 25 * (float) pointsToHeal / target.CharStats.MaxMp();
                    expScore += (int) expPoints;
                    break;
                case EnumMagicType.RestoreBoth:
                    diff = target.CharStats.MaxHp() -
                           target.CharStats.CurrentHp;
                    pointsToHeal = diff < damage ? diff : damage;

                    target.CharStats.CurrentHp += pointsToHeal;
                    _sentences.Add(
                        $"{critString}{target.Name.AddColor(Constants.Orange)} healed {pointsToHeal} " +
                        $"points.");
                    expPoints = 13 * (float) pointsToHeal / target.CharStats.MaxHp();
                    expScore += (int) expPoints;

                    diff = target.CharStats.MaxMp() -
                           target.CharStats.CurrentMp;
                    pointsToHeal = diff < damage ? diff : damage;
                    target.CharStats.CurrentMp += pointsToHeal;
                    _sentences.Add(
                        $"{critString}{target.Name.AddColor(Constants.Orange)} restored {pointsToHeal} " +
                        $"points of Mana.");

                    expPoints = 13 * (float) pointsToHeal / target.CharStats.MaxMp();
                    expScore += (int) expPoints;
                    break;
                case EnumMagicType.Cure:
                    var statusEffectsToCure = new List<EnumStatusEffect>();
                    switch (magicLevel) {
                        case 1:
                            statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                            break;
                        case 2:
                            statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                            statusEffectsToCure.Add(EnumStatusEffect.sleep);
                            break;
                        case 3:
                            statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                            statusEffectsToCure.Add(EnumStatusEffect.sleep);
                            statusEffectsToCure.Add(EnumStatusEffect.confused);
                            break;
                        case 4:
                            statusEffectsToCure.Add(EnumStatusEffect.poisoned);
                            statusEffectsToCure.Add(EnumStatusEffect.sleep);
                            statusEffectsToCure.Add(EnumStatusEffect.confused);
                            statusEffectsToCure.Add(EnumStatusEffect.paralyzed);
                            break;
                    }

                    foreach (var statusEffect in statusEffectsToCure) {
                        if (target.StatusEffects.HasFlag(statusEffect)) {
                            target.StatusEffects = target.StatusEffects.Remove(statusEffect);
                            _sentences.Add(
                                $"{target.Name.AddColor(Constants.Orange)} is no longer " +
                                $"{Enum.GetName(typeof(EnumStatusEffect), statusEffect).AddColor(Color.gray)}");
                        }
                    }

                    expScore += 5;
                    break;
                case EnumMagicType.Special:
                    return 0;
                default:
                    return 0;
            }
            return expScore;
        }

        private int ExecuteRegularAttack(Unit attacker, Unit targetUnit) {
            var expScore = 0;
            var currentChar = attacker.GetCharacter();
                var target = targetUnit.GetCharacter();
                var levelDifference = currentChar.CharStats.Level -
                                      target.CharStats.Level;
                var expBase = levelDifference <= 0 ? 50 : levelDifference >= 5 ? 0 : 50 - 10 * levelDifference;
                var damage = _battleCalculator.GetBaseDamageWeaponAttack(
                    currentChar, target, _cursor.GetLandEffect(targetUnit.transform.position));


                var isCrit = _battleCalculator.RollForCrit(currentChar);
                var critString = "";
                //TODO is elemental resistance vulnerability?
                if (isCrit) {
                    damage = _battleCalculator.GetCritDamage(currentChar, damage);
                    critString = "Critical hit!\n";
                }

                _sentences.Add(
                    $"{critString}{target.Name.AddColor(Constants.Orange)} suffered {damage} " +
                    $"points of damage.");
                if (target.CharStats.CurrentHp <= damage) {
                    _sentences.Add($"{target.Name.AddColor(Constants.Orange)}" +
                                 $" got defeated!");
                    target.CharStats.CurrentHp = 0;
                    currentChar.CharStats.Kills += 1;
                    target.CharStats.Defeats += 1;
                    _unitsKilled.Add(targetUnit);
                    if (targetUnit.GetCharacter() is Monster monster) {
                        _gold += monster.Gold;
                    }
                    expScore += (int)((expBase * (float)damage / target.CharStats.MaxHp()) +
                                      expBase);
                } else {
                    target.CharStats.CurrentHp -= damage;
                    expScore += (int)(expBase * (float)damage / target.CharStats.MaxHp());
                }

            return expScore;
        }

        private int GetSpacesApart(Unit unit1, Unit unit2) {
            var x = (int)Math.Abs(unit1.transform.position.x - unit2.transform.position.x);
            var y = (int)Math.Abs(unit1.transform.position.y - unit2.transform.position.y);
            return x + y;
        }

        IEnumerator WaitSeconds(float seconds, EnumAttackPhase nextState) {
            _state = EnumAttackPhase.None;
            yield return new WaitForSeconds(seconds);
            _state = nextState;
        }
        IEnumerator WaitExplosion(float seconds) {
            yield return new WaitForSeconds(seconds);
            Player.InputDisabledInAttackPhase = false;
        }
    }


    internal enum EnumAttackPhase {
        None,
        AttackFadinDone,
        GetNextTarget,
        AttackAnimation,
        CheckCounter,
        DoCounter,
        CheckDoubleAttack,
        Transition,
        DisplayTextUpdateQuickInfo,
        EndAttackPhase,
        DoEndAttackPhase
    }
}
