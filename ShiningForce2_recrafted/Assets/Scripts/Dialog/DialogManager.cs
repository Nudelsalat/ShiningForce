using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Battle;
using Assets.Scripts.Menus;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    public GameObject DialogBox;
    public GameObject DialogueTriangle;
    public Text DialogText;
    public GameObject BattleDialogBox;
    public GameObject BattleDialogueTriangle;
    public Text BattleDialogText;

    public GameObject YesNoBox;

    public Animator AnimatorYes;
    public Animator AnimatorNo;

    public bool DialogActive;

    private Animator _animatorDialogue;
    private Animator _animatorBattleDialogue;

    private Animator _currentDialogueAnimator;
    private GameObject _dialogBox;
    private GameObject _dialogueTriangle;
    private Text _dialogText;

    private Queue<string> _sentences;
    private bool _hasPortrait;
    private Dialogue _dialogue;
    private Inventory _inventory;
    private Portrait _portrait;
    public static AudioManager _audioManager;
    public static BattleController _battleController;

    private bool _isInQuestion = false;
    private bool _selectedAnswer = true;
    private bool _showUi = true;

    private string _voicePitch;

    public static DialogManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        } else {
            Instance = this;
        }

        _animatorDialogue = DialogBox.GetComponent<Animator>();
        _animatorBattleDialogue = BattleDialogBox.GetComponent<Animator>();

        _sentences = new Queue<string>();
        DialogueTriangle.SetActive(false);
        DialogBox.SetActive(false);
        BattleDialogueTriangle.SetActive(false);
        BattleDialogBox.SetActive(false);
        YesNoBox.SetActive(false);
    }

    // Start is called before the first frame update
    void Start() {
        _inventory = Inventory.Instance;
        _portrait = Portrait.Instance;
        _audioManager = AudioManager.Instance;
        _battleController = BattleController.Instance;
    }

    void Update() {
        if (_isInQuestion) {
            if (Input.GetAxisRaw("Horizontal") > 0.05f) {
                _selectedAnswer = false;
            } else if (Input.GetAxisRaw("Horizontal") < -0.05f) {
                _selectedAnswer = true;
            }

            if (Input.GetButtonUp("Back")) {
                //Back should always be NO
                _selectedAnswer = false;
                QuestionResult(_selectedAnswer);
            }

            if (Input.GetButtonUp("Interact")) {
                QuestionResult(_selectedAnswer);
            }
        }
    }

    void FixedUpdate() {
        if (_isInQuestion) {
            if (_selectedAnswer) {
                AnimatorYes.SetBool("selected", true);
                AnimatorNo.SetBool("selected", false);
            } else {
                AnimatorNo.SetBool("selected", true);
                AnimatorYes.SetBool("selected", false);
            }
        }
    }

    public void EvokeSingleSentenceDialogue(string sentence, bool evokeNewDialogue = false) {
        _showUi = true;
        if (Player.IsInDialogue && !evokeNewDialogue) {
            var replacedSentence = ReplaceNameVariables(sentence);
            _sentences.Enqueue(replacedSentence);
            return;
        }
        var dialogue = new Dialogue {
            Sentences = new List<string>() {sentence},
            Name = "singleSentence",
            VoicePitch = EnumVoicePitch.none
        };
        StartDialogue(dialogue);
    }

    public void EvokeSentenceDialogue(List<string> sentence) {
        if (Player.IsInDialogue) {
            foreach (string newSentence in sentence) {
                var replacedSentence = ReplaceNameVariables(newSentence);
                _sentences.Enqueue(replacedSentence);
            }
            return;
        }
        var dialogue = new Dialogue {
            Sentences = sentence,
            Name = "singleSentence",
            VoicePitch = EnumVoicePitch.none
        };
        StartDialogue(dialogue);
    }

    public void StartDialogue(Dialogue dialogue) {
        _showUi = true;
        if (Player.IsInDialogue) {
            return;
        }

        if (_battleController.IsActive) {
            _dialogBox = BattleDialogBox;
            _dialogueTriangle = BattleDialogueTriangle;
            _dialogText = BattleDialogText;
            _currentDialogueAnimator = _animatorBattleDialogue;
        } else {
            _dialogBox = DialogBox;
            _dialogueTriangle = DialogueTriangle;
            _dialogText = DialogText;
            _currentDialogueAnimator = _animatorDialogue;
        }

        SelectVoice(dialogue.VoicePitch);

        _dialogue = dialogue;

        Debug.Log("Starting dialogue with: " + dialogue.Name);
        _sentences.Clear();

        foreach (string sentence in dialogue.Sentences) {
            var replacedSentence = ReplaceNameVariables(sentence);
            _sentences.Enqueue(replacedSentence);
        }

        if (dialogue.Portrait != null) {
            _hasPortrait = true;
            _portrait.ShowPortrait(dialogue.Portrait);
        }

        _dialogBox.SetActive(true);
        _currentDialogueAnimator.SetBool("dialogueBoxIsOpen", true);
        DisplayNextSentence();
        Player.IsInDialogue = true;
    }

    public void DisplayNextSentence() {
        _dialogueTriangle?.SetActive(false);
        if (_sentences.Count == 0) {
            Debug.Log("EndDialogue");
            EndDialogue();
            return;
        }

        DialogActive = true;
        var sentence = _sentences.Dequeue();


        _audioManager.Play(_voicePitch);
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    public void AddSentenceAndContinue(string sentence) {
        _sentences.Enqueue(sentence);
    }
    
    private string ReplaceNameVariables(string sentence) {
        var partyLeaderName = _inventory.GetPartyLeaderName();
        sentence = sentence.Replace("#LEADER#", partyLeaderName);
        sentence = sentence.Replace("#CHESTER#", 
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.chester));
        sentence = sentence.Replace("#SARAH#",
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.sarah));
        sentence = sentence.Replace("#JAHA#",
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.jaha));
        sentence = sentence.Replace("#KAZIN#",
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.kazin));
        sentence = sentence.Replace("#SLADE#",
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.slade));
        //sentence = sentence.Replace("#GERHALT#",
        //    _inventory.GetPartyMemberNameByEnum(EnumCharacterType.gerhalt));
        return sentence;
    }

    private void QuestionResult(bool result) {
        _isInQuestion = false;
        YesNoBox.SetActive(false);
        Player.InputDisabledInDialogue = false;
        Debug.Log("EndDialogue");
        EndDialogue(result);
    }

    private void EndDialogue(bool result) {
        _currentDialogueAnimator.SetBool("dialogueBoxIsOpen", false);
        if (_hasPortrait) {
            _portrait.HidePortrait();
        }
        _showUi = false;
        StartCoroutine(WaitForQuaterSec(result));
    }
    private void EndDialogue() {
        _currentDialogueAnimator?.SetBool("dialogueBoxIsOpen", false);
        if (_hasPortrait) {
            _portrait.HidePortrait();
        }
        _showUi = false;
        StartCoroutine(WaitForQuaterSec());
    }

    private void AskQuestionYesNo(Question question) {
        _isInQuestion = true;
        _selectedAnswer = question.DefaultSelectionForQuestion == YesNo.Yes;
        YesNoBox.SetActive(true);
    }

    private void SelectVoice(EnumVoicePitch enumVoicePitch) {
        _voicePitch = Enum.GetName(typeof(EnumVoicePitch), enumVoicePitch);
    }

    IEnumerator TypeSentence(string sentence) {
        _dialogText.text = "";
        Player.InputDisabledInDialogue = true;
        var stringBuilder = new StringBuilder();
        var styleTagEncountered = false;
        var closingBraketCount = 0;
        var letterSpeedUp = 0;
        foreach (var letter in sentence) {
            if (letter == '<') {
                styleTagEncountered = true;
            }
            if (styleTagEncountered) {
                stringBuilder.Append(letter);
                if (letter == '>') {
                    closingBraketCount++;
                }

                if (closingBraketCount == 2) {
                    _dialogText.text += stringBuilder;
                    closingBraketCount = 0;
                    styleTagEncountered = false;
                    stringBuilder.Clear();
                    yield return null;
                }
            } else {
                _dialogText.text += letter;
                if (Time.timeScale > 1f && letterSpeedUp < 2) {
                    letterSpeedUp++;
                } else {
                    letterSpeedUp = 0;
                    yield return null;
                }
            }
        }

        _audioManager.Stop(_voicePitch);

        _dialogueTriangle.SetActive(true);
        
        if (_sentences.Count == 0 && _dialogue is Question question) {
            AskQuestionYesNo(question);
        } else {
            Player.InputDisabledInDialogue = false;
        }
    }


    IEnumerator WaitForQuaterSec(bool result) {
        Player.InputDisabledInDialogue = true;
        //MLE waiting 0.2f before callback feels and looks wrong.
        if (_dialogue is QuestionCallback callback) {
            callback.OnAnswerAction(result);
        }

        yield return new WaitForSeconds(0.1f);

        if (!_showUi) {
            _dialogBox.SetActive(false);
            if (_hasPortrait) {
                _portrait.HidePortrait();
                _hasPortrait = false;
            }

            Player.IsInDialogue = false;
            DialogActive = false;
        }

        if (_dialogue is QuestionFollowUpEvent followUpQuestion) {
            if (result) {
                followUpQuestion.FollowUpEventForYes.Invoke("EventTrigger", 0);
            }
            else {
                followUpQuestion.FollowUpEventForNo.Invoke("EventTrigger", 0);
            }
        }
        else if (_dialogue.FollowUpEvent != null) {
            _dialogue.FollowUpEvent.Invoke("EventTrigger", 0);
        }
        yield return new WaitForEndOfFrame();
        Player.InputDisabledInDialogue = false;
    }

    IEnumerator WaitForQuaterSec() {
        Player.InputDisabledInDialogue = true;
        yield return new WaitForSeconds(0.1f);

        if (!_showUi) {
            _dialogBox?.SetActive(false);
            if (_hasPortrait) {
                _portrait.HidePortrait();
                _hasPortrait = false;
            }

            Player.IsInDialogue = false;
            DialogActive = false;
        }

        if (_dialogue?.FollowUpEvent != null) {
            _dialogue.FollowUpEvent.Invoke("EventTrigger", 0);
        }
        yield return new WaitForEndOfFrame();
        Player.InputDisabledInDialogue = false;
    }
}