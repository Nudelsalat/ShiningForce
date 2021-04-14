using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameData.Chests;
using Assets.Scripts.GlobalObjectScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemHolder : MonoBehaviour {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public GameItem gameItem;
    public int ID;
    public bool DespawnAfterUser = false;

    private bool _isInSpace = false;
    private Inventory _inventory;
    private DialogManager _dialogManager;
    private PickedUpItemStorage _pickedUpItemStorage;

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
        _pickedUpItemStorage = PickedUpItemStorage.Instance;

        if (_pickedUpItemStorage.AlreadyPickedUp(SceneManager.GetActiveScene().name, ID)) {
            RemoveItem();
        }
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
                if (audioClip != null) {
                    AudioManager.Instance.PlaySFX(audioClip);
                } else {
                    AudioManager.Instance.PlaySFX(Constants.SfxMenuDing);
                }
            } else {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue() {
        _dialogManager.StartDialogue(Dialogue);
    }

    public void RemoveItem() {
        _pickedUpItemStorage.AddToPickedUpList(SceneManager.GetActiveScene().name, ID);
        gameItem = null;
        Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
        Dialogue.Sentences.Add("Nothing here...");
        if (DespawnAfterUser) {
            Destroy(gameObject);
        }
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
