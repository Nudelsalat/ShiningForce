using System.Linq;
using Assets.Scripts.Dialog;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoldHolder : AbstractDialogHolder {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public int Gold;
    public string PreviousSceneName;

    private string _sceneToSave;
    private Inventory _inventory;
    private DialogManager _dialogManager;
    private PickedUpItemStorage _pickedUpItemStorage;

    void Awake() {
        if (Gold != 0) {
            var newList = Dialogue.Sentences.Select(x => x.Replace("#AMOUNT#",
                Gold.ToString())).ToList();
            Dialogue.Sentences = newList;
        }
    }

    void Start() {
        _inventory = Inventory.Instance;
        _dialogManager = DialogManager.Instance;
        _pickedUpItemStorage = PickedUpItemStorage.Instance;
        _sceneToSave = string.IsNullOrEmpty(PreviousSceneName)
            ? SceneManager.GetActiveScene().name
            : PreviousSceneName;
        if (_pickedUpItemStorage.AlreadyPickedUp(_sceneToSave, gameObject.name.GetHashCode())) {
            RemoveItem();
        }
    }

    public override void TriggerDialogue() {
        if (Gold != 0) {
            _inventory.AddGold(Gold);
            _dialogManager.StartDialogue(Dialogue);
            Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
            RemoveItem();
            if (audioClip != null) {
                AudioManager.Instance.PlaySFX(audioClip);
            } else {
                AudioManager.Instance.PlaySFX(Constants.SfxMenuDing);
            }
        } else {
            _dialogManager.StartDialogue(Dialogue);
        }
    }

    public void RemoveItem() {
        _pickedUpItemStorage.AddToPickedUpList(_sceneToSave, gameObject.name.GetHashCode());
        Gold = 0;
        Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
        Dialogue.Sentences.Add("Nothing here...");
        if (DespawnAfterUse) {
            Destroy(gameObject);
        }
    }
}
