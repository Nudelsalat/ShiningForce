using System.Collections;
using Assets.Scripts.GameData.Trigger;
using Assets.Scripts.GlobalObjectScripts;
using Assets.Scripts.LevelManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpToScene : MonoBehaviour, IEventTrigger {

    public string sceneToWarpTo;
    public int id;
    public AudioClip audioClip;

    void Start() {
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            DoWarp();
        }
    }

    public void EventTrigger() {
        DoWarp();
    }

    public void DoWarp() {
        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
        Inventory.Instance.SetWarpId(id);
        Player.InWarp = true;
        LevelManager.setLastLevelInt(SceneManager.GetActiveScene().buildIndex);
        LevelManager.setLastLevelString(SceneManager.GetActiveScene().name);
        FadeInOut.Instance.FadeOutAndThenBackIn(2.75f);
        StartCoroutine(WaitForSecToWarp(0.4f));
    }

    IEnumerator WaitForSecToWarp(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        Player.InWarp = false;
        Debug.Log($"Loading scene: {sceneToWarpTo}");
        SceneManager.LoadScene(sceneToWarpTo, LoadSceneMode.Single);
    }
}
