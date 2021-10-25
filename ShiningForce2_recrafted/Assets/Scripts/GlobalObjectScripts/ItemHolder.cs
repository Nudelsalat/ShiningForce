using System.Linq;
using Assets.Scripts.Dialog;
using Assets.Scripts.GameData.Chests;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemHolder : AbstractDialogHolder {
    //private int _dialogueCounter = 0;
    public Dialogue Dialogue;
    public AudioClip audioClip;
    public GameItem gameItem;
    public string PreviousSceneName;

    private string _sceneToSave;
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
        _sceneToSave = string.IsNullOrEmpty(PreviousSceneName)
            ? SceneManager.GetActiveScene().name
            : PreviousSceneName;
        if (_pickedUpItemStorage.AlreadyPickedUp(_sceneToSave, gameObject.name.GetHashCode())) {
            RemoveItem();
        }
    }

    public override void TriggerDialogue() {
        if (gameItem != null) {
            var addedToWhom = _inventory.AddGameItem(Instantiate(gameItem));
            Dialogue.Sentences.Add(addedToWhom);
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
        gameItem = null;
        Dialogue.Sentences.RemoveAt(Dialogue.Sentences.Count() - 1);
        Dialogue.Sentences.Add("Nothing here...");
        if (DespawnAfterUse) {
            Destroy(gameObject);
        }
    }
}
