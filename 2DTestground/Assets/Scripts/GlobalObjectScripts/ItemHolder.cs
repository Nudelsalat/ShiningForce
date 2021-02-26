using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;

public class ItemHolder : MonoBehaviour {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public GameItem gameItem;
    public bool DespawnAfterUser = false;

    private bool _isInSpace = false;
    private Inventory _inventory;
    private DialogManager _dialogManager;

    void Awake() {
        if (gameItem != null) {
            var newList = Dialogue.Sentences.Select(x => x.Replace("#ITEMNAME#",
                gameItem.ItemName.AddColor(Color.green))).ToList();
            Dialogue.Sentences = newList;
        }
    }

    void Start() {
        _inventory = Inventory.Instance;
        _dialogManager = DialogManager.Instance;
    }

    void Update() {
        if (_isInSpace && Input.GetButtonUp("Interact") && !Player.InputDisabledInDialogue 
            && Player.PlayerIsInMenu == EnumMenuType.none) {
            if (gameItem != null) {
                var addedToWhom = _inventory.AddGameItem(Instantiate(gameItem));
                Dialogue.Sentences.Add(addedToWhom);
                TriggerDialogue();
                Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
                RemoveItem();
            } else {
                TriggerDialogue();
            }

            if (DespawnAfterUser) {
                Destroy(gameObject);
            }
        }
    }

    public void TriggerDialogue() {
        _dialogManager.StartDialogue(Dialogue);
        if (audioClip != null) {
            AudioManager.Instance.PlaySFX(audioClip);
        }
    }

    public void RemoveItem() {
        gameItem = null;
        audioClip = null;
        Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
        Dialogue.Sentences.Add("Nothing here...");
    }


    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("InteractionPointer")) {
            _isInSpace = true;
        }
    }
    void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.tag.Equals("InteractionPointer")) {
            _isInSpace = false;
        }
    }
}
