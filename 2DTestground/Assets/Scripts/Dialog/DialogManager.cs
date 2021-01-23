using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    public GameObject DialogBox;
    public GameObject Portrait;
    public Animator AnimatorDialogue;
    public Animator AnimatorPortrait;
    public Text DialogText;
    public bool DialogActive;

    private Queue<string> Sentences;
    private bool _hasPortrait = false;

    // Start is called before the first frame update
    void Start() {
        Sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue) {
        if (Player.IsInDialogue) {
            return;
        }

        Debug.Log("Starting dialogue with: " + dialogue.Name);
        Sentences.Clear();

        foreach (string sentence in dialogue.Sentences) {
            Sentences.Enqueue(sentence);
        }

        if (dialogue.Portrait != null) {
            _hasPortrait = true;
            Debug.Log("Portrait not nulll.");
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

    public void DisplayNextSentence() {
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
        foreach (char letter in sentence.ToCharArray()) {
            DialogText.text += letter;
            yield return null;
        }
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
    }
}
