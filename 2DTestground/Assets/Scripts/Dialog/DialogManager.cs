using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    public GameObject DialogBox;
    public GameObject Portrait;
    public GameObject DialogueTriangle;
    public Animator AnimatorDialogue;
    public Animator AnimatorPortrait;
    public Text DialogText;
    public bool DialogActive;

    private Queue<string> Sentences;
    private bool _hasPortrait = false;
    private Dialogue _dialogue;
    private Inventory _inventory;


    void Awake() {
        Sentences = new Queue<string>();
        DialogueTriangle.SetActive(false);
        _inventory = Inventory.Instance;
    }

    // Start is called before the first frame update
    void Start() {
        
    }

    public void StartDialogue(Dialogue dialogue) {
        if (Player.IsInDialogue) {
            return;
        }
        _dialogue = dialogue;

        Debug.Log("Starting dialogue with: " + dialogue.Name);
        Sentences.Clear();

        foreach (string sentence in dialogue.Sentences) {
            var replacedSentence = ReplaceNameVariables(sentence);
            Sentences.Enqueue(replacedSentence);
        }

        if (dialogue.Portrait != null) {
            _hasPortrait = true;
            Debug.Log("Portrait not null.");
            var image = Portrait.transform.GetChild(0).GetComponent<Image>();
            image.sprite = dialogue.Portrait;
            Portrait.SetActive(true);
            AnimatorPortrait.SetBool("portraitIsOpen", true);
        }

        DialogBox.SetActive(true);
        AnimatorDialogue.SetBool("dialogueBoxIsOpen", true);
        DisplayNextSentence();
        Player.IsInDialogue = true;
    }

    public string ReplaceNameVariables(string sentence) {
        var partyLeaderName = _inventory.GetPartyLeaderName();
        sentence = sentence.Replace("#LEADERNAME#", partyLeaderName);
        sentence = sentence.Replace("#CHESTER#", _inventory.GetPartyMemberNameByEnum(CharacterType.chester));
        return sentence.Replace("#SARAH#", _inventory.GetPartyMemberNameByEnum(CharacterType.sarah));
    }

    public void DisplayNextSentence() {
        DialogueTriangle.SetActive(false);
        if (Sentences.Count == 0) {
            Debug.Log("EndDialogue");
            EndDialogue();
            return;
        }

        DialogActive = true;
        string sentence = Sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence) {
        DialogText.text = "";
        Player.InputDisabledInDialogue = true;
        foreach (char letter in sentence.ToCharArray()) {
            DialogText.text += letter;
            yield return null;
        }
        DialogueTriangle.SetActive(true);
        Player.InputDisabledInDialogue = false;
    }

    public void EndDialogue() {
        AnimatorDialogue.SetBool("dialogueBoxIsOpen", false);
        if (_hasPortrait) {
            AnimatorPortrait.SetBool("portraitIsOpen", false);
        }
        StartCoroutine(WaitForQuaterSec());
        _hasPortrait = false;
    }
    IEnumerator WaitForQuaterSec() {
        yield return new WaitForSeconds(0.25f);

        DialogBox.SetActive(false);
        Portrait.SetActive(false);
        Player.IsInDialogue = false;
        DialogActive = false;
        if (_dialogue.FollowUpEvent != null) {
            _dialogue.FollowUpEvent.Invoke("EventTrigger",0);
        }
    }
}
