using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    public GameObject DialogBox;
    public GameObject Portrait;
    public GameObject DialogueTriangle;
    public GameObject YesNoBox;

    public Animator AnimatorDialogue;
    public Animator AnimatorPortrait;
    public Animator AnimatorYes;
    public Animator AnimatorNo;


    //public Text DialogText;
    public Text DialogText;
    public bool DialogActive;
    
    //private Queue<string> _sentences;
    private Queue<string> _sentences;
    private bool _hasPortrait;
    private Dialogue _dialogue;
    private Inventory _inventory;

    private bool _isInQuestion = false;
    private bool _selectedAnswer = true;


    void Awake() {
        _sentences = new Queue<string>();
        DialogueTriangle.SetActive(false);
        YesNoBox.SetActive(false);
        DialogBox.SetActive(false);
    }

    // Start is called before the first frame update
    void Start() {
        _inventory = Inventory.Instance;
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

    public void StartDialogue(Dialogue dialogue) {
        if (Player.IsInDialogue) {
            return;
        }
        _dialogue = dialogue;

        Debug.Log("Starting dialogue with: " + dialogue.Name);
        _sentences.Clear();

        foreach (string sentence in dialogue.Sentences) {
            var replacedSentence = ReplaceNameVariables(sentence);
            _sentences.Enqueue(replacedSentence);
        }

        if (dialogue.Portrait != null) {
            _hasPortrait = true;
            Debug.Log("Portrait not null.");
            var image = Portrait.transform.Find("PortraitPicture").GetComponent<Image>();
            image.sprite = dialogue.Portrait;
            Portrait.SetActive(true);
            AnimatorPortrait.SetBool("portraitIsOpen", true);
        }

        DialogBox.SetActive(true);
        AnimatorDialogue.SetBool("dialogueBoxIsOpen", true);
        DisplayNextSentence();
        Player.IsInDialogue = true;
    }

    public void DisplayNextSentence() {
        DialogueTriangle.SetActive(false);
        if (_sentences.Count == 0) {
            Debug.Log("EndDialogue");
            EndDialogue();
            return;
        }

        DialogActive = true;
        var sentence = _sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }
    
    private string ReplaceNameVariables(string sentence) {
        var partyLeaderName = _inventory.GetPartyLeaderName();
        sentence = sentence.Replace("#LEADERNAME#", partyLeaderName.AddColor(Constants.Orange));
        sentence = sentence.Replace("#CHESTER#", 
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.chester).AddColor(Constants.Orange));
        sentence = sentence.Replace("#SARAH#",
            _inventory.GetPartyMemberNameByEnum(EnumCharacterType.sarah).AddColor(Constants.Orange));
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
        AnimatorDialogue.SetBool("dialogueBoxIsOpen", false);
        if (_hasPortrait) {
            AnimatorPortrait.SetBool("portraitIsOpen", false);
        }
        StartCoroutine(WaitForQuaterSec(result));
    }
    private void EndDialogue() {
        AnimatorDialogue.SetBool("dialogueBoxIsOpen", false);
        if (_hasPortrait) {
            AnimatorPortrait.SetBool("portraitIsOpen", false);
        }
        StartCoroutine(WaitForQuaterSec());
    }

    private void AskQuestionYesNo(Question question) {
        _isInQuestion = true;
        _selectedAnswer = question.DefaultSelectionForQuestion == YesNo.Yes;
        YesNoBox.SetActive(true);
    }


    IEnumerator TypeSentence(string sentence) {
        DialogText.text = "";
        Player.InputDisabledInDialogue = true;
        var stringBuilder = new StringBuilder();
        var styleTagEncountered = false;
        var closingBraketCount = 0;
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
                    DialogText.text += stringBuilder;
                    closingBraketCount = 0;
                    styleTagEncountered = false;
                    stringBuilder.Clear();
                    yield return null;
                }
            } else {
                DialogText.text += letter;
                yield return null;
            }
        }

        DialogueTriangle.SetActive(true);
        
        if (_sentences.Count == 0 && _dialogue is Question question) {
            AskQuestionYesNo(question);
        } else {
            Player.InputDisabledInDialogue = false;
        }
    }

    IEnumerator WaitForQuaterSec(bool result) {
        //MLE waiting 0.2f before callback feels and looks wrong.
        if (_dialogue is QuestionCallback callback) {
            callback.OnAnswerAction(result);
        }
        yield return new WaitForSeconds(0.1f);

        DialogBox.SetActive(false);
        if (_hasPortrait) {
            Portrait.SetActive(false);
            _hasPortrait = false;
        }
        Player.IsInDialogue = false;
        DialogActive = false;
        if (_dialogue is QuestionFollowUpEvent followUpQuestion) {
            if (result) {
                followUpQuestion.FollowUpEventForYes.Invoke("EventTrigger", 0);
            } else {
                followUpQuestion.FollowUpEventForNo.Invoke("EventTrigger", 0);
            }
        } else if (_dialogue.FollowUpEvent != null) {
            _dialogue.FollowUpEvent.Invoke("EventTrigger",0);
        }
    }
    IEnumerator WaitForQuaterSec() {

        yield return new WaitForSeconds(0.1f);

        DialogBox.SetActive(false);
        if (_hasPortrait) {
            Portrait.SetActive(false);
            _hasPortrait = false;
        }
        Player.IsInDialogue = false;
        DialogActive = false;

        if (_dialogue.FollowUpEvent != null) {
            _dialogue.FollowUpEvent.Invoke("EventTrigger", 0);
        }
    }
}